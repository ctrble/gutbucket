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
  public float forwardVelocity;
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
      case MovementState.Airborne:
        airborneState.EnterState();
        break;
      case MovementState.Tilt:
        tiltState.EnterState();
        break;
      case MovementState.Idle:
        idleState.EnterState();
        break;
      case MovementState.Drive:
        driveState.EnterState();
        break;
      case MovementState.Brake:
        brakeState.EnterState();
        break;
      case MovementState.Skid:
        skidState.EnterState();
        break;
      default:
        idleState.EnterState();
        break;
    }
  }

  IEnumerator ChangeStateAfterFrame() {
    // this helps make extra sure there aren't multiple states transitioning simultaneously
    yield return new WaitForEndOfFrame();
    ChangeState();
  }

  public void SetState(string nextState) {
    if (nextState == "airborne" && currentState != MovementState.Airborne) {
      currentState = MovementState.Airborne;
      StartCoroutine(ChangeStateAfterFrame());
    }

    if (nextState == "tilt" && currentState != MovementState.Tilt) {
      currentState = MovementState.Tilt;
      StartCoroutine(ChangeStateAfterFrame());
    }

    if (nextState == "idle" && currentState != MovementState.Idle) {
      currentState = MovementState.Idle;
      StartCoroutine(ChangeStateAfterFrame());
    }

    if (nextState == "drive" && currentState != MovementState.Drive) {
      currentState = MovementState.Drive;
      StartCoroutine(ChangeStateAfterFrame());
    }

    if (nextState == "brake" && currentState != MovementState.Brake) {
      currentState = MovementState.Brake;
      StartCoroutine(ChangeStateAfterFrame());
    }

    if (nextState == "skid" && currentState != MovementState.Skid) {
      currentState = MovementState.Skid;
      StartCoroutine(ChangeStateAfterFrame());
    }
  }

  void Update() {
    SetWheelRotation();
    SetReverse();
    SetBoost();
    TrackBoost();
    TrackAngles();
    TrackGround();
  }

  void SetReverse() {
    if (playerInput.ReverseStart()) {
      reverse = !reverse;
    }
  }

  void SetBoost() {
    if (playerInput.BoostStart()) {
      boost = !boost;
    }

    if (vehicleData.RemainingBoost <= 0f) {
      boost = false;
    }
  }

  void TrackBoost() {
    if (boost) {
      vehicleData.RemainingBoost -= Time.deltaTime;
    }
  }

  void TrackAngles() {
    // current angle overall
    currentAngle = Vector3.SignedAngle(Vector3.up, transform.up, Vector3.up);

    // just along the z axis (roll)
    currentZAngle = GameUtilities.instance.GetRelativeAngles(transform.eulerAngles).z;
  }

  void SetWheelRotation() {
    foreach (VehicleWheel wheel in wheelControllers) {
      if (wheel.steering) {
        RotateWheel(wheel.wheelChild.transform, 30f);
      }
    }
  }

  void RotateWheel(Transform wheel, float maxRotation) {
    Vector3 newWheelRotation = Vector3.up * SteerDirection() * maxRotation;
    wheel.localEulerAngles = newWheelRotation;
  }

  public float SteerDirection() {
    return playerInput.PlayerMove().x;
  }

  public void TrackGround() {
    groundedWheels = 0;
    for (int i = 0; i < wheelControllers.Length; i++) {
      if (wheelControllers[i].grounded) {
        groundedWheels++;
      }
    }
    grounded = groundedWheels > 0;
  }

  public void TrackCurrentSpeed() {
    Vector3 direction = reverse ? -transform.forward : transform.forward;
    forwardVelocity = Vector3.Dot(vehicleRB.velocity, direction);
    // float forwardSpeed = Vector3.Dot(vehicleRB.velocity, direction);
    float smoothedSpeed = Mathf.SmoothDamp(forwardVelocity, lastSpeed, ref tempSpeed, 0.5f);
    currentSpeed = smoothedSpeed;

    // cache velocity
    lastSpeed = forwardVelocity;

    // check if reversing and set it to negative
    if (reverse) {
      currentSpeed = -Mathf.Abs(currentSpeed);
    }
  }

  public Vector3 AverageWheelDirection() {
    Vector3 wheelHeading = Vector3.zero;
    int steerers = 0;
    for (int i = 0; i < wheelControllers.Length; i++) {
      if (wheelControllers[i].steering) {
        wheelHeading += wheelControllers[i].transform.GetChild(0).forward;
        steerers++;
      }
    }
    return wheelHeading / steerers;
  }

  public void AddVehicleForce(float targetSpeed) {
    // drive forward from the front wheels
    Vector3 direction = (AverageWheelDirection() + transform.forward).normalized;

    // get the thrust needed while accounting for drag
    float target = GameUtilities.instance.ConvertFromKPH(targetSpeed);
    float thrust = GetRequiredAcceleraton(target, vehicleRB.drag);

    // we're applying acceleration as meters per second
    vehicleRB.AddForce(direction * thrust, ForceMode.Acceleration);
  }

  public void DampVehicleForce(float targetSpeed) {
    // slow down in whichever direction we happen to be going
    Vector3 direction = vehicleRB.velocity.normalized;

    // get the thrust needed while accounting for drag
    float thrust = GetRequiredAcceleraton(targetSpeed, vehicleRB.drag);
    vehicleRB.AddForce(direction * thrust, ForceMode.Acceleration);
  }

  public void AddVehicleTorque(Vector3 current, Vector3 target, float speed) {
    Vector3 currentVector = current;
    Vector3 targetVector = target * speed;
    float turnDegrees = vehicleData.turnRadius;
    float turnSpeed = vehicleData.turnRadius * Mathf.Abs(SteerDirection());

    Vector3 positionDifference = DirectionDifference(currentVector, targetVector, turnDegrees, turnSpeed);
    Vector3 velocityDifference = AngularVelocityDifference();

    float frequency = vehicleData.handling * 0.01f;
    float damping = 1f;
    Vector3 torque = GameUtilities.instance.StableBackwardsPD(positionDifference, velocityDifference, frequency, damping);

    vehicleRB.AddTorque(torque);
  }

  public void CounterRoll(float frequency) {
    Vector3 currentVector = transform.up * Mathf.Abs(currentZAngle);
    Vector3 targetVector = Vector3.up;
    float turnDegrees = 180f;
    float turnSpeed = Mathf.Abs(currentZAngle);

    Vector3 positionDifference = DirectionDifference(currentVector, targetVector, turnDegrees, turnSpeed);
    Vector3 velocityDifference = AngularVelocityDifference();

    float damping = 1f;
    Vector3 torque = GameUtilities.instance.StableBackwardsPD(positionDifference, velocityDifference, frequency, damping);

    vehicleRB.AddTorque(torque);
  }

  float GetRequiredVelocityChange(float targetSpeed, float drag) {
    // this is how Unity does drag, so we do too
    float dragPercent = Mathf.Clamp01(drag * Time.deltaTime);
    return targetSpeed * dragPercent / (1 - dragPercent);
  }

  float GetRequiredAcceleraton(float targetSpeed, float drag) {
    return GetRequiredVelocityChange(targetSpeed, drag) / Time.deltaTime;
  }

  Vector3 DirectionDifference(Vector3 current, Vector3 target, float degrees, float speed) {
    // Determine heading to point at target limited by maximum turn rate
    float turnRadians = degrees * Mathf.Deg2Rad;
    Vector3 adjustedTargetHeading = Vector3.RotateTowards(current, target, turnRadians, speed);

    // The angle controller drives the vehicle's angle towards the target angle.
    Vector3 headingError = Vector3.Cross(current, adjustedTargetHeading);
    return headingError;
  }

  Vector3 AngularVelocityDifference() {
    Vector3 angularVelocityError = vehicleRB.angularVelocity * -1f;
    return angularVelocityError;
  }
}
