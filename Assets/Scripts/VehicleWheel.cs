using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleWheel : MonoBehaviour {

  [Header("Components")]

  private VehicleData vehicleData;
  private VehicleMovement vehicleMovement;
  private Rigidbody vehicleRB;
  private CapsuleCollider vehicleCollider;
  public GameObject wheelPrefab;
  public Vector3 defaultWheelCenter;
  public Vector3 defaultWheelPosition;
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
  private bool wheelBelowCenter;
  public float skinWidth;
  public float wheelRadiusX;
  public float wheelRadiusY;
  [SerializeField]
  private float hoverHeight;
  private float hoverForce;
  private Vector3 wheelRestOffset;
  private Vector3 wheelGroundedOffset;
  private Vector3 wheelMaxOffset;
  private float distancePercent;

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

    // how big is this thing
    Bounds wheelBounds = wheelPrefab.GetComponent<MeshRenderer>().bounds;
    wheelRadiusX = wheelBounds.extents.x;
    wheelRadiusY = wheelBounds.extents.y;

    // and where is it
    defaultWheelCenter = wheelBounds.center;
    defaultWheelPosition = wheelPrefab.transform.position;

    // bit of wiggle room
    skinWidth = vehicleCollider.radius * (vehicleCollider.radius * 0.2f);

    SetUpWheels();
  }

  void SetUpWheels() {
    // place where the wheel is
    Vector3 positionOfWheel = defaultWheelCenter;
    transform.position = positionOfWheel;

    // then move upwards
    Vector3 local = transform.localPosition;
    local.y += wheelRadiusY + vehicleCollider.radius + skinWidth;
    transform.localPosition = local;

    // Physics Init
    hoverForce = vehicleMovement.gravityForce;
    hoverHeight = (vehicleCollider.radius * 2) + skinWidth;

    // Wheel Init
    grounded = false;

    // TODO: check these they're probably not good
    wheelRestOffset = -transform.up * (hoverHeight - wheelRadiusY);
    wheelGroundedOffset = wheelRestOffset;
    wheelMaxOffset = -transform.up * (hoverHeight - (wheelRadiusY * 2));
  }

  void Update() {
    GetWheelData();
  }

  void GetWheelData() {
    grounded = false;
    Vector3 tirePosition = transform.position;
    wheelBelowCenter = transform.root.position.y > tirePosition.y;

    Debug.DrawRay(tirePosition, -transform.up * hoverHeight, Color.red);
    bool rayHits = Physics.RaycastNonAlloc(tirePosition, -transform.up, wheelHits, hoverHeight, GameUtilities.instance.staticLayer) > 0;
    if (rayHits) {
      foreach (RaycastHit wheelHit in wheelHits) {
        grounded = true;
        groundNormal = wheelHit.normal;

        // calculate forces
        distancePercent = (1.0f - (wheelHit.distance / hoverHeight));
        wheelForce = transform.up * hoverForce * distancePercent;

        // how far is wheel from the ground
        wheelGroundedOffset = -transform.up * (wheelHit.distance - wheelRadiusY);
      }
    }
    else {
      // might need to change this to Vector3.zero at some point
      groundNormal = Vector3.up;

      // Self levelling - returns the vehicle to horizontal when not grounded and simulates gravity
      if (wheelBelowCenter) {
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
    Vector3 tirePosition = transform.position;
    vehicleRB.AddForceAtPosition(wheelForce, tirePosition);
  }

  void LateUpdate() {
    ClampWheelPosition();
  }

  void ClampWheelPosition() {
    // TODO: this is all wonky cause those wheels are yikes

    // Vector3 targetOffset = grounded ? wheelGroundedOffset : wheelRestOffset;

    // // smooth out the new position
    // Vector3 lerpedOffset = Vector3.Lerp(wheelPrefab.transform.position, transform.position + targetOffset, 0.5f);
    // wheelPrefab.transform.position = lerpedOffset;

    // // only move up and down, dammit
    // Vector3 resetPosition = wheelPrefab.transform.localPosition;
    // wheelPrefab.transform.localPosition = new Vector3(defaultWheelPosition.x, resetPosition.y, defaultWheelPosition.z);

    // // clamp the wheel height
    // if (wheelPrefab.transform.localPosition.y > wheelMaxOffset.y) {
    //   wheelPrefab.transform.localPosition = new Vector3(defaultWheelPosition.x, wheelMaxOffset.y, defaultWheelPosition.z);
    // }
  }
}
