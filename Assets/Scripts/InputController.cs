using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class InputController : MonoBehaviour {
  public int playerId = 0; // The Rewired player id of this character
  private Player player; // The Rewired Player
  private float deadZone = 0.071f;

  void Start() {
    player = ReInput.players.GetPlayer(playerId);
  }

  public bool Accelerate() {
    return player.GetButton("Accelerate");
  }

  public bool AccelerateStart() {
    return player.GetButtonDown("Accelerate");
  }

  public bool Brake() {
    return player.GetButton("Brake");
  }

  public bool BoostStart() {
    return player.GetButtonDown("Boost");
  }

  public bool ReverseStart() {
    return player.GetButtonDown("Reverse");
  }

  public Vector3 PlayerMove() {
    Vector2 stickInput = new Vector2(player.GetAxis("Move_Horizontal"), player.GetAxis("Move_Vertical"));
    if (stickInput.sqrMagnitude < deadZone * deadZone) {
      stickInput = Vector2.zero;
    }

    return new Vector3(player.GetAxis("Move_Horizontal"), 0, player.GetAxis("Move_Vertical"));
  }

  public bool Shoot() {
    return player.GetButton("Shoot");
  }

  public Vector3 AimVector() {
    Vector2 stickInput = new Vector2(player.GetAxis("Aim_Horizontal"), player.GetAxis("Aim_Vertical"));
    if (stickInput.sqrMagnitude < deadZone * deadZone) {
      stickInput = Vector2.zero;
    }

    return new Vector3(stickInput.x, 0, stickInput.y);
  }

  public Vector3 AimDelta() {
    return new Vector3(player.GetAxisDelta("Aim_Horizontal"), 0, player.GetAxisDelta("Aim_Vertical"));
  }

  public bool ChangeCamera() {
    return player.GetButtonDown("Change Camera");
  }
}
