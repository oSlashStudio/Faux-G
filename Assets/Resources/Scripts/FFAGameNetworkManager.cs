using UnityEngine;
using System.Collections;
using PhotonPlayerExtension;

public class FFAGameNetworkManager : InGameNetworkManager {

    public override void AddKillData (int killingPlayerId) {
        base.AddKillData (killingPlayerId);
        PhotonPlayer killingPlayer = PhotonPlayer.Find (killingPlayerId);
        AddScore (killingPlayer.CurrentTeamId (), 1);
    }

}
