using UnityEngine;

public abstract class Sensor : MonoBehaviour
{
    protected Agent _agent;

    protected virtual void Start()
    {
        _agent = GetComponent<Agent>();
    }

    public abstract SensorOutput Sense(Agent target);

    protected abstract PositionOutput SensePosition(Agent target);

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