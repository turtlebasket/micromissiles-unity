using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// The round-robin assignment class assigns missiles to the targets in a
// round-robin order.
public class RoundRobinAssignment : Assignment
{
    // Previous target index that was assigned.
    private int prevTargetIndex = -1;

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

        foreach (int missileIndex in assignableMissileIndices)
        {
            int nextActiveTargetIndex = activeTargetIndices
                .FindIndex(index => index > prevTargetIndex);

            if (nextActiveTargetIndex == -1)
            {
                nextActiveTargetIndex = 0;
            }

            int nextTargetIndex = activeTargetIndices[nextActiveTargetIndex];
            missileToTargetAssignments.AddFirst(new AssignmentItem(missileIndex, nextTargetIndex));
            prevTargetIndex = nextTargetIndex;
        }
    }
}