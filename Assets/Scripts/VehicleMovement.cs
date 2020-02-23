using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleMovement : MonoBehaviour {

  public enum MovementState { Airborne, Tilt, Idle, Drive, Brake, Skid };
  public MovementState currentState;
  public AirborneVehicle airborneState;
  public IdleVehicle idleState;
  public DriveVehicle driveState;
  public BrakeVehicle brakeState;
  public SkidVehicle skidState;
  public TiltVehicle tiltState;

  [Space]
  [Header("Helpers")]

  public bool boost;
  public bool reverse;
  public bool grounded;
  public int groundedWheels;
  public float currentSpeed;
  private float tempSpeed;
  private float lastSpeed;
  public float gravityForce;
  public float currentAngle;
  public float currentZAngle;

  [Space]
  [Header("Components")]

  public VehicleData vehicleData;
  public InputController playerInput;
  public VehicleWheel[] wheelControllers;
  [SerializeField]
  public Rigidbody vehicleRB;

  public void Start() {
    // Component Init
    if (vehicleData == null) {
      vehicleData = gameObject.GetComponent<VehicleData>();
    }
    if (playerInput == null) {
      playerInput = gameObject.GetComponentInParent<InputController>();
    }
    if (vehicleRB == null) {
      vehicleRB = gameObject.GetComponentInParent<Rigidbody>();
    }
    if (wheelControllers.Length == 0) {
      wheelControllers = gameObject.GetComponentsInChildren<VehicleWheel>();
    }

    airborneState = GetComponent<AirborneVehicle>();
    tiltState = GetComponent<TiltVehicle>();
    idleState = GetComponent<IdleVehicle>();
    driveState = GetComponent<DriveVehicle>();
    brakeState = GetComponent<BrakeVehicle>();
    skidState = GetComponent<SkidVehicle>();

    currentState = MovementState.Idle;
    ChangeState();

    gravityForce = -Physics.gravity.y * (vehicleData.weight * 0.5f);
  }

  public void ChangeState() {
    switch (currentState) {
      // case MovementState.Airborne:
      //   airborneState.EnterState();
      //   break;
      // case MovementState.Tilt:
      //   tiltState.EnterState();
      //   break;
      // case MovementState.Idle:
      //   idleState.EnterState();
      //   break;
      // case MovementState.Drive:
      //   driveState.EnterState();
      //   break;
      // case MovementState.Brake:
      //   brakeState.EnterState();
      //   break;
      // case MovementState.Skid:
      //   skidState.EnterState();
      //   break;
      default:
        idleState.EnterState();
        break;
    }
  }
}
