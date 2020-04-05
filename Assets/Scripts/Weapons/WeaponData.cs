using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New WeaponData", menuName = "Weapon Data", order = 51)]
public class WeaponData : ScriptableObject {
  // [SerializeField]
  // private AmmoData

  [SerializeField]
  private string weaponName;
  [SerializeField]
  private float fireRate;
  [SerializeField]
  private int maxAmmo;
  [SerializeField]
  private Vector3 spawnPosition;
  [SerializeField]
  private GameObject ammoPrefab;
  // [SerializeField]
  // private GameObject modelPrefab;

  public string WeaponName {
    get {
      return weaponName;
    }
  }

  public float FireRate {
    get {
      return fireRate;
    }
  }

  public int MaxAmmo {
    get {
      return maxAmmo;
    }
  }

  public Vector3 SpawnPosition {
    get {
      return spawnPosition;
    }
  }

  public GameObject AmmoPrefab {
    get {
      return ammoPrefab;
    }
  }

  // public GameObject ModelPrefab {
  //   get {
  //     return modelPrefab;
  //   }
  // }
}
