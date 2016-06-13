using UnityEngine;
using System.Collections;
using PhotonPlayerExtension;

public class GlobalHeal : Ability {

    public float healAmount;

    protected override void ActivateAbility () {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag ("Player")) {
            PhotonView playerPhotonView = player.GetComponent<PhotonView> ();

            if (playerPhotonView.owner.CurrentTeamId () == PhotonNetwork.player.CurrentTeamId ()) {
                photonView.RPC ("RpcHealTarget", playerPhotonView.owner, playerPhotonView.viewID, PhotonNetwork.player.ID);
            }
        }
    }

    [PunRPC]
    void RpcHealTarget (int targetViewId, int healingPlayerId) {
        PhotonView targetPhotonView = PhotonView.Find (targetViewId);
        targetPhotonView.GetComponent<HealthController> ().Heal (healAmount, targetPhotonView.transform.position, healingPlayerId);
    }

}
