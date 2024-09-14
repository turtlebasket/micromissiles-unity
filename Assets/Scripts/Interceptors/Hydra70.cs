using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Hydra70 : Missile {
  private Vector3 _acceleration;
  private bool _submunitionsLaunched = false;

  protected override void Update() {
    base.Update();

    // Check if it's time to launch submunitions
    if (!_submunitionsLaunched &&
        (GetFlightPhase() == FlightPhase.MIDCOURSE || GetFlightPhase() == FlightPhase.BOOST) &&
        SimManager.Instance.GetElapsedSimulationTime() >=
            _agentConfig.submunitions_config.launch_config.launch_time) {
      SpawnSubmunitions();
      _submunitionsLaunched = true;
    }
  }

  protected override void UpdateMidCourse(double deltaTime) {
    Vector3 accelerationInput = Vector3.zero;
    // Calculate and set the total acceleration
    Vector3 acceleration = CalculateAcceleration(accelerationInput);
    GetComponent<Rigidbody>().AddForce(acceleration, ForceMode.Acceleration);
    _acceleration = acceleration;
  }

  protected override void DrawDebugVectors() {
    base.DrawDebugVectors();
    if (_acceleration != null) {
      Debug.DrawRay(transform.position, _acceleration * 1f, Color.green);
    }
  }

  public void SpawnSubmunitions() {
    Debug.Log("Spawning submunitions");
    // print the callstack
    Debug.Log(new System.Diagnostics.StackTrace().ToString());
    List<Missile> submunitions = new List<Missile>();
    switch (_agentConfig.submunitions_config.agent_config.missile_type) {
      case MissileType.MICROMISSILE:
        for (int i = 0; i < _agentConfig.submunitions_config.num_submunitions; i++) {
          AgentConfig convertedConfig =
              AgentConfig.FromSubmunitionAgentConfig(_agentConfig.submunitions_config.agent_config);

          convertedConfig.initial_state.position = transform.position;
          convertedConfig.initial_state.velocity = GetComponent<Rigidbody>().velocity;
          Missile submunition = SimManager.Instance.CreateMissile(convertedConfig);
          submunitions.Add(submunition);
          Debug.Log("Created submunition");
        }
        break;
    }
    SimManager.Instance.AssignMissilesToTargets(submunitions);
  }
}
