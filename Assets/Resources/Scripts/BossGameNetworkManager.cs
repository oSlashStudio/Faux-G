using UnityEngine;
using System.Collections;
using PhotonPlayerExtension;

public class BossGameNetworkManager : InGameNetworkManager {
    
    public override void AddDamageData (int damagingPlayerId, float damage) {
        base.AddDamageData (damagingPlayerId, damage);
        PhotonPlayer damagingPlayer = PhotonPlayer.Find (damagingPlayerId);
        AddScore (damagingPlayer.CurrentTeamId (), damage);
    }

}
