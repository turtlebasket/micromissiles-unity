using UnityEngine;
using System.Collections.Generic;

public class Vessel : MonoBehaviour {
  [SerializeField]
  private List<Missile> missileInventory = new List<Missile>();

  public void AddMissile(Missile missile) {
    if (missile != null) {
      missileInventory.Add(missile);
    }
  }

  public void RemoveMissile(Missile missile) {
    missileInventory.Remove(missile);
  }

  public List<Missile> GetMissileInventory() {
    return new List<Missile>(missileInventory);
  }

  public int GetMissileCount() {
    return missileInventory.Count;
  }

  // Additional methods can be added here as needed
}
