using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class NetworkManagerController : NetworkManager {

    private Queue<int> playersToAssign = new Queue<int> ();
    private Queue<string> playerNamesToAssign = new Queue<string> ();
    private Queue<int> playersToUnassign = new Queue<int> ();

    public static NetworkLobbyManagerController Instance { get; private set; }
    
    public override void OnServerConnect (NetworkConnection conn) {
        base.OnServerConnect (conn);
    }

    public void AssignPlayer (int playerConnectionId, string playerName) {
        playersToAssign.Enqueue (playerConnectionId);
        playerNamesToAssign.Enqueue (playerName);
    }

    public override void OnServerDisconnect (NetworkConnection conn) {
        playersToUnassign.Enqueue (conn.connectionId);
        base.OnServerDisconnect (conn); 
    }

    void Update () {
        if (GameManagerController.Instance == null) {
            return;
        }
        
        if (playersToAssign.Count != 0) {
            GameManagerController.Instance.AssignPlayer (playersToAssign.Dequeue (), playerNamesToAssign.Dequeue ());
        }
        if (playersToUnassign.Count != 0) {
            GameManagerController.Instance.UnassignPlayer (playersToUnassign.Dequeue ());
        }
    }

}
