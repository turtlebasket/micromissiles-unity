using UnityEngine;

/// <summary>
/// Static targetable point to defend
/// </summary>
public class DefendPoint : Agent {
  // Currently just initializes to the origin
  public DefendPoint() {
    // Set the initial state
    this.transform.position = Vector3.zero;
  }

  protected override void FixedUpdate() {
    return;
  }

  protected override void UpdateBoost(double deltaTime) {
    return;
  }

  protected override void UpdateMidCourse(double deltaTime) {
    return;
  }

  protected override void UpdateReady(double deltaTime) {
    return;
  }
}