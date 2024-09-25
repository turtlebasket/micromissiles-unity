using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

// Integrated Air Defense System
public class IADS : MonoBehaviour {
  public enum TargetStatus { UNASSIGNED, ASSIGNED, HIT, DEGRADED, DESTROYED }

  // Look up threat status by unique threat ID
  public Dictionary<string, TargetStatus> _targetStatusDictionary;

  private List<Threat> _threats;

  private List<Interceptor> _interceptors;

  private List<Vessel> _vessels;

  public delegate void RegisterNewThreatDelegate(Threat threat);
  public event RegisterNewThreatDelegate OnRegisterNewThreat;

  void Start() {
    _threats = new List<Threat>();
  }

  public void RegisterNewThreat(Threat threat) {
    _threats.Add(threat);
    OnRegisterNewThreat?.Invoke(threat);
  }
}