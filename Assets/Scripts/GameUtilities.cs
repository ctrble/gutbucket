using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtilities : MonoBehaviour {
  public static GameUtilities instance = null;
  public Transform forwardReference;
  public LayerMask staticLayer;
  public LayerMask hitLayer;

  void Awake() {
    CreateSingleton();
  }

  void CreateSingleton() {
    if (instance == null)
      instance = this;
    else if (instance != this)
      Destroy(gameObject);

    DontDestroyOnLoad(gameObject);
  }

  void Start() {
    forwardReference = CameraController.instance.mainCamera.transform;
  }

  public bool ObjectIsInLayerMask(int layer, LayerMask layerMask) {
    // https://answers.unity.com/questions/150690/using-a-bitwise-operator-with-layermask.html
    // https://answers.unity.com/questions/50279/index.html

    // gameObject.layer is already an int, but it's not bitwise
    int bitShiftLayer = 1 << layer;
    // combine layer values, if they're over 0, then they're on the same layer
    return (bitShiftLayer & hitLayer.value) > 0;
  }

  public Vector3 GetRelativeAngles(Vector3 angles) {
    // Convert angles above 180 degrees into negative/relative angles
    Vector3 relativeAngles = angles;
    if (relativeAngles.x > 180f)
      relativeAngles.x -= 360f;
    if (relativeAngles.y > 180f)
      relativeAngles.y -= 360f;
    if (relativeAngles.z > 180f)
      relativeAngles.z -= 360f;
    return relativeAngles;
  }

  public Vector3 ConvertInputForISO(Vector3 direction) {
    Vector3 currentForward = forwardReference.eulerAngles;

    // the only axis that matters is Y, reset the others
    currentForward.x = 0;
    currentForward.z = 0;
    return Quaternion.Euler(currentForward) * direction;
  }

  public float ConvertToKPH(float metersPerSecond) {
    // 1 meter per second = 3.6 kilometers per hour
    return metersPerSecond * 3.6f;
  }

  public float ConvertToMPH(float metersPerSecond) {
    // 1 meter per second = 2.23693629 miles per hour
    return metersPerSecond * 2.23693629f;
  }

  public float ConvertFromKPH(float metersPerSecond) {
    // 1 kilometer per hour = 0.277777778 meters per second
    return metersPerSecond * 0.277777778f;
  }

  public float ConvertFromMPH(float metersPerSecond) {
    // 1 mile per hour = 0.44704 meters per second
    return metersPerSecond * 0.44704f;
  }

  public Vector3 StableBackwardsPD(Vector3 positionDifference, Vector3 velocityDifference, float frequency, float damping) {
    // damping = 1, the system is critically damped
    // damping > 1 the system is over damped (sluggish)
    // damping is < 1 the system is under damped (it will oscillate a little)
    // Frequency is the speed of convergence. If damping is 1, frequency is the 1 / time taken to reach ~95 % of the target value.i.e.a frequency of 6 will bring you very close to your target within 1/6 seconds.

    //  proportional gain
    float kp = (6f * frequency) * (6f * frequency) * 0.25f;

    //  derivative gain
    float kd = 4.5f * frequency * damping;

    // it's stable because it works backwards from the desired value next frame
    float deltaTime = Time.deltaTime;
    float nextFrameTime = 1 / (1 + kd * deltaTime + kp * deltaTime * deltaTime);
    float positionTimeInterval = kp * nextFrameTime;
    float speedTimeInterval = (kd + kp * deltaTime) * nextFrameTime;

    return positionDifference * positionTimeInterval + velocityDifference * speedTimeInterval;
  }

  public float StableBackwardsPD(float positionDifference, float velocityDifference, float frequency, float damping) {
    // damping = 1, the system is critically damped
    // damping > 1 the system is over damped (sluggish)
    // damping is < 1 the system is under damped (it will oscillate a little)
    // Frequency is the speed of convergence. If damping is 1, frequency is the 1 / time taken to reach ~95 % of the target value.i.e.a frequency of 6 will bring you very close to your target within 1/6 seconds.

    //  proportional gain
    float kp = (6f * frequency) * (6f * frequency) * 0.25f;

    //  derivative gain
    float kd = 4.5f * frequency * damping;

    // it's stable because it works backwards from the desired value next frame
    float deltaTime = Time.deltaTime;
    float nextFrameTime = 1 / (1 + kd * deltaTime + kp * deltaTime * deltaTime);
    float positionTimeInterval = kp * nextFrameTime;
    float speedTimeInterval = (kd + kp * deltaTime) * nextFrameTime;

    return positionDifference * positionTimeInterval + velocityDifference * speedTimeInterval;
  }
}
