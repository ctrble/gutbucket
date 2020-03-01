using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraController : MonoBehaviour {
  public Camera mainCamera;
  public Transform currentCamera;
  // public List<Transform> allCameras = new List<Transform>();
  public List<CinemachineVirtualCamera> allCameras = new List<CinemachineVirtualCamera>();
  public int currentCameraIndex;
  public InputController inputController;

  void Awake() {
    if (mainCamera == null) {
      mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
    currentCamera = mainCamera.transform;

    if (allCameras.Count == 0) {
      foreach (Transform child in transform) {
        allCameras.Add(child.GetComponent<CinemachineVirtualCamera>());
      }
    }

    for (int i = 0; i < allCameras.Count; i++) {
      // allCameras[i].gameObject.SetActive(i == 0);
      allCameras[i].gameObject.SetActive(true);
      // if (allCameras[i])
      allCameras[i].m_Priority = i;
    }
    // the last in the list has the highest priority to start
    currentCameraIndex = allCameras.Count - 1;
  }

  void Start() {
    inputController = GameController.instance.player.GetComponent<InputController>();
  }

  void Update() {
    if (inputController.ChangeCamera() || Input.GetKeyDown("c")) {
      EnableNextCamera();
    }
  }

  void EnableNextCamera() {
    Debug.Log("changing camera");
    int nextIndex = (currentCameraIndex + 1 == allCameras.Count) ? 0 : currentCameraIndex + 1;
    currentCameraIndex = nextIndex;
    for (int i = 0; i < allCameras.Count; i++) {
      if (i == currentCameraIndex) {
        allCameras[i].m_Priority = allCameras.Count - 1;
      }
      else {
        allCameras[i].m_Priority = 0;
        // allCameras[i].m_Priority = currentCameraIndex - i;
      }

      // if (allCameras[i].gameObject.activeInHierarchy) {
      // // turn off the current camera
      // allCameras[i].gameObject.SetActive(false);
      // // turn on the next one
      // int next = (i + 1 == allCameras.Count) ? 0 : i + 1;
      // allCameras[next].gameObject.SetActive(true);
      // break;
      // }
    }
  }
}
