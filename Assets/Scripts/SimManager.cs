using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the simulation by handling missiles, targets, and their assignments.
/// Implements the Singleton pattern to ensure only one instance exists.
/// </summary>
public class SimManager : MonoBehaviour {
  /// <summary>
  /// Singleton instance of SimManager.
  /// </summary>
  public static SimManager Instance { get; private set; }

  /// <summary>
  /// Configuration settings for the simulation.
  /// </summary>
  [SerializeField]
  public SimulationConfig simulationConfig;

  private List<Missile> _missiles = new List<Missile>();
  private List<Missile> _activeMissiles = new List<Missile>();
  private List<Threat> _unassignedThreats = new List<Threat>();
  private List<Threat> _threats = new List<Threat>();
  private List<Threat> _activeThreats = new List<Threat>();
  private float _elapsedSimulationTime = 0f;
  private float endTime = 100f;  // Set an appropriate end time
  private bool simulationRunning = false;

  private IAssignment _assignmentScheme;

  public delegate void SimulationEventHandler();
  public event SimulationEventHandler OnSimulationEnded;
  public event SimulationEventHandler OnSimulationStarted;

  /// <summary>
  /// Gets the elapsed simulation time.
  /// </summary>
  /// <returns>The elapsed time in seconds.</returns>
  public double GetElapsedSimulationTime() {
    return _elapsedSimulationTime;
  }

  public List<Missile> GetActiveMissiles() {
    return _activeMissiles;
  }

  public List<Threat> GetActiveThreats() {
    return _activeThreats;
  }

  public List<Agent> GetActiveAgents() {
    return _activeMissiles.ConvertAll(missile => missile as Agent)
        .Concat(_activeThreats.ConvertAll(threat => threat as Agent))
        .ToList();
  }

  void Awake() {
    // Ensure only one instance of SimManager exists
    if (Instance == null) {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    } else {
      Destroy(gameObject);
    }
    simulationConfig = ConfigLoader.LoadSimulationConfig("1_salvo_1_hydra_7_drones.json");
    Debug.Log(simulationConfig);
  }

  void Start() {
    // Slow down time by simulationConfig.timeScale
    if (Instance == this) {
      StartSimulation();
    }
  }

  public void SetTimeScale(float timeScale) {
    Time.timeScale = timeScale;
    Time.fixedDeltaTime = Time.timeScale * 0.02f;
    Time.maximumDeltaTime = Time.timeScale * 0.15f;
  }

  public void StartSimulation() {
    InitializeSimulation();
    simulationRunning = true;
    OnSimulationStarted?.Invoke();
  }

  public void PauseSimulation() {
    SetTimeScale(0);
    simulationRunning = false;
  }

  public void ResumeSimulation() {
    SetTimeScale(simulationConfig.timeScale);
    simulationRunning = true;
  }

  public bool IsSimulationRunning() {
    return simulationRunning;
  }

  private void InitializeSimulation() {
    List<Missile> missiles = new List<Missile>();
    // Create missiles based on config
    foreach (var swarmConfig in simulationConfig.missile_swarm_configs) {
      for (int i = 0; i < swarmConfig.num_agents; i++) {
        var missile = CreateMissile(swarmConfig.agent_config);
        missile.OnAgentHit += RegisterMissileHit;
        missile.OnAgentMiss += RegisterMissileMiss;
      }
    }

    List<Threat> targets = new List<Threat>();
    // Create targets based on config
    foreach (var swarmConfig in simulationConfig.target_swarm_configs) {
      for (int i = 0; i < swarmConfig.num_agents; i++) {
        var threat = CreateThreat(swarmConfig.agent_config);
        threat.OnAgentHit += RegisterThreatHit;
        threat.OnAgentMiss += RegisterThreatMiss;
      }
    }

    _assignmentScheme = new ThreatAssignment();

    // Invoke the simulation started event to let listeners
    // know to invoke their own handler behavior
    OnSimulationStarted?.Invoke();
  }

  public void AssignMissilesToThreats() {
    AssignMissilesToThreats(_missiles);
  }

  public void RegisterMissileHit(Agent missile) {
    if (missile is Missile missileComponent) {
      _activeMissiles.Remove(missileComponent);
    }
  }

  public void RegisterMissileMiss(Agent missile) {
    if (missile is Missile missileComponent) {
      _activeMissiles.Remove(missileComponent);
    }
  }

  public void RegisterThreatHit(Agent threat) {
    if (threat is Threat targetComponent) {
      _activeThreats.Remove(targetComponent);
    }
  }

  public void RegisterThreatMiss(Agent threat) {
    if (threat is Threat targetComponent) {
      _unassignedThreats.Add(targetComponent);
    }
  }

  /// <summary>
  /// Assigns the specified list of missiles to available targets based on the assignment scheme.
  /// </summary>
  /// <param name="missilesToAssign">The list of missiles to assign.</param>
  public void AssignMissilesToThreats(List<Missile> missilesToAssign) {
    // Convert Missile and Threat lists to Agent lists
    List<Agent> missileAgents = new List<Agent>(missilesToAssign.ConvertAll(m => m as Agent));
    // Convert Threat list to Agent list, excluding already assigned targets
    List<Agent> targetAgents = _unassignedThreats.ToList<Agent>();

    // Perform the assignment
    IEnumerable<IAssignment.AssignmentItem> assignments =
        _assignmentScheme.Assign(missileAgents, targetAgents);

    // Apply the assignments to the missiles
    foreach (var assignment in assignments) {
      if (assignment.MissileIndex < missilesToAssign.Count) {
        Missile missile = missilesToAssign[assignment.MissileIndex];
        Threat threat = _unassignedThreats[assignment.ThreatIndex];
        missile.AssignTarget(threat);
        Debug.Log($"Missile {missile.name} assigned to threat {threat.name}");
      }
    }
    // TODO this whole function should be optimized
    _unassignedThreats.RemoveAll(
        threat => missilesToAssign.Any(missile => missile.GetAssignedTarget() == threat));
  }

  /// <summary>
  /// Creates a missile based on the provided configuration.
  /// </summary>
  /// <param name="config">Configuration settings for the missile.</param>
  /// <returns>The created Missile instance, or null if creation failed.</returns>
  public Missile CreateMissile(AgentConfig config) {
    string prefabName = config.missile_type switch { MissileType.HYDRA_70 => "Hydra70",
                                                     MissileType.MICROMISSILE => "Micromissile",
                                                     _ => "Hydra70" };

    GameObject missileObject = CreateAgent(config, prefabName);
    if (missileObject == null)
      return null;

    // Missile-specific logic
    switch (config.dynamic_config.sensor_config.type) {
      case SensorType.IDEAL:
        missileObject.AddComponent<IdealSensor>();
        break;
      default:
        Debug.LogError($"Sensor type '{config.dynamic_config.sensor_config.type}' not found.");
        break;
    }

    _missiles.Add(missileObject.GetComponent<Missile>());
    _activeMissiles.Add(missileObject.GetComponent<Missile>());

    // Assign a unique and simple ID
    int missileId = _missiles.Count;
    missileObject.name = $"{config.missile_type}_Missile_{missileId}";
    return missileObject.GetComponent<Missile>();
  }

  /// <summary>
  /// Creates a threat based on the provided configuration.
  /// </summary>
  /// <param name="config">Configuration settings for the threat.</param>
  /// <returns>The created Threat instance, or null if creation failed.</returns>
  private Threat CreateThreat(AgentConfig config) {
    string prefabName = config.target_type switch {
      ThreatType.DRONE => "DroneTarget", ThreatType.MISSILE => "MissileTarget",
      _ => throw new System.ArgumentException($"Unsupported threat type: {config.target_type}")
    };
    GameObject threatObject = CreateAgent(config, prefabName);
    if (threatObject == null)
      return null;

    _threats.Add(threatObject.GetComponent<Threat>());
    _activeThreats.Add(threatObject.GetComponent<Threat>());
    _unassignedThreats.Add(threatObject.GetComponent<Threat>());

    // Assign a unique and simple ID
    int targetId = _threats.Count;
    threatObject.name = $"{config.target_type}_Target_{targetId}";
    return threatObject.GetComponent<Threat>();
  }

  /// <summary>
  /// Creates an agent (missile or threat) based on the provided configuration and prefab name.
  /// </summary>
  /// <param name="config">Configuration settings for the agent.</param>
  /// <param name="prefabName">Name of the prefab to instantiate.</param>
  /// <returns>The created GameObject instance, or null if creation failed.</returns>
  public GameObject CreateAgent(AgentConfig config, string prefabName) {
    GameObject prefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
    if (prefab == null) {
      Debug.LogError($"Prefab '{prefabName}' not found in Resources/Prefabs folder.");
      return null;
    }

    Vector3 noiseOffset = Utilities.GenerateRandomNoise(config.standard_deviation.position);
    Vector3 noisyPosition = config.initial_state.position + noiseOffset;

    GameObject agentObject =
        Instantiate(prefab, noisyPosition, Quaternion.Euler(config.initial_state.rotation));

    Rigidbody agentRigidbody = agentObject.GetComponent<Rigidbody>();
    Vector3 velocityNoise = Utilities.GenerateRandomNoise(config.standard_deviation.velocity);
    Vector3 noisyVelocity = config.initial_state.velocity + velocityNoise;
    agentRigidbody.velocity = noisyVelocity;

    agentObject.GetComponent<Agent>().SetAgentConfig(config);

    return agentObject;
  }

  public void LoadNewConfig(string configFileName) {
    simulationConfig = ConfigLoader.LoadSimulationConfig(configFileName);
    if (simulationConfig != null) {
      Debug.Log($"Loaded new configuration: {configFileName}");
      RestartSimulation();
    } else {
      Debug.LogError($"Failed to load configuration: {configFileName}");
    }
  }

  public void RestartSimulation() {
    OnSimulationEnded?.Invoke();
    Debug.Log("Simulation ended");
    // Reset simulation time
    _elapsedSimulationTime = 0f;
    simulationRunning = true;

    // Clear existing missiles and targets
    foreach (var missile in _missiles) {
      if (missile != null) {
        Destroy(missile.gameObject);
      }
    }

    foreach (var threat in _threats) {
      if (threat != null) {
        Destroy(threat.gameObject);
      }
    }

    _missiles.Clear();
    _threats.Clear();
    _unassignedThreats.Clear();

    StartSimulation();
  }

  void Update() {
    // Check if all missiles have terminated
    bool allMissilesTerminated = true;
    foreach (var missile in _missiles) {
      if (missile != null && !missile.IsHit() && !missile.IsMiss()) {
        allMissilesTerminated = false;
        break;
      }
    }
    // If all missiles have terminated, restart the simulation
    if (allMissilesTerminated) {
      RestartSimulation();
    }

    if (simulationRunning && _elapsedSimulationTime < endTime) {
      _elapsedSimulationTime += Time.deltaTime;
    } else if (_elapsedSimulationTime >= endTime) {
      simulationRunning = false;
      Debug.Log("Simulation completed.");
    }
  }
}
