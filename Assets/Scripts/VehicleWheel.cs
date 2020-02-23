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

    // defaultWheelPosition = wheelPrefab.transform.position;

    // how big is this thing
    Bounds wheelBounds = wheelPrefab.GetComponent<MeshRenderer>().bounds;
    defaultWheelPosition = wheelBounds.center;
    wheelRadiusX = wheelBounds.extents.x;
    wheelRadiusY = wheelBounds.extents.y;
    // wheelRadiusX = Mathf.Abs(wheelBounds.extents.x * wheelPrefab.transform.localScale.x);
    // wheelRadiusY = Mathf.Abs(wheelBounds.extents.y * wheelPrefab.transform.localScale.y);

    // bit of wiggle room
    skinWidth = vehicleCollider.radius * (vehicleCollider.radius * 0.2f);

    SetUpWheels();
  }

  void SetUpWheels() {
    // set at the wheel's position
    // transform.position = new Vector3(defaultWheelPosition.x, defaultWheelPosition.y + wheelRadiusY, defaultWheelPosition.z);

    // Vector3 positionOfWheel = transform.TransformPoint(wheelPrefab.transform.position);
    Vector3 positionOfWheel = defaultWheelPosition;
    transform.position = positionOfWheel;

    Vector3 local = transform.localPosition;
    local.y += wheelRadiusY + vehicleCollider.radius + skinWidth;
    transform.localPosition = local;

    // move up to the top of the wheel
    // Vector3 local = transform.localPosition;
    // local.y += vehicleCollider.radius + wheelRadiusY;
    // local.y += wheelRadiusY;
    // transform.localPosition = local;

    // Physics Init
    hoverForce = vehicleMovement.gravityForce;
    hoverHeight = (vehicleCollider.radius * 2) + skinWidth;

    // Wheel Init
    grounded = false;

    // Position the wheel
    // Quaternion wheelRotation = Quaternion.LookRotation(transform.forward, -transform.right);

    // wheelRadiusY = wheelPrefab.GetComponent<MeshFilter>().mesh.bounds.extents.y;
    wheelRestOffset = -transform.up * (hoverHeight - wheelRadiusY);
    wheelGroundedOffset = wheelRestOffset;
    wheelMaxOffset = -transform.up * (hoverHeight - (wheelRadiusY * 2));

    // transform.position += wheelRestOffset;

    // wheelChild = Instantiate(wheelPrefab, transform.position + wheelRestOffset, wheelRotation, transform);
  }

  void Update() {
    GetWheelData();
  }

  void GetWheelData() {
    grounded = false;
    Vector3 tirePosition = transform.position;
    // Vector3 tirePosition = wheelPrefab.transform.position;
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

    if (!grounded) {
      Debug.Log("not grounded! " + tirePosition);
    }
  }

  void FixedUpdate() {
    ApplyWheelForces();
  }

  void ApplyWheelForces() {
    Vector3 tirePosition = transform.position;
    // Debug.DrawRay(tirePosition, groundedForce, Color.green);
    vehicleRB.AddForceAtPosition(wheelForce, tirePosition);
  }
}
