using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class NetworkManagerController : NetworkManager {

    private Queue<int> playersToAssign = new Queue<int> ();
    private Queue<int> playersToUnassign = new Queue<int> ();

    public override void OnServerConnect (NetworkConnection conn) {
        playersToAssign.Enqueue (conn.connectionId);
        base.OnServerConnect (conn);
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
            GameManagerController.Instance.AssignPlayer (playersToAssign.Dequeue ());
        }
        if (playersToUnassign.Count != 0) {
            GameManagerController.Instance.UnassignPlayer (playersToUnassign.Dequeue ());
        }
    }

}
