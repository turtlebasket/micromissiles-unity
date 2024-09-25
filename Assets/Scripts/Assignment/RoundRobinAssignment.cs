using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// The round-robin assignment class assigns missiles to the targets in a
// round-robin order.
public class RoundRobinAssignment : IAssignment {
  // Previous target index that was assigned.
  private int prevTargetIndex = -1;

  // Assign a target to each interceptor that has not been assigned a target yet.
  public IEnumerable<IAssignment.AssignmentItem> Assign(List<Agent> missiles, List<Agent> targets) {
    List<IAssignment.AssignmentItem> assignments = new List<IAssignment.AssignmentItem>();
    List<int> assignableInterceptorIndices = IAssignment.GetAssignableInterceptorIndices(missiles);
    if (assignableInterceptorIndices.Count == 0) {
      return assignments;
    }

    List<int> activeThreatIndices = IAssignment.GetActiveThreatIndices(targets);
    if (activeThreatIndices.Count == 0) {
      return assignments;
    }

    foreach (int missileIndex in assignableInterceptorIndices) {
      int nextActiveTargetIndex = activeThreatIndices.FindIndex(index => index > prevTargetIndex);

      if (nextActiveTargetIndex == -1) {
        nextActiveTargetIndex = 0;
      }

      int nextTargetIndex = activeThreatIndices[nextActiveTargetIndex];
      assignments.Add(new IAssignment.AssignmentItem(missileIndex, nextTargetIndex));
      prevTargetIndex = nextTargetIndex;
    }

    return assignments;
  }
}