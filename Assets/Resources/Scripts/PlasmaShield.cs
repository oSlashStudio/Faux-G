using UnityEngine;
using System.Collections;

public class PlasmaShield : Ability {

    public GameObject plasmaShieldPrefab;
    public float duration;

    protected override void ActivateAbility () {
        photonView.RPC ("RpcPlasmaShield", PhotonTargets.AllViaServer);
    }

    [PunRPC]
    void RpcPlasmaShield () {
        GameObject plasmaShield = (GameObject) Instantiate (plasmaShieldPrefab, transform.position, transform.rotation);
        Destroy (plasmaShield, duration);
    }

}
