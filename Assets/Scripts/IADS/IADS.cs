using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

// Integrated Air Defense System
public class IADS : MonoBehaviour {
  public enum TargetStatus { UNASSIGNED, ASSIGNED, HIT, DEGRADED, DESTROYED }

  // Look up target status by unique target ID
  public Dictionary<string, TargetStatus> _targetStatusDictionary;

  private List<Target> _targets;

  private List<Missile> _missiles;

  private List<Vessel> _vessels;

  public delegate void RegisterNewTargetDelegate(Target target);
  public event RegisterNewTargetDelegate OnRegisterNewTarget;

  void Start() {
    _targets = new List<Target>();
  }

  public void RegisterNewTarget(Target target) {
    _targets.Add(target);
    OnRegisterNewTarget?.Invoke(target);
  }
}