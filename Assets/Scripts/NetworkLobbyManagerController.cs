using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

using Prototype.NetworkLobby;

public class NetworkLobbyManagerController : LobbyManager {

    private Queue<int> playersToAssign = new Queue<int> ();
    private Queue<int> playersToUnassign = new Queue<int> ();

    public override void OnLobbyServerConnect (NetworkConnection conn) {
        playersToAssign.Enqueue (conn.connectionId);
        base.OnLobbyServerConnect (conn);
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
            GameManagerController.Instance.AssignPlayer (playersToAssign.Dequeue ());
        }
        if (playersToUnassign.Count != 0) {
            GameManagerController.Instance.UnassignPlayer (playersToUnassign.Dequeue ());
        }
    }

}
