using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// The threat assignment class assigns missiles to the targets based
// on the threat level of the targets.
public class ThreatAssignment : IAssignment
{
    // Assign a target to each missile that has not been assigned a target yet.
    public IEnumerable<IAssignment.AssignmentItem> Assign(List<Agent> missiles, List<Agent> targets)
    {

        List<IAssignment.AssignmentItem> assignments = new List<IAssignment.AssignmentItem>();

        List<int> assignableMissileIndices = IAssignment.GetAssignableMissileIndices(missiles);
        if (assignableMissileIndices.Count == 0)
        {
            return assignments;
        }

        List<int> activeTargetIndices = IAssignment.GetActiveTargetIndices(targets);
        if (activeTargetIndices.Count == 0)
        {
            return assignments;
        }

        Vector3 positionToDefend = Vector3.zero;
        List<ThreatInfo> threatInfos = CalculateThreatLevels(targets, activeTargetIndices, positionToDefend);
        
        foreach (int missileIndex in assignableMissileIndices)
        {
            if (missiles[missileIndex].HasAssignedTarget()) continue;
            if (threatInfos.Count == 0) break;

            // Find the optimal target for this missile based on distance and threat
            ThreatInfo optimalTarget = null;
            float optimalScore = float.MinValue;

            foreach (ThreatInfo threat in threatInfos)
            {
                float distance = Vector3.Distance(missiles[missileIndex].transform.position, targets[threat.TargetIndex].transform.position);
                float score = threat.ThreatLevel / distance; // Balance threat level with proximity

                if (score > optimalScore)
                {
                    optimalScore = score;
                    optimalTarget = threat;
                }
            }

            if (optimalTarget != null)
            {
                assignments.Add(new IAssignment.AssignmentItem(missileIndex, optimalTarget.TargetIndex));
                threatInfos.Remove(optimalTarget);
            }
        }
        return assignments;
    }


    private List<ThreatInfo> CalculateThreatLevels(List<Agent> targets, List<int> activeTargetIndices, Vector3 missilesMeanPosition)
    {
        List<ThreatInfo> threatInfos = new List<ThreatInfo>();

        foreach (int targetIndex in activeTargetIndices)
        {
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

    private class ThreatInfo
    {
        public int TargetIndex { get; }
        public float ThreatLevel { get; }

        public ThreatInfo(int targetIndex, float threatLevel)
        {
            TargetIndex = targetIndex;
            ThreatLevel = threatLevel;
        }
    }
}