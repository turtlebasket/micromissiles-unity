using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public static class ConfigLoader {
    public static SimulationConfig LoadSimulationConfig(string configFileName) {
        string configFilePath = Path.Combine(Application.streamingAssetsPath, "Configs", configFileName);
        if (File.Exists(configFilePath)) {
            string json = File.ReadAllText(configFilePath);
            SimulationConfig config = JsonConvert.DeserializeObject<SimulationConfig>(json, new JsonSerializerSettings {
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            });
            return config;
        } else {
            Debug.LogError($"Configuration file not found at path: {configFilePath}");
            return null;
        }
    }

    public static StaticConfig LoadStaticConfig(string configFileName) {
        string configFilePath = Path.Combine(Application.streamingAssetsPath, "Configs/Models", configFileName);
        if (File.Exists(configFilePath)) {
            string json = File.ReadAllText(configFilePath);
            StaticConfig config = JsonConvert.DeserializeObject<StaticConfig>(json, new JsonSerializerSettings {
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            });
            return config;
        } else {
            Debug.LogError($"Static configuration file not found at path: {configFilePath}");
            return null;
        }
    }

    public static void PrintSimulationConfig(SimulationConfig config)
    {
        if (config == null)
        {
            Debug.Log("SimulationConfig is null");
            return;
        }

        Debug.Log("SimulationConfig:");
        Debug.Log($"Time Scale: {config.timeScale}");

        Debug.Log("Missile Swarm Configurations:");
        for (int i = 0; i < config.missile_swarm_configs.Count; i++)
        {
            PrintSwarmConfig(config.missile_swarm_configs[i], $"Missile Swarm {i + 1}");
        }

        Debug.Log("Threat Swarm Configurations:");
        for (int i = 0; i < config.target_swarm_configs.Count; i++)
        {
            PrintSwarmConfig(config.target_swarm_configs[i], $"Threat Swarm {i + 1}");
        }
    }

    private static void PrintSwarmConfig(SwarmConfig swarmConfig, string swarmName)
    {
        Debug.Log($"{swarmName}:");
        Debug.Log($"  Number of Agents: {swarmConfig.num_agents}");
        PrintAgentConfig(swarmConfig.agent_config);
    }

    private static void PrintAgentConfig(AgentConfig agentConfig)
    {
        Debug.Log("  Agent Configuration:");
        Debug.Log($"    Missile Type: {agentConfig.missile_type}");
        Debug.Log($"    Threat Type: {agentConfig.target_type}");
        PrintInitialState(agentConfig.initial_state);
        PrintStandardDeviation(agentConfig.standard_deviation);
        PrintDynamicConfig(agentConfig.dynamic_config);
        PrintPlottingConfig(agentConfig.plotting_config);
        PrintSubmunitionsConfig(agentConfig.submunitions_config);
    }

    private static void PrintInitialState(InitialState initialState)
    {
        Debug.Log("    Initial State:");
        Debug.Log($"      Position: {initialState.position}");
        Debug.Log($"      Rotation: {initialState.rotation}");
        Debug.Log($"      Velocity: {initialState.velocity}");
    }

    private static void PrintStandardDeviation(StandardDeviation standardDeviation)
    {
        Debug.Log("    Standard Deviation:");
        Debug.Log($"      Position: {standardDeviation.position}");
        Debug.Log($"      Velocity: {standardDeviation.velocity}");
    }

    private static void PrintDynamicConfig(DynamicConfig dynamicConfig)
    {
        Debug.Log("    Dynamic Configuration:");
        Debug.Log($"      Launch Time: {dynamicConfig.launch_config.launch_time}");
        Debug.Log($"      Sensor Type: {dynamicConfig.sensor_config.type}");
        Debug.Log($"      Sensor Frequency: {dynamicConfig.sensor_config.frequency}");
    }

    private static void PrintPlottingConfig(PlottingConfig plottingConfig)
    {
        Debug.Log("    Plotting Configuration:");
        Debug.Log($"      Color: {plottingConfig.color}");
        Debug.Log($"      Line Style: {plottingConfig.linestyle}");
        Debug.Log($"      Marker: {plottingConfig.marker}");
    }

    private static void PrintSubmunitionsConfig(SubmunitionsConfig submunitionsConfig)
    {
        if (submunitionsConfig == null)
        {
            Debug.Log("    Submunitions Configuration: None");
            return;
        }

        Debug.Log("    Submunitions Configuration:");
        Debug.Log($"      Number of Submunitions: {submunitionsConfig.num_submunitions}");
        Debug.Log($"      Launch Time: {submunitionsConfig.launch_config.launch_time}");
        PrintSubmunitionAgentConfig(submunitionsConfig.agent_config);
    }

    private static void PrintSubmunitionAgentConfig(SubmunitionAgentConfig agentConfig)
    {
        Debug.Log("      Submunition Agent Configuration:");
        Debug.Log($"        Missile Type: {agentConfig.missile_type}");
        PrintInitialState(agentConfig.initial_state);
        PrintStandardDeviation(agentConfig.standard_deviation);
        PrintDynamicConfig(agentConfig.dynamic_config);
        PrintPlottingConfig(agentConfig.plotting_config);
    }
}