using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleWheel : MonoBehaviour {

  [Header("Components")]

  private VehicleData vehicleData;
  private VehicleMovement vehicleMovement;
  private Rigidbody vehicleRB;
  private CapsuleCollider vehicleCollider;
  public GameObject wheelModel;
  public Vector3 initialWheelCenter;
  public Vector3 initialWheelPosition;
  public GameObject wheelChild;

  [Space]
  [Header("Data")]

  public bool grounded;
  public Vector3 groundNormal = Vector3.up;
  private Vector3 wheelForce = Vector3.zero;
  public RaycastHit[] wheelHits = new RaycastHit[1];

  [Space]
  [Header("Settings")]

  public bool steering = false;
  private bool vehicleIsFlipped;
  public float skinWidth;
  public float wheelRadiusX;
  public float wheelRadiusY;
  [SerializeField]
  private float hoverHeight;
  private float hoverForce;
  [SerializeField]
  private Vector3 wheelRestOffset;
  [SerializeField]
  private Vector3 wheelGroundedPosition;
  [SerializeField]
  private Vector3 wheelHighestPosition;
  [SerializeField]
  private Vector3 wheelLowestPosition;
  private float distancePercent;
  public Bounds wheelBounds;

  void Start() {
    if (vehicleRB == null) {
      vehicleRB = gameObject.GetComponentInParent<Rigidbody>();
    }
    if (vehicleCollider == null) {
      vehicleCollider = transform.root.GetComponentInChildren<CapsuleCollider>();
    }
    if (vehicleData == null) {
      vehicleData = gameObject.GetComponentInParent<VehicleData>();
    }
    if (vehicleMovement == null) {
      vehicleMovement = gameObject.GetComponentInParent<VehicleMovement>();
    }

    SetUpWheels();
  }

  void SetUpWheels() {
    // bit of wiggle room
    skinWidth = vehicleCollider.radius * (vehicleCollider.radius * 0.2f);

    // physics init
    hoverForce = vehicleMovement.gravityForce;
    hoverHeight = (vehicleCollider.radius * 2) + skinWidth;
    grounded = false;

    // how big is this thing?
    wheelBounds = wheelModel.GetComponent<MeshRenderer>().bounds;
    wheelRadiusX = wheelBounds.extents.x;
    wheelRadiusY = wheelBounds.extents.y;

    // and where is it?
    initialWheelCenter = wheelBounds.center;
    initialWheelPosition = wheelModel.transform.position;

    // place this the wheel is
    transform.position = initialWheelCenter;

    // move this up to where the top of the collider is
    float colliderTopPos = vehicleCollider.transform.localPosition.y + vehicleCollider.radius;
    Vector3 positionAtColliderTop = new Vector3(transform.localPosition.x, colliderTopPos, transform.localPosition.z);
    transform.localPosition = positionAtColliderTop;

    // reparent the wheel model to the wheel child
    wheelChild = transform.GetChild(0).gameObject;
    wheelModel.transform.parent = wheelChild.transform;

    // figure out mins and maxes of child positioning
    Vector3 wheelChildPosition = wheelChild.transform.localPosition;
    wheelHighestPosition = new Vector3(wheelChildPosition.x, wheelChildPosition.y + (wheelRadiusY * 2), wheelChildPosition.z);
    wheelGroundedPosition = Vector3.zero;
    wheelLowestPosition = new Vector3(wheelChildPosition.x, wheelChildPosition.y - wheelRadiusY, wheelChildPosition.z);
  }

  void Update() {
    GetWheelData();
  }

  void GetWheelData() {
    grounded = false;

    // check from the top of the collider (this position) down to the ground
    // Debug.DrawRay(transform.position, -transform.up * hoverHeight, Color.red);
    bool rayHits = Physics.RaycastNonAlloc(transform.position, -transform.up, wheelHits, hoverHeight, GameUtilities.instance.staticLayer) > 0;
    if (rayHits) {
      foreach (RaycastHit wheelHit in wheelHits) {
        grounded = true;
        groundNormal = wheelHit.normal;

        // calculate forces
        distancePercent = wheelHit.distance / hoverHeight;
        wheelForce = transform.up * hoverForce * distancePercent;

        // get where the wheel will rest on the ground
        wheelGroundedPosition = transform.localPosition + transform.InverseTransformPoint(wheelHit.point);
      }
    }
    else {
      // might need to change this to Vector3.zero at some point
      groundNormal = Vector3.up;

      // Self levelling - returns the vehicle to horizontal when not grounded and simulates gravity
      vehicleIsFlipped = transform.parent.position.y > wheelChild.transform.position.y;
      if (vehicleIsFlipped) {
        // Push it up (add balance)
        wheelForce = transform.up * vehicleMovement.gravityForce;
      }
      else {
        // Pull it down (apply gravity)
        wheelForce = -transform.up * vehicleMovement.gravityForce;
      }
    }
  }

  void FixedUpdate() {
    ApplyWheelForces();
  }

  void ApplyWheelForces() {
    // Vector3 tirePosition = transform.position;
    Vector3 tirePosition = wheelChild.transform.position;
    vehicleRB.AddForceAtPosition(wheelForce, tirePosition);
  }

  void LateUpdate() {
    WheelSuspension();
  }

  void WheelSuspension() {
    Vector3 newWheelPosition = grounded ? wheelGroundedPosition : wheelLowestPosition;

    // smooth out the new position
    wheelChild.transform.localPosition = Vector3.Lerp(wheelChild.transform.localPosition, newWheelPosition, 0.25f);

    // only move up and down, dammit, and don't go too high
    Vector3 localPosition = wheelChild.transform.localPosition;
    float resetY = Mathf.Clamp(localPosition.y, wheelLowestPosition.y, wheelHighestPosition.y);
    wheelChild.transform.localPosition = new Vector3(0, Mathf.Lerp(localPosition.y, resetY, 0.75f), 0);
  }
}
