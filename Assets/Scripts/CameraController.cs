using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraController : MonoBehaviour {
  public Camera mainCamera;
  public Transform currentCamera;
  public List<CinemachineVirtualCamera> allCameras = new List<CinemachineVirtualCamera>();
  public int currentCameraIndex;
  public InputController inputController;

  void Awake() {
    if (mainCamera == null) {
      mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
    currentCamera = mainCamera.transform;
  }

  void CamerasInit() {
    // gather all of them
    if (allCameras.Count == 0) {
      foreach (Transform child in transform) {
        allCameras.Add(child.GetComponent<CinemachineVirtualCamera>());
      }
    }

    // turn on, set priority by hierarchy
    for (int i = 0; i < allCameras.Count; i++) {
      allCameras[i].gameObject.SetActive(true);
      allCameras[i].m_Priority = i;
    }
    currentCameraIndex = allCameras.Count - 1;
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
    int nextIndex = (currentCameraIndex + 1 == allCameras.Count) ? 0 : currentCameraIndex + 1;
    currentCameraIndex = nextIndex;
    for (int i = 0; i < allCameras.Count; i++) {
      if (i == currentCameraIndex) {
        allCameras[i].m_Priority = allCameras.Count - 1;
      }
      else {
        allCameras[i].m_Priority = 0;
      }
    }
  }
}
