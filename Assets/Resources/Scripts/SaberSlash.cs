using UnityEngine;
using System.Collections;

public class SaberSlash : Ability {

    public GameObject saberPrefab;

    protected override void ActivateAbility () {
        photonView.RPC ("RpcSaberSlash", PhotonTargets.AllViaServer);
    }

    [PunRPC]
    void RpcSaberSlash () {
        GameObject saber = (GameObject) Instantiate (saberPrefab, transform.position + transform.up * saberPrefab.transform.lossyScale.y / 2, transform.rotation);
        saber.transform.parent = transform;

        saber.GetComponent<TrailRenderer> ().material.SetColor ("_TintColor", GetComponent<Renderer> ().material.color);

        saber.GetComponent<SaberController> ().instantiatorId = photonView.owner.ID;
        Physics2D.IgnoreCollision (saber.GetComponent<Collider2D> (), GetComponent<Collider2D> ());
    }

}
