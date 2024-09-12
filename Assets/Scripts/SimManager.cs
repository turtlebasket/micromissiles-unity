using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimManager : MonoBehaviour
{
    [SerializeField]
    public SimulationConfig simulationConfig;

    
    private List<Missile> missiles = new List<Missile>();
    private List<Target> targets = new List<Target>();
    private float currentTime = 0f;
    private float endTime = 100f; // Set an appropriate end time
    private bool simulationRunning = false;

    private Assignment _assignment;


    void Start() {
        // Slow down time by simulationConfig.timeScale
        Time.timeScale = simulationConfig.timeScale;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        Time.maximumDeltaTime = Time.timeScale * 0.15f;
        InitializeSimulation();
        simulationRunning = true;
    }
    
    private void InitializeSimulation() 
    {
        // Create missiles based on config
        foreach (var swarmConfig in simulationConfig.missile_swarm_configs)
        {
            for (int i = 0; i < swarmConfig.num_agents; i++) {
                var missile = CreateMissile(swarmConfig.agent_config);
                missiles.Add(missile);
            }
        }

        // Create targets based on config
        foreach (var swarmConfig in simulationConfig.target_swarm_configs)
        {
            for (int i = 0; i < swarmConfig.num_agents; i++) {
                var target = CreateTarget(swarmConfig.agent_config);
                targets.Add(target);
            }
        }

        _assignment = new ThreatAssignment();
        // Perform initial assignment
        AssignMissilesToTargets();
    }

    private void AssignMissilesToTargets()
    {
        // Convert Missile and Target lists to Agent lists
        List<Agent> missileAgents = new List<Agent>(missiles.ConvertAll(m => m as Agent));
        List<Agent> targetAgents =  new List<Agent>(targets.ConvertAll(t => t as Agent));

        // Perform the assignment
        _assignment.Assign(missileAgents, targetAgents);

        // Apply the assignments to the missiles
        foreach (var assignment in _assignment.Assignments)
        {
            Missile missile = missiles[assignment.MissileIndex];
            Target target = targets[assignment.TargetIndex];
            missile.AssignTarget(target);
            Debug.Log($"Missile {missile.name} assigned to target {target.name}");  
        }
    }

    private Missile CreateMissile(AgentConfig config)
    {
        // Load the missile prefab from Resources
        GameObject missilePrefab = Resources.Load<GameObject>($"Prefabs/{config.prefabName}");
        if (missilePrefab == null)
        {
            Debug.LogError($"Missile prefab '{config.prefabName}' not found in Resources/Prefabs folder.");
            return null;
        }

        // Apply noise to the initial position
        Vector3 noiseOffset = Utilities.GenerateRandomNoise(config.standard_deviation.position);
        Vector3 noisyPosition = config.initial_state.position + noiseOffset;

        // Instantiate the missile with the noisy position
        GameObject missileObject = Instantiate(missilePrefab, noisyPosition, Quaternion.Euler(config.initial_state.rotation));

        switch(config.dynamic_config.sensor_config.type) {
            case SensorType.IDEAL:
                missileObject.AddComponent<IdealSensor>();
                break;
            default:
                Debug.LogError($"Sensor type '{config.dynamic_config.sensor_config.type}' not found.");
                break;
        }   

        // Set initial velocity
        Rigidbody missileRigidbody = missileObject.GetComponent<Rigidbody>();
        // Apply noise to the initial velocity
        Vector3 velocityNoise = Utilities.GenerateRandomNoise(config.standard_deviation.velocity);
        Vector3 noisyVelocity = config.initial_state.velocity + velocityNoise;
        missileRigidbody.velocity = noisyVelocity;


        Missile missile = missileObject.GetComponent<Missile>();
        missile.SetAgentConfig(config);
        if (missile == null)
        {
            Debug.LogError($"Missile component not found on prefab '{config.prefabName}'.");
            Destroy(missileObject);
            return null;
        }

        // Initialize missile properties
        //missile.Initialize(config);

        return missile;
    }
    private Target CreateTarget(AgentConfig config)
    {
        // Load the target prefab from Resources
        GameObject targetPrefab = Resources.Load<GameObject>($"Prefabs/{config.prefabName}");
        if (targetPrefab == null)
        {
            Debug.LogError($"Target prefab '{config.prefabName}' not found in Resources/Prefabs folder.");
            return null;
        }

        // Apply noise to the initial position
        Vector3 noiseOffset = Utilities.GenerateRandomNoise(config.standard_deviation.position);
        Vector3 noisyPosition = config.initial_state.position + noiseOffset;

        // Instantiate the target with the noisy position
        GameObject targetObject = Instantiate(targetPrefab, noisyPosition, Quaternion.Euler(config.initial_state.rotation));

        // Set initial velocity with noise
        Rigidbody targetRigidbody = targetObject.GetComponent<Rigidbody>();
        Vector3 velocityNoise = Utilities.GenerateRandomNoise(config.standard_deviation.velocity);
        Vector3 noisyVelocity = config.initial_state.velocity + velocityNoise;
        targetRigidbody.velocity = noisyVelocity;

        Target target = targetObject.GetComponent<Target>();
        target.SetAgentConfig(config);

        if (target == null)
        {
            Debug.LogError($"Target component not found on prefab '{config.prefabName}'.");
            Destroy(targetObject);
            return null;
        }

        // Initialize target properties
        //target.Initialize(config);

        return target;
    }

    private void RestartSimulation()
    {
        // Reset simulation time
        currentTime = 0f;
        simulationRunning = true;

        // Clear existing missiles and targets
        foreach (var missile in missiles)
        {
            if (missile != null)
            {
                Destroy(missile.gameObject);
            }
        }
        missiles.Clear();

        foreach (var target in targets)
        {
            if (target != null)
            {
                Destroy(target.gameObject);
            }
        }
        targets.Clear();

        InitializeSimulation();
    }

    void Update()
    {
        // Check if all missiles have terminated
        bool allMissilesTerminated = true;
        foreach (var missile in missiles)
        {
            if (missile != null && !missile.IsHit() && !missile.IsMiss())
            {
                allMissilesTerminated = false;
                break;
            }
        }   
        // If all missiles have terminated, restart the simulation
        if (allMissilesTerminated)
        {
            RestartSimulation();
        }

        if (simulationRunning && currentTime < endTime)
        {
            currentTime += Time.deltaTime;
        }
        else if (currentTime >= endTime)
        {
            simulationRunning = false;
            Debug.Log("Simulation completed.");
        }

        
    }

}
