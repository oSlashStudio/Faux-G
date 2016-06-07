using UnityEngine;
using System.Collections;
using PhotonPlayerExtension;

public class DominationGameNetworkManager : InGameNetworkManager {

    public override void AddKillData (int killingPlayerId) {
        base.AddKillData (killingPlayerId);
        PhotonPlayer killingPlayer = PhotonPlayer.Find (killingPlayerId);
        photonView.RPC ("RpcAddScore", PhotonTargets.All, killingPlayer.CurrentTeamId (), 1);
    }

    [PunRPC]
    void RpcAddScore (int teamId, float scoreIncrease) {
        teamData[teamId].score += scoreIncrease;
    }

}
