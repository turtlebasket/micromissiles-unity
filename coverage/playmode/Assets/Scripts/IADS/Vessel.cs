using UnityEngine;
using System.Collections.Generic;

public class Vessel : MonoBehaviour {
  [SerializeField]
  private List<Interceptor> missileInventory = new List<Interceptor>();

  public void AddInterceptor(Interceptor interceptor) {
    if (interceptor != null) {
      missileInventory.Add(interceptor);
    }
  }

  public void RemoveInterceptor(Interceptor interceptor) {
    missileInventory.Remove(interceptor);
  }

  public List<Interceptor> GetInterceptorInventory() {
    return new List<Interceptor>(missileInventory);
  }

  public int GetInterceptorCount() {
    return missileInventory.Count;
  }

  // Additional methods can be added here as needed
}
