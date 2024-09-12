using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Micromissile : Missile
{
    [SerializeField] private float navigationGain = 10f; // Typically 3-5
    [SerializeField] private bool _showDebugVectors = true;

    private Vector3 _previousLOS;
    private Vector3 _accelerationCommand;
    private float _lastUpdateTime;
    protected override void UpdateMidCourse(double deltaTime)
    {
        Vector3 accelerationInput = Vector3.zero;
        if (HasAssignedTarget())
        {
            // Update the target model (assuming we have a target model)
            // TODO: Implement target model update logic

            // Correct the state of the target model at the sensor frequency
            float sensorUpdatePeriod = 1f / _dynamicConfig.sensor_config.frequency;
            if (_elapsedTime - _sensorUpdateTime >= sensorUpdatePeriod)
            {
                // TODO: Implement guidance filter to estimate state from sensor output
                // For now, we'll use the target's actual state
                // targetModel.SetState(_target.GetState());
                _sensorUpdateTime = (float)_elapsedTime;
            }

            // Sense the target
            SensorOutput sensorOutput = GetComponent<Sensor>().Sense(_target);
            if(sensorOutput.velocity.range > 1000f) {
                this.MarkAsMiss();
                _target.MarkAsMiss();
            }

            // Calculate the acceleration input
            accelerationInput = CalculateAccelerationCommand(sensorOutput);
        }

        // Calculate and set the total acceleration
        Vector3 acceleration = CalculateAcceleration(accelerationInput, compensateForGravity: true);
        GetComponent<Rigidbody>().AddForce(acceleration, ForceMode.Acceleration);

        if (_showDebugVectors)
        {
            DrawDebugVectors();
        }


    }
    private Vector3 CalculateAccelerationCommand(SensorOutput sensorOutput)
    {
        // Implement Proportional Navigation guidance law
        Vector3 accelerationCommand = Vector3.zero;

        // Extract relevant information from sensor output
        float los_rate_az = sensorOutput.velocity.azimuth;
        float los_rate_el = sensorOutput.velocity.elevation;
        float closing_velocity = -sensorOutput.velocity.range; // Negative because closing velocity is opposite to range rate

        // Navigation gain (adjust as needed)
        float N = navigationGain;

        // Calculate acceleration commands in azimuth and elevation planes
        float acc_az = N * closing_velocity * los_rate_az;
        float acc_el = N * closing_velocity * los_rate_el;

        // Convert acceleration commands to missile body frame
        accelerationCommand = transform.right * acc_az + transform.up * acc_el;

        // Clamp the acceleration command to the maximum acceleration
        float maxAcceleration = CalculateMaxAcceleration();
        accelerationCommand = Vector3.ClampMagnitude(accelerationCommand, maxAcceleration);

        // Update the stored acceleration command for debugging
        _accelerationCommand = accelerationCommand;
        return accelerationCommand;
    }
    private void DrawDebugVectors()
    {
        if (_target != null)
        {
            // Line of sight
            Debug.DrawLine(transform.position, _target.transform.position, Color.white);

            // Velocity vector
            Debug.DrawRay(transform.position, GetVelocity()*0.01f, Color.blue);

            // Acceleration input
            Debug.DrawRay(transform.position, _accelerationCommand*1f, Color.green);

            // Current forward direction
            Debug.DrawRay(transform.position, transform.forward * 5f, Color.yellow);

            // Pitch axis (right)
            Debug.DrawRay(transform.position, transform.right * 5f, Color.red);

            // Yaw axis (up)
            Debug.DrawRay(transform.position, transform.up * 5f, Color.magenta);
        }
    }
}
