using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New AmmoData", menuName = "Ammo Data", order = 52)]
public class AmmoData : ScriptableObject {
  [SerializeField]
  private string ammoName;
  [SerializeField]
  private float damageAmount;
  [SerializeField]
  private Vector3 impactForce;
  [SerializeField]
  private float ammoLifetime;
  [SerializeField]
  private float movementSpeed;
  // [SerializeField]
  // private GameObject modelPrefab;

  public string AmmoName {
    get {
      return ammoName;
    }
  }

  public float DamageAmount {
    get {
      return damageAmount;
    }
  }

  public Vector3 ImpactForce {
    get {
      return impactForce;
    }
  }

  public float AmmoLifeTime {
    get {
      return ammoLifetime;
    }
  }

  public float MovementSpeed {
    get {
      return movementSpeed;
    }
  }

  // public GameObject ModelPrefab {
  //   get {
  //     return modelPrefab;
  //   }
  // }
}
