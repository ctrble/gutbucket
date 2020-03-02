using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraController : MonoBehaviour {
  public static CameraController instance = null;
  public Camera mainCamera;
  public Camera secondaryCamera;
  public Transform currentCamera;
  public List<CinemachineVirtualCamera> player1Cameras = new List<CinemachineVirtualCamera>();
  public List<CinemachineVirtualCamera> player2Cameras = new List<CinemachineVirtualCamera>();
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

    // prep for player 1
    foreach (Transform child in transform) {
      player1Cameras.Add(child.GetComponent<CinemachineVirtualCamera>());
    }
    TrackPlayer(GameController.instance.allPlayers[0].transform, player1Cameras);

    if (GameController.instance.playerCount > 1) {
      SetUpSplitScreen();
    }

    GetAllInputs();
  }

  void SetUpSplitScreen() {
    int p1Layer = LayerMask.NameToLayer("P1");
    int p2Layer = LayerMask.NameToLayer("P2");

    CloneMainCamera();
    ResizeViewPorts();

    SpawnPlayer2Vcam(closeCameraPrefab, p2Layer);
    SpawnPlayer2Vcam(midCameraPrefab, p2Layer);
    SpawnPlayer2Vcam(farCameraPrefab, p2Layer);

    SetCullingMasks(p1Layer, p2Layer);
    SetLayerMasks(p1Layer, p2Layer);

    TrackPlayer(GameController.instance.allPlayers[1].transform, player2Cameras);
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

  void SpawnPlayer2Vcam(GameObject cameraPrefab, int p2Layer) {
    GameObject newCamera = Instantiate(cameraPrefab, transform.position, Quaternion.identity, transform);
    CinemachineVirtualCamera newVCam = newCamera.GetComponent<CinemachineVirtualCamera>();
    player2Cameras.Add(newVCam);
  }

  void SetCullingMasks(int p1Layer, int p2Layer) {
    // current culling masks
    int currentP1Mask = mainCamera.cullingMask;
    int currentP2Mask = secondaryCamera.cullingMask;

    // new culling masks, removes each player from the other's layers
    int newP1Mask = currentP1Mask & ~(1 << p2Layer);
    int newP2Mask = currentP1Mask & ~(1 << p1Layer);

    // set the layers on the cameras and players
    mainCamera.cullingMask = newP1Mask;
    secondaryCamera.cullingMask = newP2Mask;
  }

  void SetLayerMasks(int p1Layer, int p2Layer) {
    GameController.instance.allPlayers[0].layer = p1Layer;
    GameController.instance.allPlayers[1].layer = p2Layer;

    foreach (CinemachineVirtualCamera camera in player1Cameras) {
      camera.gameObject.layer = p1Layer;
    }

    foreach (CinemachineVirtualCamera camera in player2Cameras) {
      camera.gameObject.layer = p2Layer;
    }
  }

  void TrackPlayer(Transform player, List<CinemachineVirtualCamera> cameras) {
    foreach (CinemachineVirtualCamera camera in cameras) {
      camera.m_Follow = player;
      camera.m_LookAt = player;
    }
  }

  void GetAllInputs() {
    foreach (GameObject player in GameController.instance.allPlayers) {
      inputControllers.Add(player.GetComponent<InputController>());
    }
  }

  void CamerasInit() {
    // gather all of them
    if (player1Cameras.Count == 0) {
      foreach (Transform child in transform) {
        player1Cameras.Add(child.GetComponent<CinemachineVirtualCamera>());
      }
    }

    // turn on, set priority by hierarchy
    for (int i = 0; i < player1Cameras.Count; i++) {
      player1Cameras[i].gameObject.SetActive(true);
      player1Cameras[i].m_Priority = i;
    }
    currentCameraIndex = player1Cameras.Count - 1;
  }

  void Update() {
    if (GameController.instance.playerCount == 1) {
      if (inputControllers[0].ChangeCamera()) {
        EnableNextCamera();
      }
    }
  }

  void EnableNextCamera() {
    int nextIndex = (currentCameraIndex + 1 == player1Cameras.Count) ? 0 : currentCameraIndex + 1;
    currentCameraIndex = nextIndex;
    for (int i = 0; i < player1Cameras.Count; i++) {
      if (i == currentCameraIndex) {
        player1Cameras[i].m_Priority = player1Cameras.Count - 1;
      }
      else {
        player1Cameras[i].m_Priority = 0;
      }
    }
  }
}
