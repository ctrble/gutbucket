using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour {
  [SerializeField]
  private AmmoData ammoData;
  public Rigidbody ammoRB;
  public BoxCollider ammoCollider;
  public float speed;

  void OnEnable() {
    if (ammoRB == null) {
      ammoRB = gameObject.GetComponent<Rigidbody>();
    }

    if (ammoCollider == null) {
      ammoCollider = gameObject.GetComponent<BoxCollider>();
    }

    speed = ammoData.MovementSpeed;
    StartCoroutine(Lifetime());
  }

  // void Update() {
  //   transform.Translate(transform.forward * ammoData.MovementSpeed * Time.deltaTime, Space.World);
  // }

  void FixedUpdate() {
    // ammoRB.AddRelativeForce(transform.forward * ammoData.MovementSpeed * Time.deltaTime);
    ammoRB.AddRelativeForce(Vector3.forward * ammoData.MovementSpeed * Time.deltaTime);
  }

  // public void InheritVelocity(Vector3 velocity) {
  //   transform.Translate(velocity);
  // }

  IEnumerator Lifetime() {
    yield return new WaitForSeconds(ammoData.AmmoLifeTime);
    gameObject.SetActive(false);
  }
}
