using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
  public static GameController instance = null;
  public GameObject playerPrefab;
  public List<GameObject> allPlayers = new List<GameObject>();
  public int playerCount = 1;
  private Vector3 player1SpawnPosition = Vector3.zero;
  private Vector3 player2SpawnPosition = Vector3.right * 2;

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
    SpawnPlayers();
  }

  void SpawnPlayers() {
    GameObject[] spawnedPlayers = GameObject.FindGameObjectsWithTag("Player");
    int playersToSpawn = playerCount - spawnedPlayers.Length;

    if (playersToSpawn == 0) {
      // we've got everyone, huzzah and add them to the players list
      allPlayers.Add(spawnedPlayers[0]);
    }
    else if (playersToSpawn == 1) {
      if (playerCount == 1) {
        // it's a one player game and we don't even have our player!
        GameObject player1 = Instantiate(playerPrefab, player1SpawnPosition, Quaternion.identity);
        allPlayers.Add(player1);
      }
      else {
        // we've got the first one, add them to the list
        allPlayers.Add(spawnedPlayers[0]);

        // but we still need a second player
        GameObject player2 = Instantiate(playerPrefab, player2SpawnPosition, Quaternion.identity);
        allPlayers.Add(player2);
      }
    }
    else if (playersToSpawn == 2) {
      // need both players
      GameObject player1 = Instantiate(playerPrefab, player1SpawnPosition, Quaternion.identity);
      GameObject player2 = Instantiate(playerPrefab, player2SpawnPosition, Quaternion.identity);
      allPlayers.Add(player1);
      allPlayers.Add(player2);
    }

    if (allPlayers.Count == 2) {
      // need to tell player 2 to use the right inputs, because only player 1 knows any better
      allPlayers[1].GetComponent<InputController>().playerId = 1;
    }
  }
}
