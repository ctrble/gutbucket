using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour, IDamageable {
  [SerializeField]
  private float health;
  public float maxHealth;

  public Rigidbody entityRb;
  public GameObject vehicle;


  void Start() {
    if (entityRb == null) {
      entityRb = GetComponent<Rigidbody>();
    }

    if (vehicle == null) {
      // TODO: obviously this needs to be way smarter
      vehicle = transform.Find("Vehicle").gameObject;
    }

    Health = maxHealth;
  }

  public void Damage(Vector3 direction, float strength, float damage) {
    Health -= damage;
    if (health <= 0) {
      health = 0;
      // Kill();
    }

    Debug.DrawRay(transform.position, direction * strength);
    entityRb.AddForce(direction * strength, ForceMode.Impulse);
  }

  // public void Kill() {
  //   gameObject.SetActive(false);
  // }


  public float Health {
    get { return health; }
    set {
      if (value < 0) {
        health = 0;
      }
      else if (value > maxHealth) {
        health = maxHealth;
      }
      else {
        health = value;
      }
    }
  }
}
