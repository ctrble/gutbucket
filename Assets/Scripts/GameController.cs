using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
  public static GameController instance = null;
  public GameObject playerPrefab;
  public List<GameObject> allPlayers = new List<GameObject>();
  public int playerCount = 1;

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
    if (playerCount == 1) {
      // there's not a player yet, spawn one
      if (!GameObject.FindWithTag("Player")) {
        Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
      }

      // add the player to the players list
      if (allPlayers.Count == 0) {
        GameObject[] currentPlayers = GameObject.FindGameObjectsWithTag("Player");
        allPlayers.Add(currentPlayers[0]);
      }
    }
    else if (playerCount > 1) {
      GameObject[] currentPlayers = GameObject.FindGameObjectsWithTag("Player");

      // how many more do we need?
      int playersToSpawn = playerCount - currentPlayers.Length;
      if (playersToSpawn == 0) {
        Debug.Log("no need to spawn");
      }
      else if (playersToSpawn > 0) {
        Debug.Log($"need {playersToSpawn} more players");

        for (int i = 1; i <= playersToSpawn; i++) {
          // make another player
          Vector3 spawnPosition = (playersToSpawn % 2 == 0) ? Vector3.right * (i + 1) : Vector3.left * (i + 1);
          GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

          // set the player #
          player.GetComponent<InputController>().playerId = i;
        }
      }

      // update our lists of players
      currentPlayers = GameObject.FindGameObjectsWithTag("Player");
      foreach (GameObject player in currentPlayers) {
        allPlayers.Add(player);
      }
    }
  }
}
