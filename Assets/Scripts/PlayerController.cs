using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable {
  public Rigidbody playerRB;

  void OnEnable() {
    if (playerRB == null) {
      playerRB = gameObject.GetComponent<Rigidbody>();
    }
  }

  public void Damage(Vector3 direction, float strength, float damage) {
    Debug.Log("damage!");

    playerRB.AddForce(direction * strength, ForceMode.Impulse);
  }
}
