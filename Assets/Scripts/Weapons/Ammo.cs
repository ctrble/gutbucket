using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour {
  [SerializeField]
  private AmmoData ammoData;
  public Rigidbody ammoRB;
  public BoxCollider ammoCollider;
  private Transform parentObject;

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
  //   MoveAmmo();
  // }

  void FixedUpdate() {
    MoveAmmo();
  }

  void MoveAmmo() {
    // transform.Translate(transform.forward * ammoData.MovementSpeed * Time.deltaTime, Space.World);
    ammoRB.AddRelativeForce(Vector3.forward * ammoData.MovementSpeed * Time.deltaTime);
  }

  public void InheritVelocity(Vector3 velocity) {
    // transform.Translate(velocity);
    ammoRB.velocity = velocity + (transform.forward * ammoData.MovementSpeed * Time.deltaTime);
  }

  public void InheritParent(Transform parent) {
    // useful for filtering collisions
    parentObject = parent;
  }

  IEnumerator Lifetime() {
    yield return new WaitForSeconds(ammoData.AmmoLifeTime);
    gameObject.SetActive(false);
  }

  void OnTriggerEnter(Collider other) {
    bool isSelf = other.transform.root == parentObject;
    bool isHitLayer = GameUtilities.instance.ObjectIsInLayerMask(other.gameObject.layer, GameUtilities.instance.hitLayer);

    if (!isSelf) {
      Debug.Log(other.transform.name);
      gameObject.SetActive(false);

      if (isHitLayer) {
        Debug.Log("go boom");
      }
    }
  }

  // void CheckCollisions() {
  //   Vector3 halfExtents = ammoCollider.size * 0.5f;
  //   Collider[] hitColliders = Physics.OverlapBox(ammoRB.position, halfExtents, transform.rotation);

  //   for (int i = 0; i < hitColliders.Length; i++) {
  //     // don't shoot yourself or your siblings
  //     bool isSelf = hitColliders[i].transform.root == transform.root;
  //     bool isHitLayer = GameUtilities.instance.ObjectIsInLayerMask(hitColliders[i].gameObject.layer, GameUtilities.instance.hitLayer);

  //     if (!isSelf && isHitLayer) {
  //       Debug.Log("Hit : " + hitColliders[i].transform.root.name + i);
  //       ammoRB.velocity = Vector3.zero;
  //       // hitColliders[i].gameObject.SetActive(false);

  //       // IDamageable damagable = hitColliders[i].gameObject.GetComponent<IDamageable>();
  //       // // Vector3 direction = -DirectionToTarget(hitColliders[i].transform);
  //       // Vector3 direction = transform.forward;
  //       // float strength = 750;
  //       // float damage = 1;

  //       // damagable.Damage(direction, strength, damage);
  //       // gameObject.SetActive(false);
  //     }
  //   }
  // }
}
