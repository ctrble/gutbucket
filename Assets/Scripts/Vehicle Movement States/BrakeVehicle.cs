using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrakeVehicle : MonoBehaviour {
  private VehicleData vehicleData;
  private VehicleMovement vehicleMovement;

  [SerializeField]
  private bool braking;

  void OnEnable() {
    if (vehicleData == null) {
      vehicleData = gameObject.GetComponent<VehicleData>();
    }
    if (vehicleMovement == null) {
      vehicleMovement = gameObject.GetComponent<VehicleMovement>();
    }
  }

  public void EnterState() {
    braking = true;

    RigidbodyInit();
  }

  public void ExitState(string nextState) {
    braking = false;
    vehicleMovement.SetState(nextState);
  }

  void Update() {
    if (braking) {
      // possible transitions
      ListenForChange();

      if (vehicleMovement.boost) {
        RigidbodyBoosting();
      }
    }
  }

  void FixedUpdate() {
    if (braking) {
      Brake();
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

    // skid
    if (vehicleMovement.playerInput.Accelerate() && vehicleMovement.playerInput.Brake()) {
      ExitState("skid");
    }
  }

  void RigidbodyInit() {
    vehicleMovement.vehicleRB.mass = vehicleData.weight;
    vehicleMovement.vehicleRB.drag = vehicleData.brakes;
    vehicleMovement.vehicleRB.angularDrag = vehicleData.brakes;
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

  void Brake() {
    vehicleMovement.TrackCurrentSpeed();

    // boosting
    float boostModifier = vehicleMovement.boost ? vehicleData.boostAmount * (vehicleData.brakes * 0.1f) : 0f;
    float reverseModifier = vehicleMovement.reverse ? -0.5f : 1f;
    float driveSpeed = boostModifier * reverseModifier;
    vehicleMovement.AddVehicleForce(driveSpeed);

    // decelerate
    vehicleMovement.DampVehicleForce(0f);

    Turn();

    // balance
    if (Mathf.Abs(vehicleMovement.currentZAngle) >= 2f) {
      vehicleMovement.CounterRoll(1f);
    }
  }

  void Turn() {
    Vector3 wheelForward = vehicleMovement.AverageWheelDirection();
    Vector3 currentVector = vehicleMovement.reverse ? wheelForward : transform.forward;
    Vector3 targetVector = vehicleMovement.reverse ? transform.forward : wheelForward;
    float torquePower = Mathf.Clamp(vehicleMovement.currentSpeed * 0.1f, 0, vehicleData.turnRadius * 0.5f);

    vehicleMovement.AddVehicleTorque(currentVector, targetVector, torquePower);
  }
}
