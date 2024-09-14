using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// The round-robin assignment class assigns missiles to the targets in a
// round-robin order.
public class RoundRobinAssignment : IAssignment {
  // Previous target index that was assigned.
  private int prevTargetIndex = -1;

  // Assign a target to each missile that has not been assigned a target yet.
  public IEnumerable<IAssignment.AssignmentItem> Assign(List<Agent> missiles, List<Agent> targets) {
    List<IAssignment.AssignmentItem> assignments = new List<IAssignment.AssignmentItem>();
    List<int> assignableMissileIndices = IAssignment.GetAssignableMissileIndices(missiles);
    if (assignableMissileIndices.Count == 0) {
      return assignments;
    }

    List<int> activeTargetIndices = IAssignment.GetActiveTargetIndices(targets);
    if (activeTargetIndices.Count == 0) {
      return assignments;
    }

    foreach (int missileIndex in assignableMissileIndices) {
      int nextActiveTargetIndex = activeTargetIndices.FindIndex(index => index > prevTargetIndex);

      if (nextActiveTargetIndex == -1) {
        nextActiveTargetIndex = 0;
      }

      int nextTargetIndex = activeTargetIndices[nextActiveTargetIndex];
      assignments.Add(new IAssignment.AssignmentItem(missileIndex, nextTargetIndex));
      prevTargetIndex = nextTargetIndex;
    }

    return assignments;
  }
}