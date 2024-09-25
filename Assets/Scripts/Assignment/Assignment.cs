using System;
using System.Collections.Generic;
using UnityEngine;

// The assignment class is an interface for assigning a threat to each interceptor.
public interface IAssignment {
  // Assignment item type.
  // The first element corresponds to the interceptor index, and the second element
  // corresponds to the threat index.
  public struct AssignmentItem {
    public int InterceptorIndex;
    public int ThreatIndex;

    public AssignmentItem(int missileIndex, int threatIndex) {
      InterceptorIndex = missileIndex;
      ThreatIndex = threatIndex;
    }
  }

  // A list containing the interceptor-target assignments.

  // Assign a target to each interceptor that has not been assigned a target yet.
  public abstract IEnumerable<AssignmentItem> Assign(List<Agent> missiles, List<Agent> targets);

  // Get the list of assignable interceptor indices.
  protected static List<int> GetAssignableInterceptorIndices(List<Agent> missiles) {
    List<int> assignableInterceptorIndices = new List<int>();
    for (int missileIndex = 0; missileIndex < missiles.Count; missileIndex++) {
      if (missiles[missileIndex].IsAssignable()) {
        assignableInterceptorIndices.Add(missileIndex);
      }
    }
    return assignableInterceptorIndices;
  }

  // Get the list of active target indices.
  protected static List<int> GetActiveThreatIndices(List<Agent> threats) {
    List<int> activeThreatIndices = new List<int>();
    for (int threatIndex = 0; threatIndex < threats.Count; threatIndex++) {
      if (!threats[threatIndex].IsHit()) {
        activeThreatIndices.Add(threatIndex);
      }
    }
    return activeThreatIndices;
  }
}
