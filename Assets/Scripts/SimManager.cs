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
  private List<Target> _unassignedTargets = new List<Target>();
  private List<Target> _targets = new List<Target>();
  private float _elapsedSimulationTime = 0f;
  private float endTime = 100f;  // Set an appropriate end time
  private bool simulationRunning = false;

  private IAssignment _assignmentScheme;

  /// <summary>
  /// Gets the elapsed simulation time.
  /// </summary>
  /// <returns>The elapsed time in seconds.</returns>
  public double GetElapsedSimulationTime() {
    return _elapsedSimulationTime;
  }

  void Awake() {
    // Ensure only one instance of SimManager exists
    if (Instance == null) {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    } else {
      Destroy(gameObject);
    }
  }

  void Start() {
    // Slow down time by simulationConfig.timeScale
    if (Instance == this) {
      Time.timeScale = simulationConfig.timeScale;
      Time.fixedDeltaTime = Time.timeScale * 0.02f;
      Time.maximumDeltaTime = Time.timeScale * 0.15f;
      InitializeSimulation();
      simulationRunning = true;
    }
  }

  private void InitializeSimulation() {
    List<Missile> missiles = new List<Missile>();
    // Create missiles based on config
    foreach (var swarmConfig in simulationConfig.missile_swarm_configs) {
      for (int i = 0; i < swarmConfig.num_agents; i++) {
        var missile = CreateMissile(swarmConfig.agent_config);
      }
    }

    List<Target> targets = new List<Target>();
    // Create targets based on config
    foreach (var swarmConfig in simulationConfig.target_swarm_configs) {
      for (int i = 0; i < swarmConfig.num_agents; i++) {
        var target = CreateTarget(swarmConfig.agent_config);
      }
    }

    _assignmentScheme = new ThreatAssignment();
  }
  
  public void AssignMissilesToTargets() {
    AssignMissilesToTargets(_missiles);
  }

  public void RegisterTargetMiss(Target target) {
    _unassignedTargets.Add(target);
  }

  /// <summary>
  /// Assigns the specified list of missiles to available targets based on the assignment scheme.
  /// </summary>
  /// <param name="missilesToAssign">The list of missiles to assign.</param>
  public void AssignMissilesToTargets(List<Missile> missilesToAssign) {
    // Convert Missile and Target lists to Agent lists
    List<Agent> missileAgents = new List<Agent>(missilesToAssign.ConvertAll(m => m as Agent));
    // Convert Target list to Agent list, excluding already assigned targets
    List<Agent> targetAgents = _unassignedTargets.ToList<Agent>();

    // Perform the assignment
    IEnumerable<IAssignment.AssignmentItem> assignments =
        _assignmentScheme.Assign(missileAgents, targetAgents);

    // Apply the assignments to the missiles
    foreach (var assignment in assignments) {
      if (assignment.MissileIndex < missilesToAssign.Count) {
        Missile missile = missilesToAssign[assignment.MissileIndex];
        Target target = _unassignedTargets[assignment.TargetIndex];
        missile.AssignTarget(target);
        Debug.Log($"Missile {missile.name} assigned to target {target.name}");
      }
    }
    // TODO this whole function should be optimized
    _unassignedTargets.RemoveAll(
        target => missilesToAssign.Any(missile => missile.GetAssignedTarget() == target));
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

    // Missile missile = missileObject.GetComponent<Missile>();
    // if (missile == null)
    // {
    //     Debug.LogError($"Missile component not found on prefab '{prefabName}'.");
    //     Destroy(missileObject);
    //     return null;
    // }

    // missile.SetAgentConfig(config);
    _missiles.Add(missileObject.GetComponent<Missile>());
    // Assign a unique and simple target ID
    int missileId = _missiles.Count;
    missileObject.name = $"{config.missile_type}_Missile_{missileId}";
    return missileObject.GetComponent<Missile>();
  }

  /// <summary>
  /// Creates a target based on the provided configuration.
  /// </summary>
  /// <param name="config">Configuration settings for the target.</param>
  /// <returns>The created Target instance, or null if creation failed.</returns>
  private Target CreateTarget(AgentConfig config) {
    string prefabName = config.target_type switch {
      TargetType.DRONE => "DroneTarget", TargetType.MISSILE => "MissileTarget",
      _ => throw new System.ArgumentException($"Unsupported target type: {config.target_type}")
    };
    GameObject targetObject = CreateAgent(config, prefabName);
    if (targetObject == null)
      return null;

    // Target target = targetObject.GetComponent<Target>();
    // if (target == null)
    // {
    //     Debug.LogError($"Target component not found on prefab '{config.prefabName}'.");
    //     Destroy(targetObject);
    //     return null;
    // }

    // target.SetAgentConfig(config);
    _targets.Add(targetObject.GetComponent<Target>());
    _unassignedTargets.Add(targetObject.GetComponent<Target>());
    // Assign a unique and simple target ID
    int targetId = _targets.Count;
    targetObject.name = $"{config.target_type}_Target_{targetId}";
    return targetObject.GetComponent<Target>();
  }

  /// <summary>
  /// Creates an agent (missile or target) based on the provided configuration and prefab name.
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


  private void RestartSimulation() {
    // Reset simulation time
    _elapsedSimulationTime = 0f;
    simulationRunning = true;

    // Clear existing missiles and targets
    foreach (var missile in _missiles) {
      if (missile != null) {
        Destroy(missile.gameObject);
      }
    }

    foreach (var target in _targets) {
      if (target != null) {
        Destroy(target.gameObject);
      }
    }

    _missiles.Clear();
    _targets.Clear();
    _unassignedTargets.Clear();

    InitializeSimulation();
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
