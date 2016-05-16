using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkManagerController : NetworkManager {

    public override void OnServerDisconnect (NetworkConnection conn) {
        ScoreboardController.Instance.UnassignPlayer (conn.connectionId);
        base.OnServerDisconnect (conn); 
    }

}
