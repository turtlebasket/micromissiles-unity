using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// The threat assignment class assigns missiles to the targets based
// on the threat level of the targets.
public class ThreatAssignment : IAssignment {
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

    Vector3 positionToDefend = Vector3.zero;
    List<ThreatInfo> threatInfos =
        CalculateThreatLevels(targets, activeThreatIndices, positionToDefend);

    foreach (int missileIndex in assignableInterceptorIndices) {
      if (missiles[missileIndex].HasAssignedTarget())
        continue;
      if (threatInfos.Count == 0)
        break;

      // Find the optimal target for this interceptor based on distance and threat
      ThreatInfo optimalTarget = null;
      float optimalScore = float.MinValue;

      foreach (ThreatInfo threat in threatInfos) {
        float distance = Vector3.Distance(missiles[missileIndex].transform.position,
                                          targets[threat.TargetIndex].transform.position);
        float score = threat.ThreatLevel / distance;  // Balance threat level with proximity

        if (score > optimalScore) {
          optimalScore = score;
          optimalTarget = threat;
        }
      }

      if (optimalTarget != null) {
        assignments.Add(new IAssignment.AssignmentItem(missileIndex, optimalTarget.TargetIndex));
        threatInfos.Remove(optimalTarget);
      }
    }
    return assignments;
  }

  private List<ThreatInfo> CalculateThreatLevels(List<Agent> targets, List<int> activeThreatIndices,
                                                 Vector3 missilesMeanPosition) {
    List<ThreatInfo> threatInfos = new List<ThreatInfo>();

    foreach (int targetIndex in activeThreatIndices) {
      Agent target = targets[targetIndex];
      float distanceToMean = Vector3.Distance(target.transform.position, missilesMeanPosition);
      float velocityMagnitude = target.GetVelocity().magnitude;

      // Calculate threat level based on proximity and velocity
      float threatLevel = (1 / distanceToMean) * velocityMagnitude;

      threatInfos.Add(new ThreatInfo(targetIndex, threatLevel));
    }

    // Sort threats in descending order
    return threatInfos.OrderByDescending(t => t.ThreatLevel).ToList();
  }

  private class ThreatInfo {
    public int TargetIndex { get; }
    public float ThreatLevel { get; }

    public ThreatInfo(int targetIndex, float threatLevel) {
      TargetIndex = targetIndex;
      ThreatLevel = threatLevel;
    }
  }
}