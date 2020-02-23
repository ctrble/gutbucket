using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
  public Rigidbody playerRB;

  void Start() {
    if (playerRB == null) {
      playerRB = gameObject.GetComponent<Rigidbody>();
    }
  }
}
