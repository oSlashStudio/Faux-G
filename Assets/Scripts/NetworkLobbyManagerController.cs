using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

using Prototype.NetworkLobby;

public class NetworkLobbyManagerController : LobbyManager {

    private Queue<int> playersToAssign = new Queue<int> ();
    private Queue<string> playerNamesToAssign = new Queue<string> ();
    private Queue<int> playersToUnassign = new Queue<int> ();

    public static NetworkLobbyManagerController instance;

    public static NetworkLobbyManagerController Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<NetworkLobbyManagerController> ();
            }
            return instance;
        }
    }
    
    public override void OnLobbyServerConnect (NetworkConnection conn) {
        base.OnLobbyServerConnect (conn);
    }

    public void AssignPlayer (int playerConnectionId, string playerName) {
        playersToAssign.Enqueue (playerConnectionId);
        playerNamesToAssign.Enqueue (playerName);
    }

    public override void OnLobbyServerDisconnect (NetworkConnection conn) {
        playersToUnassign.Enqueue (conn.connectionId);
        base.OnLobbyServerDisconnect (conn);
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
