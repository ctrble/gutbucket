using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
  public static GameController instance = null;
  public GameObject playerPrefab;
  public GameObject player;


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
    if (!GameObject.FindWithTag("Player")) {
      Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
    }

    if (player == null) {
      player = GameObject.FindGameObjectWithTag("Player");
    }
  }
}
