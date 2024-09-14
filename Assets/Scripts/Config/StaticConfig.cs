using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StaticConfig {
  [System.Serializable]
  public class AccelerationConfig {
    [Tooltip("Maximum reference acceleration")]
    public float maxReferenceAcceleration = 300f;
    [Tooltip("Reference speed")]
    public float referenceSpeed = 1000f;
  }

  [System.Serializable]
  public class BoostConfig {
    [Tooltip("Boost time in seconds")]
    public float boostTime = 0.3f;
    [Tooltip("Boost acceleration")]
    public float boostAcceleration = 350f;
  }

  [System.Serializable]
  public class LiftDragConfig {
    [Tooltip("Lift coefficient")]
    public float liftCoefficient = 0.2f;
    [Tooltip("Drag coefficient")]
    public float dragCoefficient = 0.7f;
    [Tooltip("Lift to drag ratio")]
    public float liftDragRatio = 5f;
  }

  [System.Serializable]
  public class BodyConfig {
    [Tooltip("Mass in kg")]
    public float mass = 0.37f;
    [Tooltip("Cross-sectional area in m²")]
    public float crossSectionalArea = 3e-4f;
    [Tooltip("Fin area in m²")]
    public float finArea = 6e-4f;
    [Tooltip("Body area in m²")]
    public float bodyArea = 1e-2f;
  }

  [System.Serializable]
  public class HitConfig {
    [Tooltip("Hit radius")]
    public float hitRadius = 1f;
    [Tooltip("Kill probability")]
    public float killProbability = 0.9f;
  }

  [Header("Acceleration Configuration")]
  public AccelerationConfig accelerationConfig;

  [Header("Boost Configuration")]
  public BoostConfig boostConfig;

  [Header("Lift and Drag Configuration")]
  public LiftDragConfig liftDragConfig;

  [Header("Body Configuration")]
  public BodyConfig bodyConfig;

  [Header("Hit Configuration")]
  public HitConfig hitConfig;
}