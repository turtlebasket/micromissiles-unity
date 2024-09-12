using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.PlasticSCM.Editor.UI;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    public enum FlightPhase {
        INITIALIZED,
        READY,
        BOOST,

        MIDCOURSE,
        TERMINAL,
        TERMINATED
    }

    protected double _elapsedTime = 0;
    
    [SerializeField]
    protected FlightPhase _flightPhase = FlightPhase.INITIALIZED;

    [SerializeField]
    protected Agent _target;
    protected bool _isHit = false;
    protected bool _isMiss = false;

    protected DynamicConfig _dynamicConfig;
    [SerializeField]
    public StaticConfig StaticConfig;

    public bool HasLaunched() {
        return (_flightPhase != FlightPhase.INITIALIZED) && (_flightPhase != FlightPhase.READY);
    }

    public bool HasTerminated() {
        return _flightPhase == FlightPhase.TERMINATED;
    }

    public virtual void SetAgentConfig(AgentConfig config) {
        _dynamicConfig = config.dynamic_config;
    }

    public virtual bool IsAssignable() {
        return true;
    }

    public virtual void AssignTarget(Agent target) 
    {
        _target = target;
    }

    public bool HasAssignedTarget() {
        return _target != null;
    }

    public void CheckTargetHit() {
        if (HasAssignedTarget() && _target.IsHit()) {
            UnassignTarget();
        }
    }

    public virtual void UnassignTarget()
    {
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
        TerminateAgent();
    }

    public void MarkAsMiss() {
        _isMiss = true;
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




    // Start is called before the first frame update
    protected virtual void Start()
    {
        _elapsedTime = 0;
        _flightPhase = FlightPhase.READY;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        var launch_time = _dynamicConfig.launch_config.launch_time;
        var boost_time = launch_time + StaticConfig.boostConfig.boostTime;
        _elapsedTime += Time.deltaTime;
        if(_elapsedTime > launch_time) {
            _flightPhase = FlightPhase.BOOST;
        }
        if(_elapsedTime > boost_time) {
            _flightPhase = FlightPhase.MIDCOURSE;
        }
        AlignWithVelocity();
        switch (_flightPhase) {
            
            case FlightPhase.INITIALIZED:
                break;
            case FlightPhase.READY:
                UpdateReady(Time.deltaTime);
                break;
            case FlightPhase.BOOST:
                UpdateBoost(Time.deltaTime);
                break;
            case FlightPhase.MIDCOURSE:
            case FlightPhase.TERMINAL:
                UpdateMidCourse(Time.deltaTime);
                break;
            case FlightPhase.TERMINATED:
                break;
        }
    }
    protected virtual void AlignWithVelocity()
    {
        Vector3 velocity = GetVelocity();
        if (velocity.magnitude > 0.1f) // Only align if we have significant velocity
        {
            // Create a rotation with forward along velocity and up along world up
            Quaternion targetRotation = Quaternion.LookRotation(velocity, Vector3.up);
            
            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 1000f * Time.deltaTime);
        }
    }

}
