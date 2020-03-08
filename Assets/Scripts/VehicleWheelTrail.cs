using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Adapted from https://github.com/Nition/UnitySkidmarks
public class VehicleWheelTrail : MonoBehaviour {
  private VehicleWheel vehicleWheel;
  private Rigidbody vehicleRB;
  private VehicleMovement vehicleMovement;

  [Space]
  [Header("Settings")]

  // Min side slip speed in m/s to start showing a skid
  public float skidFxSpeed = 0.5f;
  // m/s where skid opacity is at full intensity
  public float maxSkidIntensity = 20.0f;
  // For wheelspin. Adjust how much skids show
  public float wheelSlipModifier = 10.0f;
  // Array index for the skidmarks controller. Index of last skidmark piece this wheel used
  private int lastSkid = -1;

  void OnEnable() {
    if (vehicleWheel == null) {
      vehicleWheel = GetComponent<VehicleWheel>();
    }

    if (vehicleRB == null) {
      vehicleRB = gameObject.GetComponentInParent<Rigidbody>();
    }

    if (vehicleMovement == null) {
      vehicleMovement = gameObject.GetComponentInParent<VehicleMovement>();
    }
  }

  void LateUpdate() {
    if (isGrounded()) {
      // Gives velocity with +z being the car's forward axis
      Vector3 localVelocity = transform.InverseTransformDirection(vehicleRB.velocity);
      float skidTotal = Mathf.Abs(localVelocity.x);

      // Check wheel spin as well
      float wheelAngularVelocity = vehicleWheel.wheelRadiusY * ((2 * Mathf.PI * vehicleMovement.currentSpeed) / 60);
      float wheelSpin = Mathf.Abs(vehicleMovement.forwardVelocity - wheelAngularVelocity) * wheelSlipModifier;

      // fades out the wheelspin-based skid as speed increases
      wheelSpin = Mathf.Max(0, wheelSpin * (10 - Mathf.Abs(vehicleMovement.forwardVelocity)));

      skidTotal += wheelSpin;

      // Skid if we should
      if (skidTotal >= skidFxSpeed) {
        float intensity = Mathf.Clamp01(skidTotal / maxSkidIntensity);
        // Account for further movement since the last FixedUpdate
        Vector3 skidPoint = vehicleWheel.groundPoint + (vehicleRB.velocity * Time.fixedDeltaTime);
        lastSkid = TireSkidController.instance.AddSkidMark(skidPoint, vehicleWheel.groundNormal, intensity, lastSkid);
      }
      else {
        lastSkid = -1;
      }
    }
    else {
      lastSkid = -1;
    }
  }

  bool isGrounded() {
    return vehicleWheel.grounded && vehicleWheel.groundPoint != Vector3.zero;
  }
}
