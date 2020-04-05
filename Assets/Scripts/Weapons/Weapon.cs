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
    for (int i = 0; i < pooledAmmo.Count; i++) {
      if (!pooledAmmo[i].activeInHierarchy) {
        ResetParent(pooledAmmo[i]);
      }
    }
  }

  void ResetParent(GameObject ammo) {
    // return ammo to the source
    if (ammo.transform.parent == null) {
      ammo.transform.parent = transform;
    }
  }

  public GameObject GetPooledObject() {
    for (int i = 0; i < pooledAmmo.Count; i++) {
      if (!pooledAmmo[i].activeInHierarchy) {
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

      ammoObject.transform.localPosition = weaponData.SpawnPosition;
      ammoObject.transform.forward = transform.forward;
      ammoObject.transform.parent = null;

      // Assumes the current class has access to the rotation rate of the player
      // float rotationRateDegrees = 0f;
      // float bulletAngularVelocityRad = Mathf.Deg2Rad * rotationRateDegrees;

      // The radius of the circular motion is the distance between the bullet
      // spawn point and the player's axis of rotation
      // float bulletRotationRadius = (ammoObject.transform.position - transform.position).magnitude;

      ammoObject.SetActive(true);

      // give it the same velocity as the current object so it doesn't look like it's slow
      // ammoRB.velocity = parentRB.velocity;
      // ammoRB.angularVelocity = parentRB.angularVelocity;

      Vector3 inheritedVelocity = parentRB.GetPointVelocity(ammoObject.transform.position);
      ammoRB.velocity = inheritedVelocity + (ammoObject.transform.forward * ammo.speed * Time.deltaTime);

      // You may need to reverse the sign here, since bullet.transform.right
      // may be opposite of the rotation
      // Vector3 bulletTangentialVelocity = bulletAngularVelocityRad * bulletRotationRadius * ammoObject.transform.right;
      // ammoRB.velocity = ammoRB.velocity + ammoObject.transform.forward * ammo.speed + bulletTangentialVelocity;

      // ammo.InheritVelocity(parentRB.velocity);

      Debug.DrawRay(ammoObject.transform.position, ammoObject.transform.forward * 5f, Color.red, 5f);

      // forget me now, job's done
      ammoObject = null;
    }
  }
}
