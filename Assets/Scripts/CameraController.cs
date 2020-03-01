using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraController : MonoBehaviour {
  public static CameraController instance = null;
  public Camera mainCamera;
  public Camera secondaryCamera;
  public Transform currentCamera;
  public List<CinemachineVirtualCamera> allCameras = new List<CinemachineVirtualCamera>();
  public GameObject closeCameraPrefab;
  public GameObject midCameraPrefab;
  public GameObject farCameraPrefab;
  public int currentCameraIndex;
  public List<InputController> inputControllers = new List<InputController>();

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
    // get the basic camera info
    if (mainCamera == null) {
      mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
    currentCamera = mainCamera.transform;

    if (GameController.instance.playerCount == 1) {
      // get the first player's controller, that's all folks!
      inputControllers.Add(GameController.instance.allPlayers[0].GetComponent<InputController>());
    }
    else {
      SetUpSplitScreen();
    }
  }

  void CloneMainCamera() {
    // clone the camera, set as secondary
    GameObject secondCameraObject = Instantiate(mainCamera.gameObject, mainCamera.transform.position, mainCamera.transform.rotation);
    secondaryCamera = secondCameraObject.GetComponent<Camera>();

    // there can only be one audio listener, disable it
    AudioListener cameraAudio = secondCameraObject.GetComponent<AudioListener>();
    cameraAudio.enabled = false;
  }

  void ResizeViewPorts() {
    // position x y, size x y for split screen
    mainCamera.rect = new Rect(0, 0.5f, 1, 0.5f);
    secondaryCamera.rect = new Rect(0, 0, 1, 0.5f);
  }

  void SetCullingMasks(int p1Layer, int p2Layer) {
    // current culling masks
    int currentP1Mask = mainCamera.cullingMask;
    int currentP2Mask = secondaryCamera.cullingMask;

    // new culling masks
    int newP1Mask = currentP1Mask & ~(1 << p2Layer);
    int newP2Mask = currentP1Mask & ~(1 << p1Layer);

    // set the layers on the cameras and players
    mainCamera.cullingMask = newP1Mask;
    secondaryCamera.cullingMask = newP2Mask;
  }

  void SetLayerMasks(int p1Layer, int p2Layer) {
    GameController.instance.allPlayers[0].layer = p1Layer;
    GameController.instance.allPlayers[1].layer = p2Layer;
  }

  void SpawnPlayer2Vcam(GameObject cameraPrefab, int p2Layer) {
    GameObject newCamera = Instantiate(cameraPrefab, transform.position, Quaternion.identity, transform);
    CinemachineVirtualCamera newVCam = newCamera.GetComponent<CinemachineVirtualCamera>();

    // track the player
    newVCam.m_Follow = GameController.instance.allPlayers[1].transform;
    newVCam.m_LookAt = GameController.instance.allPlayers[1].transform;
    newCamera.gameObject.layer = p2Layer;
  }

  void GetAllInputs() {
    inputControllers.Add(GameController.instance.allPlayers[0].GetComponent<InputController>());
    inputControllers.Add(GameController.instance.allPlayers[1].GetComponent<InputController>());
  }

  void SetUpSplitScreen() {
    int p1Layer = LayerMask.NameToLayer("P1");
    int p2Layer = LayerMask.NameToLayer("P2");

    CloneMainCamera();
    ResizeViewPorts();
    SetCullingMasks(p1Layer, p2Layer);
    SetLayerMasks(p1Layer, p2Layer);

    SpawnPlayer2Vcam(closeCameraPrefab, p2Layer);
    SpawnPlayer2Vcam(midCameraPrefab, p2Layer);
    SpawnPlayer2Vcam(farCameraPrefab, p2Layer);

    GetAllInputs();
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
      if (inputControllers[0].ChangeCamera()) {
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
