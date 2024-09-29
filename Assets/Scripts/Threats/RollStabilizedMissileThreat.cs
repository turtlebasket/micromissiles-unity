using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for missile targets. Uses same set of flight phases as base Hydra-70.
/// </summary>
public class RollStabilizedMissileThreat : Threat {
  protected float boostAcceleration = 20;
  protected float midcourseAcceleration = 0;
  protected float terminalAcceleration = 22;
  protected float maxAmplitude = 22;

  public RollStabilizedMissileThreat() {
    strategy = new DirectPathStrategy();
  }

  protected override void UpdateBoost(double deltaTime) {
    // The interceptor only accelerates along its roll axis (forward in Unity)
    Vector3 rollAxis = transform.forward;

    // Calculate boost acceleration
    float boostAcceleration =
        (float)(_staticConfig.boostConfig.boostAcceleration * Constants.kGravity);
    Vector3 accelerationInput = boostAcceleration * rollAxis;

    // Calculate the total acceleration
    Vector3 acceleration = CalculateAcceleration(accelerationInput);

    // Apply the acceleration force
    GetComponent<Rigidbody>().AddForce(acceleration, ForceMode.Acceleration);
  }

  protected override void UpdateMidCourse(double deltaTime) {
    Vector3 accelerationInput = Vector3.zero;
    // Calculate and set the total acceleration
    Vector3 acceleration = CalculateAcceleration(accelerationInput);
    GetComponent<Rigidbody>().AddForce(acceleration, ForceMode.Acceleration);
  }

  protected Vector3 CalculateAcceleration(Vector3 accelerationInput,
                                          bool compensateForGravity = false) {
    Vector3 gravity = Physics.gravity;
    if (compensateForGravity) {
      Vector3 gravityProjection = CalculateGravityProjectionOnPitchAndYaw();
      accelerationInput -= gravityProjection;
    }

    float airDrag = CalculateDrag();
    float liftInducedDrag = CalculateLiftInducedDrag(accelerationInput);
    float dragAcceleration = -(airDrag + liftInducedDrag);

    // Project the drag acceleration onto the forward direction
    Vector3 dragAccelerationAlongRoll = dragAcceleration * transform.forward;
    _dragAcceleration = dragAccelerationAlongRoll;

    return accelerationInput + gravity + dragAccelerationAlongRoll;
  }

  protected float CalculateMaxAcceleration() {
    float maxReferenceAcceleration =
        (float)(_staticConfig.accelerationConfig.maxReferenceAcceleration * Constants.kGravity);
    float referenceSpeed = _staticConfig.accelerationConfig.referenceSpeed;
    return Mathf.Pow(GetComponent<Rigidbody>().linearVelocity.magnitude / referenceSpeed, 2) *
           maxReferenceAcceleration;
  }

  protected Vector3 CalculateGravityProjectionOnPitchAndYaw() {
    Vector3 gravity = Physics.gravity;
    Vector3 pitchAxis = transform.right;
    Vector3 yawAxis = transform.up;

    // Project the gravity onto the pitch and yaw axes
    float gravityProjectionPitchCoefficient = Vector3.Dot(gravity, pitchAxis);
    float gravityProjectionYawCoefficient = Vector3.Dot(gravity, yawAxis);

    // Return the sum of the projections
    return gravityProjectionPitchCoefficient * pitchAxis +
           gravityProjectionYawCoefficient * yawAxis;
  }

  private float CalculateDrag() {
    float dragCoefficient = _staticConfig.liftDragConfig.dragCoefficient;
    float crossSectionalArea = _staticConfig.bodyConfig.crossSectionalArea;
    float mass = _staticConfig.bodyConfig.mass;
    float dynamicPressure = (float)GetDynamicPressure();
    float dragForce = dragCoefficient * dynamicPressure * crossSectionalArea;
    return dragForce / mass;
  }

  private float CalculateLiftInducedDrag(Vector3 accelerationInput) {
    float liftAcceleration =
        (accelerationInput - Vector3.Dot(accelerationInput, transform.up) * transform.up).magnitude;
    float liftDragRatio = _staticConfig.liftDragConfig.liftDragRatio;
    return Mathf.Abs(liftAcceleration / liftDragRatio);
  }

  // ===========================================================
  // STRATEGIES
  // ===========================================================

  /// <summary>
  /// Strategy for moving the Threat in a straight path towards its target.
  /// </summary>
  public class DirectPathStrategy : NavigationStrategy {
    DefendPoint target = new DefendPoint();

    private float _navigationGain = 3f;  // Typically 3-5
    private SensorOutput _sensorOutput;
    private Vector3 _accelerationCommand;
    private double _elapsedTime = 0;

    public override void Execute(Threat threat, List<Threat> swarmMates, FlightPhase flightPhase,
                                 List<Interceptor> interceptors, double deltaTime) {
      RollStabilizedMissileThreat missileThreat = threat as RollStabilizedMissileThreat;
      if (missileThreat == null) {
        Debug.LogError("DirectPathStrategy can only be used with RollStabilizedMissileThreat");
        return;
      }

      _elapsedTime += deltaTime;
      Vector3 accelerationInput = Vector3.zero;

      if (target != null) {
        // Correct the state of the threat model at the sensor frequency
        float sensorUpdatePeriod =
            1f / missileThreat._agentConfig.dynamic_config.sensor_config.frequency;
        if (_elapsedTime >= sensorUpdatePeriod) {
          _sensorOutput = new SensorOutput();
          missileThreat.GetComponent<Sensor>().Sense(target);
          Debug.Log(_sensorOutput.velocity.range);
          _elapsedTime = 0;
        }

        // Check whether the threat should be considered a miss
        SensorOutput sensorOutput = missileThreat.GetComponent<Sensor>().Sense(target);
        if (sensorOutput.velocity.range > 1000f) {
          missileThreat.MarkAsMiss();
        }

        // Calculate the acceleration input
        accelerationInput = CalculateAccelerationCommand(missileThreat, _sensorOutput);
      } else {
        Debug.LogError("DirectPathStrategy requires a target to be set");
      }

      // Calculate and set the total acceleration
      Vector3 acceleration =
          missileThreat.CalculateAcceleration(accelerationInput, compensateForGravity: true);
      missileThreat.GetComponent<Rigidbody>().AddForce(acceleration, ForceMode.Acceleration);
    }

    private Vector3 CalculateAccelerationCommand(Threat threat, SensorOutput sensorOutput) {
      RollStabilizedMissileThreat missileThreat = threat as RollStabilizedMissileThreat;

      // Implement Proportional Navigation guidance law
      Vector3 accelerationCommand;

      // Extract relevant information from sensor output
      float los_rate_az = sensorOutput.velocity.azimuth;
      float los_rate_el = sensorOutput.velocity.elevation;
      float closing_velocity =
          -sensorOutput.velocity
               .range;  // Negative because closing velocity is opposite to range rate

      // Navigation gain (adjust as needed)
      float N = _navigationGain;

      // Calculate acceleration commands in azimuth and elevation planes
      float acc_az = N * closing_velocity * los_rate_az;
      float acc_el = N * closing_velocity * los_rate_el;

      // Convert acceleration commands to craft body frame
      accelerationCommand =
          missileThreat.transform.right * acc_az + missileThreat.transform.up * acc_el;

      // Clamp the acceleration command to the maximum acceleration
      float maxAcceleration = missileThreat.CalculateMaxAcceleration();
      accelerationCommand = Vector3.ClampMagnitude(accelerationCommand, maxAcceleration);

      // Update the stored acceleration command for debugging
      _accelerationCommand = accelerationCommand;
      return accelerationCommand;
    }
  }

  /// <summary>
  /// Strategy for moving the Threat in a spiral towards a predefined target.
  /// </summary>
  public class SpiralStrategy : NavigationStrategy {
    private float spiralRadius;
    private float periodDistance;

    public SpiralStrategy(float spiralRadius, float periodDistance) {
      this.spiralRadius = spiralRadius;
      this.periodDistance = periodDistance;
    }

    public override void Execute(Threat threat, List<Threat> swarmMates, FlightPhase flightPhase,
                                 List<Interceptor> interceptors, double deltaTime) {
      throw new System.NotImplementedException();
    }
  }
}
