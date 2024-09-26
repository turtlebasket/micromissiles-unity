using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for missile targets. Uses same set of flight phases as base Hydra-70.
/// </summary>
public class MissileThreat : Threat
{
  protected float boostAcceleration = 20;
  protected float midcourseAcceleration = 5;

  protected override void UpdateReady(double deltaTime)
  {
    // if in ready phase, just set to boost phase immediately
    SetFlightPhase(FlightPhase.BOOST);
  }

  protected override void UpdateBoost(double deltaTime)
  {
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

  protected override void UpdateMidCourse(double deltaTime)
  {
    Vector3 accelerationInput = Vector3.zero;
    // Calculate and set the total acceleration
    Vector3 acceleration = CalculateAcceleration(accelerationInput);
    GetComponent<Rigidbody>().AddForce(acceleration, ForceMode.Acceleration);
  }

  protected Vector3 CalculateAcceleration(Vector3 accelerationInput,
                                          bool compensateForGravity = false)
  {
    Vector3 gravity = Physics.gravity;
    if (compensateForGravity)
    {
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

  protected float CalculateMaxAcceleration()
  {
    float maxReferenceAcceleration =
        (float)(_staticConfig.accelerationConfig.maxReferenceAcceleration * Constants.kGravity);
    float referenceSpeed = _staticConfig.accelerationConfig.referenceSpeed;
    return Mathf.Pow(GetComponent<Rigidbody>().linearVelocity.magnitude / referenceSpeed, 2) *
           maxReferenceAcceleration;
  }

  protected Vector3 CalculateGravityProjectionOnPitchAndYaw()
  {
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

  private float CalculateDrag()
  {
    float dragCoefficient = _staticConfig.liftDragConfig.dragCoefficient;
    float crossSectionalArea = _staticConfig.bodyConfig.crossSectionalArea;
    float mass = _staticConfig.bodyConfig.mass;
    float dynamicPressure = (float)GetDynamicPressure();
    float dragForce = dragCoefficient * dynamicPressure * crossSectionalArea;
    return dragForce / mass;
  }

  private float CalculateLiftInducedDrag(Vector3 accelerationInput)
  {
    float liftAcceleration =
        (accelerationInput - Vector3.Dot(accelerationInput, transform.up) * transform.up).magnitude;
    float liftDragRatio = _staticConfig.liftDragConfig.liftDragRatio;
    return Mathf.Abs(liftAcceleration / liftDragRatio);
  }

}
