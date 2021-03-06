﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New WeaponData", menuName = "Weapon Data", order = 51)]
public class WeaponData : ScriptableObject {

  [SerializeField]
  private string weaponName;
  [SerializeField]
  private float fireRate;
  [SerializeField]
  private float maxRange;
  [SerializeField]
  private int maxAmmo;
  [SerializeField]
  private Vector3 spawnPosition;
  [SerializeField]
  private GameObject ammoPrefab;

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

  public float MaxRange {
    get {
      return maxRange;
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
}
