using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraController : MonoBehaviour {
  public static CameraController instance = null;
  public Camera mainCamera;
  public Camera secondaryCamera;
  public GameObject closeCameraPrefab;
  public GameObject midCameraPrefab;
  public GameObject farCameraPrefab;
  public List<CinemachineVirtualCamera> player1Cameras = new List<CinemachineVirtualCamera>();
  public List<CinemachineVirtualCamera> player2Cameras = new List<CinemachineVirtualCamera>();
  public List<int> currentCameraIndexes;
  public int player1Index = 0;
  public int player2Index = 1;
  public List<InputController> inputControllers = new List<InputController>();
  public bool isVerticalSplit;

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

    // prep for player 1
    foreach (Transform child in transform) {
      player1Cameras.Add(child.GetComponent<CinemachineVirtualCamera>());
    }
    TrackPlayer(GameController.instance.allPlayers[0].transform, player1Cameras);

    // prep for player 2
    if (GameController.instance.playerCount > 1) {
      SetUpSplitScreen();
    }

    GetAllInputs();
    CamerasInit();
  }

  void SetUpSplitScreen() {
    int p1Layer = LayerMask.NameToLayer("P1");
    int p2Layer = LayerMask.NameToLayer("P2");

    CloneMainCamera();
    ResizeViewPorts();

    // spawn order for priority
    SpawnPlayer2Vcam(midCameraPrefab, p2Layer);
    SpawnPlayer2Vcam(farCameraPrefab, p2Layer);
    SpawnPlayer2Vcam(closeCameraPrefab, p2Layer);

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
    Rect player1Horizontal = new Rect(0, 0.5f, 1, 0.5f);
    Rect player2Horizontal = new Rect(0, 0, 1, 0.5f);
    Rect player1Vertical = new Rect(0, 0, 0.5f, 1);
    Rect player2Vertical = new Rect(0.5f, 0, 0.5f, 1);

    if (isVerticalSplit) {
      mainCamera.rect = player1Vertical;
      secondaryCamera.rect = player2Vertical;
    }
    else {
      mainCamera.rect = player1Horizontal;
      secondaryCamera.rect = player2Horizontal;
    }
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
    // turn on for player 1, set priority by hierarchy
    for (int i = 0; i < player1Cameras.Count; i++) {
      player1Cameras[i].gameObject.SetActive(true);
      player1Cameras[i].m_Priority = i;
    }
    currentCameraIndexes.Add(player1Cameras.Count - 1);

    if (GameController.instance.playerCount > 1) {
      // turn on for player 2, set priority by hierarchy
      for (int i = 0; i < player2Cameras.Count; i++) {
        player2Cameras[i].gameObject.SetActive(true);
        player2Cameras[i].m_Priority = i;
      }

      currentCameraIndexes.Add(player2Cameras.Count - 1);
    }
  }

  void Update() {
    if (inputControllers[0].ChangeCamera()) {
      EnableNextCamera(player1Cameras, player1Index);
    }

    if (GameController.instance.playerCount > 1) {
      if (inputControllers[1].ChangeCamera()) {
        EnableNextCamera(player2Cameras, player2Index);
      }

      if (inputControllers[0].ToggleSplitScreen() || inputControllers[1].ToggleSplitScreen()) {
        // change split screen orientation
        isVerticalSplit = !isVerticalSplit;
        ResizeViewPorts();
      }
    }
  }

  void EnableNextCamera(List<CinemachineVirtualCamera> cameras, int cameraIndex) {
    // cycle through the cameras, looping to the beginning if needed
    int nextIndex = (currentCameraIndexes[cameraIndex] + 1 == cameras.Count) ? 0 : currentCameraIndexes[cameraIndex] + 1;
    currentCameraIndexes[cameraIndex] = nextIndex;

    for (int i = 0; i < cameras.Count; i++) {
      if (i == currentCameraIndexes[cameraIndex]) {
        // set as highest priority
        cameras[i].m_Priority = cameras.Count - 1;
      }
      else {
        // reset priority
        cameras[i].m_Priority = 0;
      }
    }
  }
}
