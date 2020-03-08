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
  public GameObject wheelChild;

  [Space]
  [Header("Data")]

  private bool vehicleIsFlipped;
  public bool grounded;
  public Vector3 groundNormal = Vector3.up;
  public Vector3 groundPoint = Vector3.zero;
  public RaycastHit[] wheelHits = new RaycastHit[1];
  private Bounds wheelBounds;
  public float wheelRadiusX;
  public float wheelRadiusY;
  private Vector3 wheelGroundPosition;
  private Vector3 wheelHighPosition;
  private Vector3 wheelLowPosition;

  [Space]
  [Header("Settings")]

  public bool steering = false;
  public float skinWidth;
  private float hoverHeight;
  private float hoverForce;
  private Vector3 wheelForce = Vector3.zero;
  private float distancePercent;

  void OnEnable() {
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

    SetPhysicsDefaults();
    SetUpWheels();
  }

  void SetUpWheels() {
    GetWheelSizes();
    PositionInit();

    // figure out mins and maxes of child positioning
    Vector3 childPosition = wheelChild.transform.localPosition;
    wheelHighPosition = new Vector3(childPosition.x, childPosition.y + (wheelRadiusY * 2), childPosition.z);
    wheelGroundPosition = Vector3.zero;
    wheelLowPosition = new Vector3(childPosition.x, childPosition.y - wheelRadiusY, childPosition.z);
  }

  void SetPhysicsDefaults() {
    grounded = false;
    skinWidth = vehicleCollider.radius * (vehicleCollider.radius * 0.2f);
    hoverForce = vehicleMovement.gravityForce;
    hoverHeight = (vehicleCollider.radius * 2) + skinWidth;
  }

  void GetWheelSizes() {
    wheelBounds = wheelModel.GetComponent<MeshRenderer>().bounds;
    wheelRadiusX = wheelBounds.extents.x;
    wheelRadiusY = wheelBounds.extents.y;
  }

  void PositionInit() {
    // place this the wheel is
    transform.position = wheelBounds.center;

    // move this up to where the top of the collider is
    float colliderTopPos = vehicleCollider.transform.localPosition.y + vehicleCollider.radius;
    Vector3 positionAtColliderTop = new Vector3(transform.localPosition.x, colliderTopPos, transform.localPosition.z);
    transform.localPosition = positionAtColliderTop;

    // now it's safe to move the model
    ReparentWheelModel();
  }

  void ReparentWheelModel() {
    // reparent the wheel model to the wheel child
    wheelChild = transform.GetChild(0).gameObject;
    wheelModel.transform.parent = wheelChild.transform;
  }

  void Update() {
    GetWheelData();
  }

  void GetWheelData() {
    grounded = false;

    // check from the top of the collider (this position) down to the ground
    bool rayHits = Physics.RaycastNonAlloc(transform.position, -transform.up, wheelHits, hoverHeight, GameUtilities.instance.staticLayer) > 0;
    if (rayHits) {
      foreach (RaycastHit wheelHit in wheelHits) {
        grounded = true;
        groundNormal = wheelHit.normal;
        groundPoint = wheelHit.point;

        // calculate forces
        distancePercent = wheelHit.distance / hoverHeight;
        wheelForce = transform.up * hoverForce * distancePercent;

        // get where the wheel will rest on the ground
        wheelGroundPosition = transform.localPosition + transform.InverseTransformPoint(groundPoint);
      }
    }
    else {
      // might need to change this to Vector3.zero at some point
      groundNormal = Vector3.up;
      groundPoint = Vector3.zero;

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
    Vector3 newWheelPosition = grounded ? wheelGroundPosition : wheelLowPosition;

    // smooth out the new position
    wheelChild.transform.localPosition = Vector3.Lerp(wheelChild.transform.localPosition, newWheelPosition, 0.25f);

    // but don't cut into the ground
    bool wouldIntersect = wheelLowPosition.y > wheelGroundPosition.y;
    float lowestY = wouldIntersect ? wheelLowPosition.y : wheelGroundPosition.y;
    float clampedY = Mathf.Clamp(wheelChild.transform.localPosition.y, lowestY, wheelHighPosition.y);
    float smoothedY = Mathf.Lerp(wheelChild.transform.localPosition.y, clampedY, 0.75f);

    // apply the adjustments in wheel Y position while zeroing out other movement
    wheelChild.transform.localPosition = new Vector3(0, smoothedY, 0);
  }
}
