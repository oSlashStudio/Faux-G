using UnityEngine;
using System.Collections.Generic;
using PhotonPlayerExtension;

public class DominationGameNetworkManager : InGameNetworkManager {

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

}
