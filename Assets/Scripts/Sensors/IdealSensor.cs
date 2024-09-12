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

