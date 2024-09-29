using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedWingMissileThreat : Threat {
  protected override void UpdateBoost(double deltaTime) {
    throw new System.NotImplementedException();
  }

  protected override void UpdateMidCourse(double deltaTime) {
    throw new System.NotImplementedException();
  }

  /// <summary>
  /// Strategy for moving the Threat in a straight path towards its target.
  /// </summary>
  public class DirectPathStrategy : NavigationStrategy {
    public override void Execute(Threat threat, List<Threat> swarmMates, FlightPhase flightPhase,
                                 List<Interceptor> interceptors, double deltaTime) {
      throw new System.NotImplementedException();
    }
  }

  /// <summary>
  /// Strategy for moving the Threat in an S-curve towards a predefined target.
  /// </summary>
  public class SlalomStrategy : NavigationStrategy {
    private float maxAmplitude;
    private float periodDistance;

    public SlalomStrategy(float maxAmplitude, float periodDistance) {
      this.maxAmplitude = maxAmplitude;
      this.periodDistance = periodDistance;
    }

    public override void Execute(Threat threat, List<Threat> swarmMates, FlightPhase flightPhase,
                                 List<Interceptor> interceptors, double deltaTime) {
      throw new System.NotImplementedException();
    }
  }
}
