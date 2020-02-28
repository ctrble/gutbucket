using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Cinemachine;

public class CameraController : MonoBehaviour {
  public Camera mainCamera;
  public Transform currentCamera;
  public List<Transform> allCameras = new List<Transform>();
  public InputController inputController;

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

  void Start() {
    inputController = GameController.instance.player.GetComponent<InputController>();
  }

  void Update() {
    if (inputController.ChangeCamera()) {
      EnableNextCamera();
    }
  }

  void EnableNextCamera() {
    for (int i = 0; i < allCameras.Count; i++) {
      if (allCameras[i].gameObject.activeInHierarchy) {
        // turn off the current camera
        allCameras[i].gameObject.SetActive(false);

        // turn on the next one
        int next = (i + 1 == allCameras.Count) ? 0 : i + 1;
        allCameras[next].gameObject.SetActive(true);

        break;
      }
    }
  }
}
