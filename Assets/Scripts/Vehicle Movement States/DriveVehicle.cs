using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveVehicle : MonoBehaviour {
  private VehicleData vehicleData;
  private VehicleMovement vehicleMovement;

  [SerializeField]
  private bool driving;

  void OnEnable() {
    if (vehicleData == null) {
      vehicleData = gameObject.GetComponent<VehicleData>();
    }
    if (vehicleMovement == null) {
      vehicleMovement = gameObject.GetComponent<VehicleMovement>();
    }
  }

  public void EnterState() {
    driving = true;

    RigidbodyInit();
  }

  public void ExitState(string nextState) {
    driving = false;
    vehicleMovement.SetState(nextState);
  }

  void Update() {
    if (driving) {
      // possible transitions
      ListenForChange();

      if (vehicleMovement.boost) {
        RigidbodyBoosting();
      }
    }
  }

  void FixedUpdate() {
    if (driving) {
      Drive();
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

    // brake
    else if (!vehicleMovement.playerInput.Accelerate() && vehicleMovement.playerInput.Brake()) {
      ExitState("brake");
    }

    // skid
    if (vehicleMovement.playerInput.Accelerate() && vehicleMovement.playerInput.Brake()) {
      ExitState("skid");
    }
  }

  void RigidbodyInit() {
    vehicleMovement.vehicleRB.mass = vehicleData.weight;
    vehicleMovement.vehicleRB.drag = 1f;
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

  void Drive() {
    vehicleMovement.TrackCurrentSpeed();

    AccelerateDecelerate();
    Turn();

    // balance
    if (Mathf.Abs(vehicleMovement.currentZAngle) >= 2f) {
      vehicleMovement.CounterRoll(1f);
    }
  }

  void AccelerateDecelerate() {
    float boostModifier = vehicleMovement.boost ? vehicleData.boostAmount : 0f;
    float reverseModifier = vehicleMovement.reverse ? -0.5f : 1f;
    float driveSpeed = (vehicleData.topSpeed + boostModifier) * reverseModifier;

    vehicleMovement.AddVehicleForce(driveSpeed);
  }

  void Turn() {
    Vector3 wheelForward = vehicleMovement.AverageWheelDirection();
    Vector3 currentVector = vehicleMovement.reverse ? wheelForward : transform.forward;
    Vector3 targetVector = vehicleMovement.reverse ? transform.forward : wheelForward;
    float torquePower = vehicleData.turnRadius * 0.5f;

    vehicleMovement.AddVehicleTorque(currentVector, targetVector, torquePower);
  }
}
