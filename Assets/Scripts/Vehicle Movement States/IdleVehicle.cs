using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleVehicle : MonoBehaviour {
  private VehicleData vehicleData;
  private VehicleMovement vehicleMovement;

  [SerializeField]
  private bool idling;

  void OnEnable() {
    if (vehicleData == null) {
      vehicleData = gameObject.GetComponent<VehicleData>();
    }
    if (vehicleMovement == null) {
      vehicleMovement = gameObject.GetComponent<VehicleMovement>();
    }
  }

  public void EnterState() {
    idling = true;

    RigidbodyInit();
  }

  public void ExitState(string nextState) {
    idling = false;
    vehicleMovement.SetState(nextState);
  }

  void Update() {
    if (idling) {
      // possible transitions
      ListenForChange();
    }
  }

  void FixedUpdate() {
    if (idling) {
      Idle();
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

    // drive
    else if (vehicleMovement.playerInput.Accelerate() && !vehicleMovement.playerInput.Brake()) {
      ExitState("drive");
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
    vehicleMovement.vehicleRB.drag = 1.5f;
    vehicleMovement.vehicleRB.angularDrag = 4f;
    vehicleMovement.vehicleRB.useGravity = false;
    vehicleMovement.vehicleRB.centerOfMass = vehicleData.centerOfMass.localPosition;
  }

  void Idle() {
    vehicleMovement.TrackCurrentSpeed();

    Turn();

    // balance
    if (Mathf.Abs(vehicleMovement.currentZAngle) >= 5f) {
      vehicleMovement.CounterRoll(5f);
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
