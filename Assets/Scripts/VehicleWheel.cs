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
  public float wheelRadius;
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

    SetUpWheels();
  }

  void SetUpWheels() {
    // Set the height of the wheel
    skinWidth = vehicleCollider.radius * (vehicleCollider.radius * 0.2f);
    Vector3 local = transform.localPosition;
    local.y = vehicleCollider.radius - skinWidth;
    transform.localPosition = local;

    // Physics Init
    hoverForce = vehicleMovement.gravityForce;
    hoverHeight = (vehicleCollider.radius * 2) + skinWidth;

    // Wheel Init
    grounded = false;

    // Position the wheel
    Quaternion wheelRotation = Quaternion.LookRotation(transform.forward, -transform.right);

    wheelRadius = wheelPrefab.GetComponent<CapsuleCollider>().radius;
    wheelRestOffset = -transform.up * (hoverHeight - wheelRadius);
    wheelGroundedOffset = wheelRestOffset;
    wheelMaxOffset = -transform.up * (hoverHeight - (wheelRadius * 2));

    wheelChild = Instantiate(wheelPrefab, transform.position + wheelRestOffset, wheelRotation, transform);
  }
}
