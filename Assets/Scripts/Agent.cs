using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour {
  public enum FlightPhase { INITIALIZED, READY, BOOST, MIDCOURSE, TERMINAL, TERMINATED }

  [SerializeField]
  private FlightPhase _flightPhase = FlightPhase.INITIALIZED;

  [SerializeField]
  protected Vector3 _velocity;

  [SerializeField]
  protected Vector3 _acceleration;

  [SerializeField]
  protected Vector3 _dragAcceleration;

  [SerializeField]
  protected Agent _target;
  protected bool _isHit = false;
  protected bool _isMiss = false;

  protected AgentConfig _agentConfig;

  protected double _timeSinceLaunch = 0;
  protected double _timeInPhase = 0;

  [SerializeField]
  public string staticConfigFile = "generic_static_config.json";

  protected StaticConfig _staticConfig;

  // Define delegates
  public delegate void AgentHitEventHandler(Agent agent);
  public delegate void AgentMissEventHandler(Agent agent);

  // Define events
  public event AgentHitEventHandler OnAgentHit;
  public event AgentMissEventHandler OnAgentMiss;

  public void SetFlightPhase(FlightPhase flightPhase) {
    Debug.Log(
        $"Setting flight phase to {flightPhase} at time {SimManager.Instance.GetElapsedSimulationTime()}");
    _timeInPhase = 0;
    _flightPhase = flightPhase;
  }

  public FlightPhase GetFlightPhase() {
    return _flightPhase;
  }

  public bool HasLaunched() {
    return (_flightPhase != FlightPhase.INITIALIZED) && (_flightPhase != FlightPhase.READY);
  }

  public bool HasTerminated() {
    return _flightPhase == FlightPhase.TERMINATED;
  }

  public virtual void SetAgentConfig(AgentConfig config) {
    _agentConfig = config;
  }

  public virtual bool IsAssignable() {
    return true;
  }

  public virtual void AssignTarget(Agent target) {
    _target = target;
  }

  public Agent GetAssignedTarget() {
    return _target;
  }

  public bool HasAssignedTarget() {
    return _target != null;
  }

  public void CheckTargetHit() {
    if (HasAssignedTarget() && _target.IsHit()) {
      UnassignTarget();
    }
  }

  public virtual void UnassignTarget() {
    _target = null;
  }

  // Return whether the agent has hit or been hit.
  public bool IsHit() {
    return _isHit;
  }

  public bool IsMiss() {
    return _isMiss;
  }

  public void TerminateAgent() {
    _flightPhase = FlightPhase.TERMINATED;
    transform.position = new Vector3(0, 0, 0);
    gameObject.SetActive(false);
  }

  // Mark the agent as having hit the target or been hit.
  public void MarkAsHit() {
    _isHit = true;
    OnAgentHit?.Invoke(this);
    TerminateAgent();
  }

  public void MarkAsMiss() {
    _isMiss = true;
    if (_target != null) {
      OnAgentMiss?.Invoke(this);
      _target = null;
    }
    TerminateAgent();
  }

  public double GetSpeed() {
    return GetComponent<Rigidbody>().velocity.magnitude;
  }

  public Vector3 GetVelocity() {
    return GetComponent<Rigidbody>().velocity;
  }

  public double GetDynamicPressure() {
    var airDensity = Constants.CalculateAirDensityAtAltitude(transform.position.y);
    var flowSpeed = GetSpeed();
    return 0.5 * airDensity * (flowSpeed * flowSpeed);
  }

  protected abstract void UpdateReady(double deltaTime);
  protected abstract void UpdateBoost(double deltaTime);
  protected abstract void UpdateMidCourse(double deltaTime);

  protected virtual void Awake() {
    _staticConfig = ConfigLoader.LoadStaticConfig(staticConfigFile);
    GetComponent<Rigidbody>().mass = _staticConfig.bodyConfig.mass;
  }

  // Start is called before the first frame update
  protected virtual void Start() {
    _flightPhase = FlightPhase.READY;
  }

  // Update is called once per frame
  protected virtual void FixedUpdate() {
    _timeSinceLaunch += Time.fixedDeltaTime;
    _timeInPhase += Time.fixedDeltaTime;

    var launch_time = _agentConfig.dynamic_config.launch_config.launch_time;
    var boost_time = launch_time + _staticConfig.boostConfig.boostTime;
    double elapsedSimulationTime = SimManager.Instance.GetElapsedSimulationTime();

    if (_flightPhase == FlightPhase.TERMINATED) {
      return;
    }

    if (elapsedSimulationTime >= launch_time && _flightPhase == FlightPhase.READY) {
      SetFlightPhase(FlightPhase.BOOST);
    }
    if (_timeSinceLaunch > boost_time && _flightPhase == FlightPhase.BOOST) {
      SetFlightPhase(FlightPhase.MIDCOURSE);
    }
    AlignWithVelocity();
    switch (_flightPhase) {
      case FlightPhase.INITIALIZED:
        break;
      case FlightPhase.READY:
        UpdateReady(Time.fixedDeltaTime);
        break;
      case FlightPhase.BOOST:
        UpdateBoost(Time.fixedDeltaTime);
        break;
      case FlightPhase.MIDCOURSE:
      case FlightPhase.TERMINAL:
        UpdateMidCourse(Time.fixedDeltaTime);
        break;
      case FlightPhase.TERMINATED:
        break;
    }

    _velocity = GetComponent<Rigidbody>().velocity;
    _acceleration =
        GetComponent<Rigidbody>().GetAccumulatedForce() / GetComponent<Rigidbody>().mass;
  }

  protected virtual void AlignWithVelocity() {
    Vector3 velocity = GetVelocity();
    if (velocity.magnitude > 0.1f)  // Only align if we have significant velocity
    {
      // Create a rotation with forward along velocity and up along world up
      Quaternion targetRotation = Quaternion.LookRotation(velocity, Vector3.up);

      // Smoothly rotate towards the target rotation
      transform.rotation =
          Quaternion.RotateTowards(transform.rotation, targetRotation, 1000f * Time.deltaTime);
    }
  }
}
