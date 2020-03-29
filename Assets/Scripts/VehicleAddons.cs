using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleAddons : MonoBehaviour {

  public InputController playerInput;
  public GameObject headlights;

  void Start() {
    if (playerInput == null) {
      playerInput = transform.root.GetComponent<InputController>();
    }

    if (headlights == null) {
      headlights = transform.Find("Headlights").gameObject;
    }
  }

  void Update() {
    if (playerInput.ToggleHeadlights() && headlights != null) {
      headlights.SetActive(!headlights.activeInHierarchy);
    }
  }
}
