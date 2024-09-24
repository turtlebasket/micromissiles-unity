using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


[Serializable]
public class SimulationConfig {
    [Header("Simulation Settings")]
    public float timeScale = 0.05f;

    [Header("Missile Swarm Configurations")]
    public List<SwarmConfig> missile_swarm_configs = new List<SwarmConfig>();

    [Header("Target Swarm Configurations")]
    public List<SwarmConfig> target_swarm_configs = new List<SwarmConfig>();
}

[Serializable]
public class DynamicConfig {
    public LaunchConfig launch_config;
    public SensorConfig sensor_config;
}

 [Serializable]
public class SwarmConfig {
    public int num_agents;
    public AgentConfig agent_config;
}

[Serializable]
public class AgentConfig {
    public MissileType missile_type;
    public TargetType target_type;
    public InitialState initial_state;
    public StandardDeviation standard_deviation;
    public DynamicConfig dynamic_config;
    public PlottingConfig plotting_config;
    public SubmunitionsConfig submunitions_config;

    public static AgentConfig FromSubmunitionAgentConfig(SubmunitionAgentConfig submunitionConfig) {
        return new AgentConfig {
            missile_type = submunitionConfig.missile_type,
            initial_state = submunitionConfig.initial_state,
            standard_deviation = submunitionConfig.standard_deviation,
            dynamic_config = submunitionConfig.dynamic_config,
            plotting_config = submunitionConfig.plotting_config,
            // Set other fields as needed, using default values if not present in SubmunitionAgentConfig
            target_type = TargetType.DRONE,  // Or another default value
            submunitions_config = null       // Or a default value if needed
        };
    }
}

[Serializable]
public class InitialState {
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 velocity;
}

[Serializable]
public class StandardDeviation {
    public Vector3 position;
    public Vector3 velocity;
}

[Serializable]  
public class LaunchConfig {
    public float launch_time;
}

[Serializable]
public class PlottingConfig {
    public ConfigColor color;
    public LineStyle linestyle;
    public Marker marker;
}

[Serializable]
public class SubmunitionsConfig {
    public int num_submunitions;
    public LaunchConfig launch_config;
    public SubmunitionAgentConfig agent_config;
}

[Serializable]
public class SubmunitionAgentConfig {
    public MissileType missile_type;
    public InitialState initial_state;
    public StandardDeviation standard_deviation;
    public DynamicConfig dynamic_config;
    public PlottingConfig plotting_config;
}

[Serializable]
public class SensorConfig {
    public SensorType type;
    public float frequency;
}

[Serializable]
public class TargetConfig {
    public TargetType target_type;
    public InitialState initial_state;
    public PlottingConfig plotting_config;
    public string prefabName;
}

// Enums
[JsonConverter(typeof(StringEnumConverter))]
public enum MissileType { HYDRA_70, MICROMISSILE }
[JsonConverter(typeof(StringEnumConverter))]
public enum TargetType { DRONE, MISSILE }
[JsonConverter(typeof(StringEnumConverter))]
public enum ConfigColor { BLUE, GREEN, RED }
[JsonConverter(typeof(StringEnumConverter))]
public enum LineStyle { DOTTED, SOLID }
[JsonConverter(typeof(StringEnumConverter))]
public enum Marker { TRIANGLE_UP, TRIANGLE_DOWN, SQUARE }
[JsonConverter(typeof(StringEnumConverter))]
public enum SensorType { IDEAL }