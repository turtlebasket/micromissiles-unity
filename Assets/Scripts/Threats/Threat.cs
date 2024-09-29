using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Threat : Agent {
  /// <summary>
  /// Strategy for moving the Threat towards a predefined target.
  /// </summary>
  public abstract class NavigationStrategy {
    /// <summary>
    /// Execute one timestep of the strategy for the given threat and flight phase. Should only
    /// apply normal forces to the threat. Can also change the threat's flight phase. Should NOT
    /// manage thrust.
    /// </summary>
    /// <param name="threat">Parent Threat object</param>
    /// <param name="swarmMates">List of other threats in the swarm</param>
    /// <param name="flightPhase">Current flight phase</param>
    /// <param name="interceptors">List of active interceptors</param>
    /// <param name="deltaTime">Timestep</param>
    public abstract void Execute(Threat threat, List<Threat> swarmMates, FlightPhase flightPhase,
                                 List<Interceptor> interceptors, double deltaTime);
  }

  /// <summary>
  /// Navigation strategy that this threat will use to move towards its target.
  /// </summary>
  public NavigationStrategy strategy;

  public override bool IsAssignable() {
    return false;
  }

  protected override void Start() {
    base.Start();
  }

  protected override void FixedUpdate() {
    base.FixedUpdate();
    // NOTE: no swarm-mates for now
    strategy.Execute(this, new List<Threat>(), GetFlightPhase(),
                     SimManager.Instance.GetActiveInterceptors(), Time.fixedDeltaTime);
  }

  /// <summary>
  /// OVERRIDE: THIS SHOULD NEVER BE CALLED; threats always start in midcourse or (at the very
  /// earliest) boost phase.
  /// </summary>
  /// <param name="deltaTime"></param>
  /// <exception cref="System.NotImplementedException"></exception>
  protected override void UpdateReady(double deltaTime) {
    throw new System.NotImplementedException();
  }
}