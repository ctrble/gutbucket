using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkidVehicle : MonoBehaviour {
  private VehicleData vehicleData;
  private VehicleMovement vehicleMovement;

  [SerializeField]
  private bool skidding;

  void OnEnable() {
    if (vehicleData == null) {
      vehicleData = gameObject.GetComponent<VehicleData>();
    }
    if (vehicleMovement == null) {
      vehicleMovement = gameObject.GetComponent<VehicleMovement>();
    }
  }

  public void EnterState() {
    skidding = true;

    RigidbodyInit();
  }

  public void ExitState(string nextState) {
    skidding = false;
    vehicleMovement.SetState(nextState);
  }

  void Update() {
    if (skidding) {
      // possible transitions
      ListenForChange();

      if (vehicleMovement.boost) {
        RigidbodyBoosting();
      }
    }
  }

  void FixedUpdate() {
    if (skidding) {
      Skid();
    }
  }

  void ListenForChange() {
    // airborne
    if (!vehicleMovement.grounded) {
      ExitState("airborne");
    }

    // tilt
    if (vehicleMovement.currentAngle >= vehicleData.maxClimbAngle || vehicleMovement.currentAngle <= -vehicleData.maxClimbAngle) {
      ExitState("tilt");
    }

    // idle
    else if (!vehicleMovement.playerInput.Accelerate() && !vehicleMovement.playerInput.Brake()) {
      ExitState("idle");
    }

    // drive
    else if (vehicleMovement.playerInput.Accelerate() && !vehicleMovement.playerInput.Brake()) {
      ExitState("drive");
    }

    // brake
    if (!vehicleMovement.playerInput.Accelerate() && vehicleMovement.playerInput.Brake()) {
      ExitState("brake");
    }
  }

  void RigidbodyInit() {
    vehicleMovement.vehicleRB.mass = vehicleData.weight;
    vehicleMovement.vehicleRB.drag = 1.5f;
    vehicleMovement.vehicleRB.angularDrag = 4f;
    vehicleMovement.vehicleRB.useGravity = false;
    vehicleMovement.vehicleRB.centerOfMass = vehicleData.centerOfMass.localPosition;
  }

  void RigidbodyBoosting() {
    vehicleMovement.vehicleRB.mass = vehicleData.weight;
    vehicleMovement.vehicleRB.drag = 2f;
    vehicleMovement.vehicleRB.angularDrag = 5f;
    vehicleMovement.vehicleRB.useGravity = false;
    vehicleMovement.vehicleRB.centerOfMass = vehicleData.centerOfMass.localPosition;
  }

  void Skid() {
    vehicleMovement.TrackCurrentSpeed();

    AccelerateDecelerate();

    // skid
    // no torque added here, but we could and it seems to work fine if it's done like in braking
    float flip = vehicleMovement.reverse ? -1 : 1;
    float skidForce = vehicleData.handling;
    foreach (VehicleWheel wheel in vehicleMovement.wheelControllers) {
      if (!wheel.steering) {
        Transform wheelPosition = wheel.wheelModel.transform;
        Vector3 direction = -wheelPosition.right * vehicleMovement.SteerDirection() * flip;

        // along the plane
        Vector3 projected = Vector3.ProjectOnPlane(direction, wheel.groundNormal);
        Vector3 skidDirection = projected;
        Vector3 skidPosition = wheelPosition.position;

        vehicleMovement.vehicleRB.AddForceAtPosition(skidDirection * skidForce, skidPosition);
      }
    }
  }

  void AccelerateDecelerate() {
    float boostModifier = vehicleMovement.boost ? vehicleData.boostAmount : 0f;
    float reverseModifier = vehicleMovement.reverse ? -0.5f : 1f;
    float skidSpeed = ((vehicleData.topSpeed * 0.5f) + boostModifier) * reverseModifier;
    vehicleMovement.AddVehicleForce(skidSpeed);
  }
}
