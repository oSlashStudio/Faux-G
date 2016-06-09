using UnityEngine;
using System.Collections;

public class Ability : Photon.MonoBehaviour {

    public float defaultCooldown;
    private float cooldown;

    protected virtual void Start () {
        cooldown = 0.0f;
    }
    
    protected virtual void Update () {
        if (!photonView.isMine) {
            return;
        }

        UpdateCooldown ();
        InputActivateAbility ();
    }

    protected virtual void UpdateCooldown () {
        cooldown -= Time.deltaTime;
    }

    protected virtual void InputActivateAbility () {
        if (Input.GetKeyDown (KeyCode.T)) {
            if (cooldown <= 0.0f) {
                ActivateAbility ();
                cooldown = defaultCooldown;
            }
        }
    }

    protected virtual void ActivateAbility () {

    }

}
