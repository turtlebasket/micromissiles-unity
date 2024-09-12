using UnityEngine;

public class IdealSensor : Sensor
{
    protected override void Start()
    {
        base.Start();
    }

    public override SensorOutput Sense(Agent target)
    {
        SensorOutput targetSensorOutput = new SensorOutput();

        // Sense the target's position
        PositionOutput targetPositionSensorOutput = SensePosition(target);
        targetSensorOutput.position = targetPositionSensorOutput;

        // Sense the target's velocity
        VelocityOutput targetVelocitySensorOutput = SenseVelocity(target);
        targetSensorOutput.velocity = targetVelocitySensorOutput;

        return targetSensorOutput;
    }

    protected override PositionOutput SensePosition(Agent target)
    {
        PositionOutput positionSensorOutput = new PositionOutput();

        // Calculate the relative position of the target
        Vector3 relativePosition = target.transform.position - transform.position;

        // Calculate the distance (range) to the target
        positionSensorOutput.range = relativePosition.magnitude;

        // Calculate azimuth (horizontal angle relative to forward)
        positionSensorOutput.azimuth = Vector3.SignedAngle(transform.forward, relativePosition, transform.up);

        // Calculate elevation (vertical angle relative to forward)
        Vector3 flatRelativePosition = Vector3.ProjectOnPlane(relativePosition, transform.up);
        positionSensorOutput.elevation = Vector3.SignedAngle(flatRelativePosition, relativePosition, transform.right);

        return positionSensorOutput;
    }

    // protected override VelocityOutput SenseVelocity(Agent target)
    // {
    //     VelocityOutput velocitySensorOutput = new VelocityOutput();

    //     // Calculate the relative position of the target with respect to the agent
    //     Vector3 position = _agent.transform.position;
    //     Vector3 targetPosition = target.transform.position;
    //     Vector3 targetRelativePosition = targetPosition - position;

    //     // Calculate the relative velocity of the target with respect to the agent
    //     Vector3 velocity = _agent.GetVelocity();
    //     Vector3 targetVelocity = target.GetVelocity();
    //     Vector3 targetRelativeVelocity = targetVelocity - velocity;

    //     // Project the relative velocity vector onto the relative position vector
    //     Vector3 velocityProjectionOnRelativePosition = Vector3.Project(targetRelativeVelocity, targetRelativePosition);

    //     // Determine the sign of the range rate
    //     float rangeRateSign = Vector3.Dot(velocityProjectionOnRelativePosition, targetRelativePosition) >= 0 ? 1 : -1;

    //     // Calculate the range rate
    //     velocitySensorOutput.range = rangeRateSign * velocityProjectionOnRelativePosition.magnitude;

    //     // Project the relative velocity vector onto the sphere passing through the target
    //     Vector3 velocityProjectionOnAzimuthElevationSphere = targetRelativeVelocity - velocityProjectionOnRelativePosition;

    //     // The target azimuth vector is orthogonal to the relative position vector and
    //     // points to the starboard of the target along the azimuth-elevation sphere
    //     Vector3 targetAzimuth = Vector3.Cross(targetRelativePosition, _agent.transform.forward).normalized;

    //     // The target elevation vector is orthogonal to the relative position vector
    //     // and points upwards from the target along the azimuth-elevation sphere
    //     Vector3 targetElevation = Vector3.Cross(targetAzimuth, targetRelativePosition).normalized;

    //     // If the relative position vector is parallel to the yaw or pitch axis, the
    //     // target azimuth vector or the target elevation vector will be undefined
    //     if (targetAzimuth.magnitude == 0)
    //     {
    //         // In this case, we can use the right vector as the azimuth
    //         targetAzimuth = _agent.transform.right;
    //         // And recalculate the elevation vector
    //         targetElevation = Vector3.Cross(targetAzimuth, targetRelativePosition).normalized;
    //     }

    //     else if (targetElevation.magnitude == 0)
    //     {
    //         targetElevation = Vector3.Cross(targetAzimuth, targetRelativePosition);
    //     }

    //     // Project the relative velocity vector on the azimuth-elevation sphere onto the target azimuth vector
    //     Vector3 velocityProjectionOnTargetAzimuth = Vector3.Project(velocityProjectionOnAzimuthElevationSphere, targetAzimuth);

    //     // Determine the sign of the azimuth velocity
    //     float azimuthVelocitySign = Vector3.Dot(velocityProjectionOnTargetAzimuth, targetAzimuth) >= 0 ? 1 : -1;

    //     // Calculate the time derivative of the azimuth to the target
    //     velocitySensorOutput.azimuth = azimuthVelocitySign * velocityProjectionOnTargetAzimuth.magnitude / targetRelativePosition.magnitude;

    //     // Project the velocity vector on the azimuth-elevation sphere onto the target elevation vector
    //     Vector3 velocityProjectionOnTargetElevation = velocityProjectionOnAzimuthElevationSphere - velocityProjectionOnTargetAzimuth;

    //     // Determine the sign of the elevation velocity
    //     float elevationVelocitySign = Vector3.Dot(velocityProjectionOnTargetElevation, targetElevation) >= 0 ? 1 : -1;

    //     // Calculate the time derivative of the elevation to the target
    //     velocitySensorOutput.elevation = elevationVelocitySign * velocityProjectionOnTargetElevation.magnitude / targetRelativePosition.magnitude;

    //     return velocitySensorOutput;
    // }

    protected override VelocityOutput SenseVelocity(Agent target)
    {
        VelocityOutput velocitySensorOutput = new VelocityOutput();

        // Calculate relative position and velocity
        Vector3 relativePosition = target.transform.position - transform.position;
        Vector3 relativeVelocity = target.GetVelocity() - GetComponent<Rigidbody>().velocity;

        // Calculate range rate (radial velocity)
        velocitySensorOutput.range = Vector3.Dot(relativeVelocity, relativePosition.normalized);

        // Project relative velocity onto a plane perpendicular to relative position
        Vector3 tangentialVelocity = Vector3.ProjectOnPlane(relativeVelocity, relativePosition.normalized);

        // Calculate azimuth rate
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(tangentialVelocity, transform.up);
        velocitySensorOutput.azimuth = Vector3.Dot(horizontalVelocity, transform.right) / relativePosition.magnitude;

        // Calculate elevation rate
        Vector3 verticalVelocity = Vector3.Project(tangentialVelocity, transform.up);
        velocitySensorOutput.elevation = verticalVelocity.magnitude / relativePosition.magnitude;
        if (Vector3.Dot(verticalVelocity, transform.up) < 0)
        {
            velocitySensorOutput.elevation *= -1;
        }

        return velocitySensorOutput;
    }
}

