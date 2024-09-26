using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public static class ConfigLoader {

    private static string LoadFromStreamingAssets(string relativePath)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, relativePath);

        #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX || UNITY_IOS
        if (!filePath.StartsWith("file://"))
        {
            filePath = "file://" + filePath;
        }
        #endif

        UnityWebRequest www = UnityWebRequest.Get(filePath);
        www.SendWebRequest();

        // Wait for the request to complete
        while (!www.isDone)
        {
            // You might want to yield return null here if this is called from a coroutine
        }

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error loading file at {filePath}: {www.error}");
            return null;
        }

        return www.downloadHandler.text;
    }

    public static SimulationConfig LoadSimulationConfig(string configFileName)
    {
        string relativePath = Path.Combine("Configs", configFileName);
        string fileContent = LoadFromStreamingAssets(relativePath);

        if (string.IsNullOrEmpty(fileContent))
        {
            Debug.LogError($"Failed to load SimulationConfig from {relativePath}");
            return null;
        }

        return JsonConvert.DeserializeObject<SimulationConfig>(fileContent, new JsonSerializerSettings
        {
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
        });
    }

    public static StaticConfig LoadStaticConfig(string configFileName)
    {
        string relativePath = Path.Combine("Configs/Models", configFileName);
        string fileContent = LoadFromStreamingAssets(relativePath);

        if (string.IsNullOrEmpty(fileContent))
        {
            Debug.LogError($"Failed to load StaticConfig from {relativePath}");
            return null;
        }

        return JsonConvert.DeserializeObject<StaticConfig>(fileContent, new JsonSerializerSettings
        {
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
        });
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

        Debug.Log("Interceptor Swarm Configurations:");
        for (int i = 0; i < config.interceptor_swarm_configs.Count; i++)
        {
            PrintSwarmConfig(config.interceptor_swarm_configs[i], $"Interceptor Swarm {i + 1}");
        }

        Debug.Log("Threat Swarm Configurations:");
        for (int i = 0; i < config.threat_swarm_configs.Count; i++)
        {
            PrintSwarmConfig(config.threat_swarm_configs[i], $"Threat Swarm {i + 1}");
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
        Debug.Log($"    Interceptor Type: {agentConfig.interceptor_type}");
        Debug.Log($"    Threat Type: {agentConfig.threat_type}");
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
        Debug.Log($"        Interceptor Type: {agentConfig.interceptor_type}");
        PrintInitialState(agentConfig.initial_state);
        PrintStandardDeviation(agentConfig.standard_deviation);
        PrintDynamicConfig(agentConfig.dynamic_config);
        PrintPlottingConfig(agentConfig.plotting_config);
    }
}