using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
  public Rigidbody parentRB;
  [SerializeField]
  private WeaponData weaponData;
  public List<GameObject> pooledAmmo;

  void OnEnable() {
    if (parentRB == null) {
      parentRB = transform.root.GetComponent<Rigidbody>();
    }

    PoolObjects();
    InvokeRepeating("CheckParent", 0.5f, 0.5f);
  }

  void OnDisable() {
    CancelInvoke();
  }

  void PoolObjects() {
    if (weaponData.AmmoPrefab != null) {
      pooledAmmo = new List<GameObject>();
      for (int i = 0; i < weaponData.MaxAmmo; i++) {
        GameObject obj = Instantiate(weaponData.AmmoPrefab, transform);
        obj.SetActive(false);
        pooledAmmo.Add(obj);
      }
    }
  }

  void CheckParent() {
    // any ammo still "owned" by this weapon should return to it once it's job is done
    for (int i = 0; i < pooledAmmo.Count; i++) {
      if (!pooledAmmo[i].activeInHierarchy) {
        ResetParent(pooledAmmo[i]);
      }
    }
  }

  void ResetParent(GameObject ammo) {
    // return the ammo home
    if (ammo.transform.parent == null) {
      ammo.transform.parent = transform;
    }
  }

  public GameObject GetPooledObject() {
    for (int i = 0; i < pooledAmmo.Count; i++) {
      if (!pooledAmmo[i].activeInHierarchy) {
        // bring it home before using it, in case it got missed
        ResetParent(pooledAmmo[i]);
        return pooledAmmo[i];
      }
    }
    return null;
  }

  public void Attack() {
    GameObject ammoObject = GetPooledObject();
    if (ammoObject != null) {
      Rigidbody ammoRB = ammoObject.GetComponent<Rigidbody>();
      Ammo ammo = ammoObject.GetComponent<Ammo>();

      // spawn ammo at the barrel and pointed forward
      // unset the parent (temporarily) so it can fly freely and live an independent life
      ammoObject.transform.localPosition = weaponData.SpawnPosition;
      ammoObject.transform.forward = transform.forward;
      ammoObject.transform.parent = null;
      ammoObject.SetActive(true);

      // give it the same velocity as the current object so it doesn't look like it's slow
      Vector3 inheritedVelocity = parentRB.GetPointVelocity(ammoObject.transform.position);
      ammo.InheritParent(transform.root);
      ammo.InheritVelocity(inheritedVelocity);

      // forget me now, job's done
      ammoObject = null;
    }
  }
}
