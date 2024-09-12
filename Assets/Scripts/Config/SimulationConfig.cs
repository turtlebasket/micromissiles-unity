using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SimulationConfig", menuName = "Simulation/Config", order = 1)]
public class SimulationConfig : ScriptableObject
{    [Header("Simulation Settings")]
    public float timeScale = 0.05f;

    [Header("Missile Swarm Configurations")]
    public List<SwarmConfig> missile_swarm_configs = new List<SwarmConfig>();

    [Header("Target Swarm Configurations")]
    public List<SwarmConfig> target_swarm_configs = new List<SwarmConfig>();

   
}

[System.Serializable]
public class DynamicConfig
{
    public LaunchConfig launch_config;
    public SensorConfig sensor_config;
}

[System.Serializable]
public class SwarmConfig
{
    public int num_agents;
    public AgentConfig agent_config;
}

[System.Serializable]
public class AgentConfig
{
    public MissileType missile_type;
    public TargetType target_type;
    public InitialState initial_state;
    public StandardDeviation standard_deviation;
    public DynamicConfig dynamic_config;
    public PlottingConfig plotting_config;
    public SubmunitionsConfig submunitions_config;

    public static AgentConfig FromSubmunitionAgentConfig(SubmunitionAgentConfig submunitionConfig)
    {
        return new AgentConfig
        {
            missile_type = submunitionConfig.missile_type,
            initial_state = submunitionConfig.initial_state,
            standard_deviation = submunitionConfig.standard_deviation,
            dynamic_config = submunitionConfig.dynamic_config,
            plotting_config = submunitionConfig.plotting_config,
            
            // Set other fields as needed, using default values if not present in SubmunitionAgentConfig
            target_type = TargetType.DRONE, // Or another default value
            submunitions_config = null // Or a default value if needed
        };
    }
}

[System.Serializable]
public class InitialState
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 velocity;
}

[System.Serializable]
public class StandardDeviation
{
    public Vector3 position;
    public Vector3 velocity;
}

[System.Serializable]
public class LaunchConfig
{
    public float launch_time;
}

[System.Serializable]
public class PlottingConfig
{
    public Color color;
    public LineStyle linestyle;
    public Marker marker;
}

[System.Serializable]
public class SubmunitionsConfig
{
    public int num_submunitions;
    public LaunchConfig launch_config;
    public SubmunitionAgentConfig agent_config;
}

[System.Serializable]
public class SubmunitionAgentConfig
{
    public MissileType missile_type;
    public InitialState initial_state;
    public StandardDeviation standard_deviation;
    public DynamicConfig dynamic_config;
    public PlottingConfig plotting_config;
}

[System.Serializable]
public class SensorConfig
{
    public SensorType type;
    public float frequency;
}

[System.Serializable]
public class TargetConfig
{
    public TargetType target_type;
    public InitialState initial_state;
    public PlottingConfig plotting_config;
    public string prefabName;
}

public enum MissileType
{
    HYDRA_70,
    MICROMISSILE
}

public enum TargetType
{
    DRONE,
    MISSILE
}

public enum ConfigColor
{
    BLUE,
    GREEN,
    RED
}

public enum LineStyle
{
    DOTTED,
    SOLID
}

public enum Marker
{
    TRIANGLE_UP,
    TRIANGLE_DOWN,
    SQUARE
}

public enum SensorType
{
    IDEAL
}