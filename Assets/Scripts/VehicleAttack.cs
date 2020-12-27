using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleAttack : MonoBehaviour {

  [Header("Components")]

  public InputController playerInput;
  public Rigidbody playerRB;

  [Space]
  [Header("Aiming")]

  public Transform vehicleAim;
  public Transform barrelPosition;
  public LineRenderer lineRenderer;
  private int lengthOfLineRenderer = 2;
  public Vector3[] linePoints;
  public Vector3 toClosestTarget;
  public Collider[] potentialTargets = new Collider[10];
  public List<Transform> visibleTargets = new List<Transform>();
  public Transform currentTarget;
  public Transform previousTarget;
  public float targetChangeProgress;
  public float verticalAimSpeed;
  private float dotTolerance = 0.90f;
  public Vector3 aimYPosition;
  public Vector3 targetYPosition;

  void Start() {
    if (playerInput == null) {
      playerInput = gameObject.GetComponentInParent<InputController>();
    }

    // if (playerRB == null) {
    //   playerRB = gameObject.GetComponent<Rigidbody>();
    // }

    if (vehicleAim == null) {
      vehicleAim = transform.Find("Aim");
    }

    if (lineRenderer == null) {
      lineRenderer = vehicleAim.GetComponent<LineRenderer>();
      lineRenderer.positionCount = lengthOfLineRenderer;
      linePoints = new Vector3[lengthOfLineRenderer];
    }

    targetChangeProgress = 0;
  }

  void Update() {
    FindVisibleTargets();
    HandleAim(playerInput.AimVector());

    if (lineRenderer != null) {
      HandleRenderer();
    }

    if (playerInput.Shoot()) {
      Weapon currentWeapon = CurrentWeapon();
      currentWeapon.Attack();
    }
  }

  Weapon CurrentWeapon() {
    // TODO: cache this
    Weapon currentWeapon = transform.GetComponentInChildren<Weapon>();
    return currentWeapon;
  }

  void HandleRenderer() {
    // TODO: this probably should go somewhere else
    Weapon currentWeapon = CurrentWeapon();

    linePoints[0] = currentWeapon.transform.position;
    linePoints[1] = vehicleAim.TransformPoint(Vector3.forward * currentWeapon.weaponData.MaxRange);
    lineRenderer.SetPositions(linePoints);
  }

  void HandleAim(Vector3 direction) {
    // reset the current target
    previousTarget = currentTarget;
    currentTarget = null;

    // where are we pointing?
    Vector3 aimInput = GameUtilities.instance.ConvertInputForISO(direction);
    Vector3 aimDirection = CurrentAim(aimInput);
    // Debug.Log(aimInput);

    Weapon currentWeapon = CurrentWeapon();
    Vector3 currentAimPosition = currentWeapon.transform.position;

    if (visibleTargets.Count != 0) {
      // take the closest dot and aim at that
      float closestDot = 0;
      int targetIndex = 0;

      for (int i = 0; i < visibleTargets.Count; i++) {
        // where the target in relation to where we're currently aiming?
        Vector3 targetDirection = DirectionToTarget(visibleTargets[i].transform);
        Vector3 flattenedAim = new Vector3(aimDirection.x, 0, aimDirection.z);
        Vector3 flattenedAngle = new Vector3(targetDirection.x, 0, targetDirection.z);

        // figure out who we're aiming at
        float currentDot = Vector3.Dot(flattenedAim.normalized, flattenedAngle.normalized);
        if (currentDot > closestDot) {
          // reassign it cause it's closer
          closestDot = currentDot;
          targetIndex = i;
        }
      }
      currentTarget = visibleTargets[targetIndex].transform;

      // dumb debugger
      // foreach (Transform target in visibleTargets) {
      //   if (target == currentTarget) {
      //     Debug.DrawRay(vehicleAim.position, DirectionToTarget(currentTarget), Color.red);
      //   }
      //   else {
      //     Debug.DrawRay(vehicleAim.position, DirectionToTarget(target), Color.yellow);
      //   }
      // }
    }

    // Debug.Log(aimDirection);
    vehicleAim.rotation = Quaternion.LookRotation(aimDirection);

    if (currentTarget != null) {
      // where is the target in relation to us?
      Vector3 flattenedTargetPosition = new Vector3(currentTarget.position.x, vehicleAim.position.y, currentTarget.position.z);
      Vector3 flatDirectionToTarget = flattenedTargetPosition - vehicleAim.position;

      // Debug.DrawRay(vehicleAim.position, flatDirectionToTarget, Color.blue);
      // Debug.DrawRay(currentTarget.position, currentTarget.up * 10, Color.black);

      // get positions to calculate angles between aim and target Y positions
      aimYPosition = vehicleAim.TransformPoint(Vector3.forward * flatDirectionToTarget.magnitude);
      targetYPosition = new Vector3(aimYPosition.x, currentTarget.position.y, aimYPosition.z);

      // get directions where we're aiming vs where the target is
      Vector3 directionToAimY = aimYPosition - vehicleAim.position;
      Vector3 directionToTargetY = targetYPosition - vehicleAim.position;
      // Debug.DrawRay(vehicleAim.position, directionToAimY, Color.black);
      // Debug.DrawRay(vehicleAim.position, directionToTargetY, Color.yellow);

      // now get the signed angle between where we're aiming vs where the target would be (Y pos) along that same vector
      float angleToTarget = Vector3.SignedAngle(directionToAimY, directionToTargetY, Vector3.right);
      bool aimIsAbove = aimYPosition.y >= targetYPosition.y;
      float signedAngleToTarget = aimIsAbove ? Mathf.Abs(angleToTarget) : -Mathf.Abs(angleToTarget);

      // we finally get a rotation that can adjust the X axis up or down to the correct height
      Quaternion rotationDifference = Quaternion.AngleAxis(signedAngleToTarget, Vector3.right);

      // reset the time/progress when the target changes
      if (currentTarget != previousTarget) {
        targetChangeProgress = 0;
      }
      if (targetChangeProgress < 1) {
        targetChangeProgress += Time.deltaTime;
      }

      // TODO: maybe do an animation curve later
      // TODO: use the angle from the last target to influence how quickly it changes
      Quaternion interpolatedRotation = Quaternion.Slerp(vehicleAim.rotation, vehicleAim.rotation * rotationDifference, verticalAimSpeed * targetChangeProgress);
      vehicleAim.rotation = interpolatedRotation;
    }
  }

  Vector3 DirectionToTarget(Transform target) {
    Weapon currentWeapon = CurrentWeapon();

    // not normalized here, contains useful info about distance too
    // return target.position - vehicleAim.position;
    return target.position - currentWeapon.transform.position;
  }

  void FindVisibleTargets() {
    ResetAllTargets();

    Weapon currentWeapon = CurrentWeapon();

    bool foundPotentialTargets = Physics.OverlapSphereNonAlloc(currentWeapon.transform.position, currentWeapon.weaponData.MaxRange, potentialTargets, GameUtilities.instance.hitLayer) > 0;

    if (foundPotentialTargets) {
      for (int i = 0; i < potentialTargets.Length; i++) {
        // don't target nothing, and don't target yourself
        if (potentialTargets[i] != null && potentialTargets[i].transform.root != transform.root) {
          AddTargetIfVisible(potentialTargets[i].transform);
        }
      }

      if (visibleTargets.Count >= 2) {
        // there are at least 2, so there's something to sort
        SortTargetsByRange();
      }
    }
  }

  void ResetAllTargets() {
    // this helps make sure the targets list is accurate and not remembering old info
    // which is really important with multiple or disappearing targets
    for (int i = 0; i < potentialTargets.Length; i++) {
      potentialTargets[i] = null;
    }
    visibleTargets.Clear();
  }

  void AddTargetIfVisible(Transform target) {
    Vector3 toTarget = DirectionToTarget(target);

    // check if there's something in the way
    bool obstacleRay = Physics.Raycast(vehicleAim.position, toTarget.normalized, toTarget.magnitude, GameUtilities.instance.staticLayer);
    if (!obstacleRay) {
      visibleTargets.Add(target);
    }
  }

  void SortTargetsByRange() {
    // the closest target will be [0]
    visibleTargets.Sort((x, y) => {
      // return (transform.position - x.position).sqrMagnitude.CompareTo((transform.position - y.position).sqrMagnitude);
      return (vehicleAim.position - x.position).sqrMagnitude.CompareTo((vehicleAim.position - y.position).sqrMagnitude);
    });
  }

  Vector3 CurrentAim(Vector3 input) {
    // forward if there's no input
    Vector3 inputDirection = (input == Vector3.zero) ? transform.forward : input;

    // wrap the aiming component around the vehicle's up
    Vector3 projectedAim = Vector3.ProjectOnPlane(inputDirection, transform.up);
    Vector3 aimDirection = Vector3.RotateTowards(vehicleAim.transform.forward, projectedAim.normalized, Mathf.PI * 2, 0f);
    return aimDirection;
  }
}
