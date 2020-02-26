using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltVehicle : MonoBehaviour {
  private VehicleData vehicleData;
  private VehicleMovement vehicleMovement;
  public VehicleWheel[] wheelControllers;

  [SerializeField]
  private bool tilted;
  public float timeTilted;

  void OnEnable() {
    if (vehicleData == null) {
      vehicleData = gameObject.GetComponent<VehicleData>();
    }
    if (vehicleMovement == null) {
      vehicleMovement = gameObject.GetComponent<VehicleMovement>();
    }
    if (wheelControllers.Length == 0) {
      wheelControllers = gameObject.GetComponentsInChildren<VehicleWheel>();
    }
  }

  public void EnterState() {
    tilted = true;
    timeTilted = 0f;

    RigidbodyInit();
  }

  public void ExitState(string nextState) {
    tilted = false;
    vehicleMovement.SetState(nextState);
  }

  void Update() {
    if (tilted) {
      // possible transitions
      ListenForChange();

      timeTilted += Time.deltaTime;
    }
  }

  void FixedUpdate() {
    if (tilted) {
      Tilted();
    }
  }

  void ListenForChange() {
    // airborne
    if (!vehicleMovement.grounded) {
      ExitState("airborne");
    }

    // idle
    else if (vehicleMovement.currentAngle < vehicleData.maxClimbAngle && vehicleMovement.currentAngle > -vehicleData.maxClimbAngle) {
      ExitState("idle");
    }
  }

  void RigidbodyInit() {
    vehicleMovement.vehicleRB.mass = vehicleData.weight;
    vehicleMovement.vehicleRB.drag = 0.5f;
    vehicleMovement.vehicleRB.angularDrag = 4f;
    vehicleMovement.vehicleRB.useGravity = false;
    vehicleMovement.vehicleRB.centerOfMass = vehicleData.centerOfMass.localPosition;
  }

  void Tilted() {
    vehicleMovement.TrackCurrentSpeed();

    DragWheels();

    // even though we're tilted, we can still drive a bit
    if (vehicleMovement.playerInput.Accelerate()) {
      AccelerateDecelerate();
      Turn();
    }

    // balance
    if (Mathf.Abs(vehicleMovement.currentZAngle) >= 10f) {
      vehicleMovement.CounterRoll(1f);
    }
  }

  void DragWheels() {
    foreach (VehicleWheel wheel in wheelControllers) {
      if (wheel.grounded && wheel.groundNormal != Vector3.up) {
        PushWheelDown(wheel);
      }
    }
  }

  void PushWheelDown(VehicleWheel wheel) {
    // down along the normal
    Vector3 tempDirection = Vector3.Cross(wheel.transform.up, wheel.groundNormal).normalized;
    Vector3 pushDirection = Vector3.Cross(tempDirection, wheel.groundNormal).normalized;

    // no messing around, lots of force and then more
    float pushAmount = vehicleMovement.gravityForce * (2 + timeTilted);

    // push the wheel
    vehicleMovement.vehicleRB.AddForceAtPosition(pushDirection * pushAmount, wheel.wheelChild.transform.position);
  }

  void AccelerateDecelerate() {
    float boostModifier = vehicleMovement.boost ? (vehicleData.boostAmount * 0.1f) : 0f;
    float reverseModifier = vehicleMovement.reverse ? -0.5f : 1f;
    float driveSpeed = (TiltSpeed(vehicleMovement.groundedWheels) + boostModifier) * reverseModifier;

    vehicleMovement.AddVehicleForce(driveSpeed);
  }

  void Turn() {
    Vector3 wheelForward = vehicleMovement.AverageWheelDirection();
    Vector3 currentVector = vehicleMovement.reverse ? wheelForward : transform.forward;
    Vector3 targetVector = vehicleMovement.reverse ? transform.forward : wheelForward;
    float torquePower = vehicleData.turnRadius * 0.5f;

    vehicleMovement.AddVehicleTorque(currentVector, targetVector, torquePower);
  }

  float TiltSpeed(int groundedWheels) {
    // the more wheels are grounded the closer to top speed we can get
    float groundedPercent = Mathf.Clamp01((float)groundedWheels / (float)wheelControllers.Length);

    // the more tilted we are the less speed we can get, can't really go more than 90 degrees
    float maxTilt = 90f;
    float tiltModifierPercent = Mathf.Clamp01((maxTilt - vehicleMovement.currentAngle) / maxTilt);

    // calculate speed while tilted
    return vehicleData.topSpeed * groundedPercent * tiltModifierPercent;
  }
}
