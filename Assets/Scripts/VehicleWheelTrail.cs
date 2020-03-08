using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleWheelTrail : MonoBehaviour {
  public float maxLineSegmentLength = 1f;
  public int maxLineLength = 20;
  [SerializeField]
  private int currentLineLength = 1;
  public LineRenderer line;
  public Vector3 newPosition;
  public Vector3 lastPosition;
  public VehicleWheel vehicleWheel;

  void OnEnable() {
    if (vehicleWheel == null) {
      vehicleWheel = GetComponent<VehicleWheel>();
    }

    if (line == null) {
      line = GetComponent<LineRenderer>();
    }

    // Ray ray = new Ray(transform.position, Vector3.down);
    // RaycastHit hit;
    // //Debug.DrawRay(ray.origin, ray.direction, Color.red, 2f);
    // if (Physics.Raycast(ray, out hit)) {
    //   lastPosition = hit.point;
    //   line.SetPosition(0, lastPosition);
    // }

    if (isGrounded()) {
      lastPosition = vehicleWheel.groundPoint;
      line.SetPosition(0, lastPosition);
    }

    // Linerenderer stuff
    line.positionCount = maxLineLength;
  }

  void Update() {
    // Get new position.
    // Ray ray = new Ray(transform.position, Vector3.down);
    // RaycastHit hit;

    // Debug.DrawRay(ray.origin, ray.direction, Color.red, 2f);
    // if (Physics.Raycast(ray, out hit)) {
    if (isGrounded()) {
      newPosition = vehicleWheel.groundPoint;

      // If position is greater than line length create a new line
      // and set new last position
      Vector3 distance = lastPosition - newPosition;
      if (distance.sqrMagnitude > Mathf.Sqrt(maxLineSegmentLength)) {
        // Debug.DrawLine(lastPosition, newPosition, Color.green, 5f);

        line.SetPosition(currentLineLength, newPosition);
        lastPosition = newPosition;
        currentLineLength++;
      }
      line.positionCount = currentLineLength + 1;
    }

  }

  bool isGrounded() {
    return vehicleWheel.grounded && vehicleWheel.groundPoint != Vector3.zero;
  }
}
