using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleData : MonoBehaviour {
  public float topSpeed = 100f; // kph
  public float boostAmount = 200f;
  public float boostCapacity = 5f;
  [SerializeField]
  private float remainingBoost;
  public float brakes = 5f; // used with braking rigidbody
  public float weight = 50f;
  public float handling = 150f;
  public float turnRadius = 30f;
  public float maxClimbAngle = 40f;
  public Transform centerOfMass;

  void OnEnable() {
    // TODO: update this later so it's a pickup or something
    RemainingBoost = boostCapacity;

    if (centerOfMass == null) {
      centerOfMass = gameObject.transform.Find("Center of Mass");
    }
  }

  public float RemainingBoost {
    get { return remainingBoost; }
    set {
      if (value < 0) {
        remainingBoost = 0;
      }
      else if (value > boostCapacity) {
        remainingBoost = boostCapacity;
      }
      else {
        remainingBoost = value;
      }
    }
  }
}
