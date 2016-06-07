using UnityEngine;
using System.Collections.Generic;
using PhotonPlayerExtension;

public class FFAGameNetworkManager : InGameNetworkManager {

    public float winningScore;

    private bool hasEnded;

    protected override void Update () {
        base.Update ();

        if (!photonView.isMine) {
            return;
        }
        if (hasEnded) { // Game has ended, don't update
            return;
        }

        foreach (KeyValuePair<int, TeamData> entry in teamData) {
            if (entry.Value.score >= winningScore) {
                hasEnded = true;
            }
        }

        if (hasEnded) {
            EndGame ();
        }
    }

    public override void AddKillData (int killingPlayerId) {
        base.AddKillData (killingPlayerId);
        PhotonPlayer killingPlayer = PhotonPlayer.Find (killingPlayerId);
        AddScore (killingPlayer.CurrentTeamId (), 1);
    }

}
