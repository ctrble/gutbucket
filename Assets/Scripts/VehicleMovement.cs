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
  // public Wheel_Controller[] wheelControllers;
  [SerializeField]
  public Rigidbody vehicleRB;
}
