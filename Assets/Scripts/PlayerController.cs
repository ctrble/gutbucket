using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
  public Rigidbody playerRB;

  void OnEnable() {
    if (playerRB == null) {
      playerRB = gameObject.GetComponent<Rigidbody>();
    }
  }
}
