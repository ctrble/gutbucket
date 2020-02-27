using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Cinemachine;

public class CameraController : MonoBehaviour {
  public Camera mainCamera;
  public Transform currentCamera;
  public List<Transform> allCameras = new List<Transform>();

  void Awake() {
    if (mainCamera == null) {
      mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
    currentCamera = mainCamera.transform;

    if (allCameras.Count == 0) {
      foreach (Transform child in transform) {
        allCameras.Add(child);
      }
    }

    for (int i = 0; i < allCameras.Count; i++) {
      allCameras[i].gameObject.SetActive(i == 0);
    }
  }
}
