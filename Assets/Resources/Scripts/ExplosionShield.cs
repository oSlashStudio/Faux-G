using UnityEngine;
using System.Collections;

public class ExplosionShield : Ability {

    public GameObject explosionShieldPrefab;
    public float defaultDuration;
    private float duration;
    private bool isActive;

    private LayerMask previousLayerMask;

    protected override void Update () {
        UpdateDuration ();
        base.Update ();
    }

    void UpdateDuration () {
        if (!isActive) {
            return;
        }

        duration -= Time.deltaTime;
        if (duration <= 0.0f) {
            DeactivateAbility ();
        }
    }

    protected override void ActivateAbility () {
        photonView.RPC ("RpcActivateAbility", PhotonTargets.AllViaServer);
    }

    [PunRPC]
    void RpcActivateAbility () {
        duration = defaultDuration;
        isActive = true;

        previousLayerMask = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer ("Ignore Raycast");

        GameObject explosionShield = (GameObject) Instantiate (explosionShieldPrefab, transform.position, transform.rotation);
        explosionShield.transform.parent = transform;
        Destroy (explosionShield, duration);
    }

    void DeactivateAbility () {
        isActive = false;

        gameObject.layer = previousLayerMask;
    }

}
