using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// The threat assignment class assigns missiles to the targets based
// on the threat level of the targets.
public class ThreatAssignment : Assignment
{
    // Assign a target to each missile that has not been assigned a target yet.
    public override void Assign(List<Agent> missiles, List<Agent> targets)
    {
        List<int> assignableMissileIndices = GetAssignableMissileIndices(missiles);
        if (assignableMissileIndices.Count == 0)
        {
            return;
        }

        List<int> activeTargetIndices = GetActiveTargetIndices(targets);
        if (activeTargetIndices.Count == 0)
        {
            return;
        }

        Vector3 missilesMeanPosition = CalculateMeanPosition(missiles);
        List<ThreatInfo> threatInfos = CalculateThreatLevels(targets, activeTargetIndices, missilesMeanPosition);
        
        foreach (int missileIndex in assignableMissileIndices)
        {
            if (threatInfos.Count == 0) break;

            ThreatInfo highestThreat = threatInfos[0];
            missileToTargetAssignments.AddFirst(new AssignmentItem(missileIndex, highestThreat.TargetIndex));
            threatInfos.RemoveAt(0);
        }
    }

    private Vector3 CalculateMeanPosition(List<Agent> agents)
    {
        return agents.Aggregate(Vector3.zero, (sum, agent) => sum + agent.transform.position) / agents.Count;
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