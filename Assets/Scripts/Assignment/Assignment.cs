using System;
using System.Collections.Generic;
using UnityEngine;

// The assignment class is an interface for assigning a target to each missile.
public interface IAssignment {
  // Assignment item type.
  // The first element corresponds to the missile index, and the second element
  // corresponds to the target index.
  public struct AssignmentItem {
    public int MissileIndex;
    public int TargetIndex;

    public AssignmentItem(int missileIndex, int targetIndex) {
      MissileIndex = missileIndex;
      TargetIndex = targetIndex;
    }
  }

  // A list containing the missile-target assignments.

  // Assign a target to each missile that has not been assigned a target yet.
  public abstract IEnumerable<AssignmentItem> Assign(List<Agent> missiles, List<Agent> targets);

  // Get the list of assignable missile indices.
  protected static List<int> GetAssignableMissileIndices(List<Agent> missiles) {
    List<int> assignableMissileIndices = new List<int>();
    for (int missileIndex = 0; missileIndex < missiles.Count; missileIndex++) {
      if (missiles[missileIndex].IsAssignable()) {
        assignableMissileIndices.Add(missileIndex);
      }
    }
    return assignableMissileIndices;
  }

  // Get the list of active target indices.
  protected static List<int> GetActiveTargetIndices(List<Agent> targets) {
    List<int> activeTargetIndices = new List<int>();
    for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++) {
      if (!targets[targetIndex].IsHit()) {
        activeTargetIndices.Add(targetIndex);
      }
    }
    return activeTargetIndices;
  }
}
