using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour {
  [SerializeField]
  private AmmoData ammoData;
  public Rigidbody ammoRB;
  public BoxCollider ammoCollider;

  void OnEnable() {
    if (ammoRB == null) {
      ammoRB = gameObject.GetComponent<Rigidbody>();
    }

    if (ammoCollider == null) {
      ammoCollider = gameObject.GetComponent<BoxCollider>();
    }

    StartCoroutine(Lifetime());
  }

  // void Update() {
  //   transform.Translate(transform.forward * ammoData.MovementSpeed * Time.deltaTime, Space.World);
  // }

  void FixedUpdate() {
    MoveAmmo();
  }

  void MoveAmmo() {
    ammoRB.AddRelativeForce(Vector3.forward * ammoData.MovementSpeed * Time.deltaTime);
  }

  public void InheritVelocity(Vector3 velocity) {
    // transform.Translate(velocity);
    ammoRB.velocity = velocity + (transform.forward * ammoData.MovementSpeed * Time.deltaTime);
  }

  IEnumerator Lifetime() {
    yield return new WaitForSeconds(ammoData.AmmoLifeTime);
    gameObject.SetActive(false);
  }
}
