using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
  public static GameController instance = null;

  public GameObject playerPrefab;

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

  void OnEnable() {
    GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
  }
}
