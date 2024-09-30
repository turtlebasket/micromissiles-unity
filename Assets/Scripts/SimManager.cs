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

  
  private List<Interceptor> _activeInterceptors = new List<Interceptor>();
  [SerializeField]
  private List<ThreatData> _threatTable = new List<ThreatData>();
  private Dictionary<Threat, ThreatData> _threatDataMap = new Dictionary<Threat, ThreatData>();

  private List<Interceptor> _interceptorObjects = new List<Interceptor>();
  private List<Threat> _threatObjects = new List<Threat>();

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

  public List<Interceptor> GetActiveInterceptors() {
    return _activeInterceptors;
  }

  public List<Threat> GetActiveThreats() {
    return _threatTable.Where(threat => threat.Status != ThreatStatus.DESTROYED).Select(threat => threat.Threat).ToList();
  }

  public List<Agent> GetActiveAgents() {
    return _activeInterceptors.ConvertAll(interceptor => interceptor as Agent)
        .Concat(GetActiveThreats().ConvertAll(threat => threat as Agent))
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
      ResumeSimulation();
    }

  }

  public void SetTimeScale(float timeScale) {
    Time.timeScale = timeScale;
    Time.fixedDeltaTime = Time.timeScale * 0.02f;
    Time.maximumDeltaTime = Time.timeScale * 0.15f;
  }

  public void StartSimulation() {
    InitializeSimulation();
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
    List<Interceptor> missiles = new List<Interceptor>();
    // Create missiles based on config
    foreach (var swarmConfig in simulationConfig.interceptor_swarm_configs) {
      for (int i = 0; i < swarmConfig.num_agents; i++) {
        var interceptor = CreateInterceptor(swarmConfig.agent_config);
      }
    }

    List<Threat> targets = new List<Threat>();
    // Create targets based on config
    foreach (var swarmConfig in simulationConfig.threat_swarm_configs) {
      for (int i = 0; i < swarmConfig.num_agents; i++) {
        var threat = CreateThreat(swarmConfig.agent_config);
      }
    }

    _assignmentScheme = new ThreatAssignment();

    // Invoke the simulation started event to let listeners
    // know to invoke their own handler behavior
    OnSimulationStarted?.Invoke();
  }

  public void AssignInterceptorsToThreats() {
    AssignInterceptorsToThreats(_interceptorObjects);
  }

  public void RegisterInterceptorHit(Interceptor interceptor, Threat threat) {
    if (interceptor is Interceptor missileComponent) {
      _activeInterceptors.Remove(missileComponent);
    }
  }

  public void RegisterInterceptorMiss(Interceptor interceptor, Threat threat) {
    if (interceptor is Interceptor missileComponent) {
      _activeInterceptors.Remove(missileComponent);
    }
    // Remove the interceptor from the threat's assigned interceptors
    _threatDataMap[threat].RemoveInterceptor(interceptor);
  }

  public void RegisterThreatHit(Interceptor interceptor, Threat threat) {
    ThreatData threatData = _threatDataMap[threat];
    threatData.RemoveInterceptor(interceptor);
    if (threatData != null) {
      threatData.MarkDestroyed();
    }
  }

  public void RegisterThreatMiss(Interceptor interceptor, Threat threat) {
    Debug.Log($"RegisterThreatMiss: Interceptor {interceptor.name} missed threat {threat.name}");
    ThreatData threatData = _threatDataMap[threat];
    threatData.RemoveInterceptor(interceptor);
  }

  /// <summary>
  /// Assigns the specified list of missiles to available targets based on the assignment scheme.
  /// </summary>
  /// <param name="missilesToAssign">The list of missiles to assign.</param>
  public void AssignInterceptorsToThreats(List<Interceptor> missilesToAssign) {
    // Perform the assignment
    IEnumerable<IAssignment.AssignmentItem> assignments =
        _assignmentScheme.Assign(missilesToAssign, _threatTable);

    // Apply the assignments to the missiles
    foreach (var assignment in assignments) {
        assignment.Interceptor.AssignTarget(assignment.Threat);
        _threatDataMap[assignment.Threat].AssignInterceptor(assignment.Interceptor);
        Debug.Log($"Interceptor {assignment.Interceptor.name} assigned to threat {assignment.Threat.name}");
    }

    // Check if any interceptors were not assigned
    List<Interceptor> unassignedInterceptors = missilesToAssign.Where(m => !m.HasAssignedTarget()).ToList();
    
    if (unassignedInterceptors.Count > 0)
    {
        string unassignedIds = string.Join(", ", unassignedInterceptors.Select(m => m.name));
        int totalInterceptors = missilesToAssign.Count;
        int assignedInterceptors = totalInterceptors - unassignedInterceptors.Count;
        
        Debug.LogWarning($"Warning: {unassignedInterceptors.Count} out of {totalInterceptors} interceptors were not assigned to any threat. " +
                         $"Unassigned interceptor IDs: {unassignedIds}. " +
                         $"Total interceptors: {totalInterceptors}, Assigned: {assignedInterceptors}, Unassigned: {unassignedInterceptors.Count}");

        // Log information about the assignment scheme
        Debug.Log($"Current Assignment Scheme: {_assignmentScheme.GetType().Name}");
    }
  }

  /// <summary>
  /// Creates a interceptor based on the provided configuration.
  /// </summary>
  /// <param name="config">Configuration settings for the interceptor.</param>
  /// <returns>The created Interceptor instance, or null if creation failed.</returns>
  public Interceptor CreateInterceptor(AgentConfig config) {
    string prefabName = config.interceptor_type switch { InterceptorType.HYDRA_70 => "Hydra70",
                                                     InterceptorType.MICROMISSILE => "Micromissile",
                                                     _ => "Hydra70" };

    GameObject interceptorObject = CreateAgent(config, prefabName);
    if (interceptorObject == null)
      return null;

    // Interceptor-specific logic
    switch (config.dynamic_config.sensor_config.type) {
      case SensorType.IDEAL:
        interceptorObject.AddComponent<IdealSensor>();
        break;
      default:
        Debug.LogError($"Sensor type '{config.dynamic_config.sensor_config.type}' not found.");
        break;
    }

    Interceptor interceptor = interceptorObject.GetComponent<Interceptor>();
    _interceptorObjects.Add(interceptor);
    _activeInterceptors.Add(interceptor);

    // Subscribe events
    interceptor.OnInterceptHit += RegisterInterceptorHit;
    interceptor.OnInterceptMiss += RegisterInterceptorMiss;

    // Assign a unique and simple ID
    int missileId = _interceptorObjects.Count;
    interceptorObject.name = $"{config.interceptor_type}_Interceptor_{missileId}";
    return interceptorObject.GetComponent<Interceptor>();
  }

  /// <summary>
  /// Creates a threat based on the provided configuration.
  /// </summary>
  /// <param name="config">Configuration settings for the threat.</param>
  /// <returns>The created Threat instance, or null if creation failed.</returns>
  private Threat CreateThreat(AgentConfig config) {
    string prefabName = config.threat_type switch {
      ThreatType.DRONE => "Drone", ThreatType.ANTISHIP_MISSILE => "AntishipMissile",
      _ => throw new System.ArgumentException($"Unsupported threat type: {config.threat_type}")
    };
    GameObject threatObject = CreateAgent(config, prefabName);
    if (threatObject == null)
      return null;

    Threat threat = threatObject.GetComponent<Threat>();
    // Assign a unique and simple ID
    int targetId = _threatTable.Count;
    threatObject.name = $"{config.threat_type}_Target_{targetId}";

    ThreatData threatData = new ThreatData(threat, threatObject.name);
    _threatDataMap.Add(threat, threatData);
    _threatTable.Add(threatData);
    _threatObjects.Add(threat);

    // Subscribe events
    threat.OnInterceptHit += RegisterThreatHit;
    threat.OnInterceptMiss += RegisterThreatMiss;

    return threatObject.GetComponent<Threat>();
  }

  /// <summary>
  /// Creates an agent (interceptor or threat) based on the provided configuration and prefab name.
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
    agentRigidbody.linearVelocity = noisyVelocity;

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
    simulationRunning = IsSimulationRunning();

    // Clear existing missiles and targets
    foreach (var interceptor in _interceptorObjects) {
      if (interceptor != null) {
        Destroy(interceptor.gameObject);
      }
    }

    foreach (var threat in _threatObjects) {
      if (threat != null) {
        Destroy(threat.gameObject);
      }
    }

    _interceptorObjects.Clear();
    _activeInterceptors.Clear();
    _threatObjects.Clear();
    _threatTable.Clear();
    


    StartSimulation();
  }

  void Update() {
    // Check if all missiles have terminated
    bool allInterceptorsTerminated = true;
    foreach (var interceptor in _interceptorObjects) {
      if (interceptor != null && !interceptor.IsHit() && !interceptor.IsMiss()) {
        allInterceptorsTerminated = false;
        break;
      }
    }
    // If all missiles have terminated, restart the simulation
    if (allInterceptorsTerminated) {
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
