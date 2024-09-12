using UnityEngine;

public abstract class Sensor : MonoBehaviour
{
    protected Agent _agent;

    protected virtual void Start()
    {
        _agent = GetComponent<Agent>();
    }

    /// <summary>
    /// Main sensing method to gather information about a target agent.
    /// </summary>
    /// <param name="target">The agent to sense.</param>
    /// <returns>SensorOutput containing position and velocity data.</returns>
    /// <remarks>
    /// Implementers should:
    /// 1. Call SensePosition to get position data.
    /// 2. Call SenseVelocity to get velocity data.
    /// 3. Combine results into a SensorOutput struct.
    /// </remarks>
    public abstract SensorOutput Sense(Agent target);

    /// <summary>
    /// Calculates the relative position of the target agent.
    /// </summary>
    /// <param name="target">The agent to sense.</param>
    /// <returns>PositionOutput containing range, azimuth, and elevation.</returns>
    /// <remarks>
    /// Implementers should calculate:
    /// - range: Distance to the target (in unity units).
    /// - azimuth: Horizontal angle to the target (in degrees).
    ///   Positive is clockwise from the forward direction.
    /// - elevation: Vertical angle to the target (in degrees).
    ///   Positive is above the horizontal plane.
    /// </remarks>
    protected abstract PositionOutput SensePosition(Agent target);

    /// <summary>
    /// Calculates the relative velocity of the target agent.
    /// </summary>
    /// <param name="target">The agent to sense.</param>
    /// <returns>VelocityOutput containing range rate, azimuth rate, and elevation rate.</returns>
    /// <remarks>
    /// Implementers should calculate:
    /// - range: Radial velocity (closing speed) in units/second.
    ///   Positive means the target is moving away.
    /// - azimuth: Rate of change of azimuth in degrees/second.
    ///   Positive means the target is moving clockwise.
    /// - elevation: Rate of change of elevation in degrees/second.
    ///   Positive means the target is moving upwards.
    /// </remarks>
    protected abstract VelocityOutput SenseVelocity(Agent target);
}

public struct SensorOutput
{
    public PositionOutput position;
    public VelocityOutput velocity;
}

public struct PositionOutput
{
    public float range;
    public float azimuth;
    public float elevation;
}

public struct VelocityOutput
{
    public float range;
    public float azimuth;
    public float elevation;
}