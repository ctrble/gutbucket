using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirborneVehicle : MonoBehaviour {
  private VehicleData vehicleData;
  private VehicleMovement vehicleMovement;

  [SerializeField]
  private bool falling;

  void OnEnable() {
    if (vehicleData == null) {
      vehicleData = gameObject.GetComponent<VehicleData>();
    }
    if (vehicleMovement == null) {
      vehicleMovement = gameObject.GetComponent<VehicleMovement>();
    }
  }

  public void EnterState() {
    falling = true;

    RigidbodyInit();
  }

  public void ExitState(string nextState) {
    falling = false;
    vehicleMovement.SetState(nextState);
  }

  void Update() {
    // possible transitions
    if (falling) {
      ListenForChange();
    }
  }

  void ListenForChange() {
    // idle
    if (vehicleMovement.grounded) {
      ExitState("idle");
    }
  }

  void RigidbodyInit() {
    vehicleMovement.vehicleRB.mass = vehicleData.weight * 0.75f;
    vehicleMovement.vehicleRB.drag = 0f;
    vehicleMovement.vehicleRB.angularDrag = 2f;
    vehicleMovement.vehicleRB.useGravity = false;
    vehicleMovement.vehicleRB.centerOfMass = vehicleData.centerOfMass.localPosition;
  }
}
