using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Target : Agent {
  public override bool IsAssignable() {
    return false;
  }

  protected override void Start() {
    base.Start();
  }

  protected override void FixedUpdate() {
    base.FixedUpdate();
  }
}