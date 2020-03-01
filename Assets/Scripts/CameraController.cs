using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraController : MonoBehaviour {
  public static CameraController instance = null;
  public Camera mainCamera;
  public Transform currentCamera;
  public List<CinemachineVirtualCamera> allCameras = new List<CinemachineVirtualCamera>();
  public GameObject closeCameraPrefab;
  public GameObject midCameraPrefab;
  public GameObject farCameraPrefab;
  public int currentCameraIndex;
  public InputController inputController;

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
    if (mainCamera == null) {
      mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
    currentCamera = mainCamera.transform;

    // this might need some help
    // if only one player...
    if (GameController.instance.playerCount == 1) {
      inputController = GameController.instance.allPlayers[0].GetComponent<InputController>();
    }
    else {
      // TODO: well this is a hot mess
      // clone the camera
      GameObject secondCameraObject = Instantiate(mainCamera.gameObject, mainCamera.transform.position, mainCamera.transform.rotation);
      AudioListener cameraAudio = secondCameraObject.GetComponent<AudioListener>();
      cameraAudio.enabled = false;
      Camera cameraComponent = secondCameraObject.GetComponent<Camera>();

      // position x y, size x y for split screen
      mainCamera.rect = new Rect(0, 0.5f, 1, 1);
      cameraComponent.rect = new Rect(0, 0, 1, 0.5f);

      // the layers to exclude
      int p1Layer = LayerMask.NameToLayer("P1");
      int p2Layer = LayerMask.NameToLayer("P2");

      // current culling masks
      int currentP1Mask = mainCamera.cullingMask;
      int currentP2Mask = cameraComponent.cullingMask;

      // new culling masks
      int newP1Mask = currentP1Mask & ~(1 << p2Layer);
      int newP2Mask = currentP1Mask & ~(1 << p1Layer);

      // set them
      mainCamera.cullingMask = newP1Mask;
      cameraComponent.cullingMask = newP2Mask;

      // spawn vCams for the second player
      GameObject closeCamera = Instantiate(closeCameraPrefab, transform.position, Quaternion.identity, transform);
      GameObject midCamera = Instantiate(midCameraPrefab, transform.position, Quaternion.identity, transform);
      GameObject farCamera = Instantiate(farCameraPrefab, transform.position, Quaternion.identity, transform);

      closeCamera.GetComponent<CinemachineVirtualCamera>().m_Follow = GameController.instance.allPlayers[1].transform;
      midCamera.GetComponent<CinemachineVirtualCamera>().m_Follow = GameController.instance.allPlayers[1].transform;
      farCamera.GetComponent<CinemachineVirtualCamera>().m_Follow = GameController.instance.allPlayers[1].transform;

      closeCamera.GetComponent<CinemachineVirtualCamera>().m_LookAt = GameController.instance.allPlayers[1].transform;
      midCamera.GetComponent<CinemachineVirtualCamera>().m_LookAt = GameController.instance.allPlayers[1].transform;
      farCamera.GetComponent<CinemachineVirtualCamera>().m_LookAt = GameController.instance.allPlayers[1].transform;

      closeCamera.gameObject.layer = p2Layer;
      midCamera.gameObject.layer = p2Layer;
      farCamera.gameObject.layer = p2Layer;
    }
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

  void Update() {
    if (GameController.instance.playerCount == 1) {
      if (inputController.ChangeCamera()) {
        EnableNextCamera();
      }
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
