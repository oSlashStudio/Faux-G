using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class GameManagerController : NetworkBehaviour {

    public GameObject scoreboardPrefab;
    public GameObject chatboxPrefab;

    private List<int> playersConnectionId = new List<int> ();
    private List<string> playerNames = new List<string> ();

    public static GameManagerController Instance { get; private set; }

    public override void OnStartServer () {
        Instance = this;

        GameObject scoreboard = (GameObject) Instantiate (scoreboardPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn (scoreboard);
        GameObject chatbox = (GameObject) Instantiate (chatboxPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn (chatbox);
    }

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void AssignPlayer (int playerConnectionId) {
        if (!playersConnectionId.Contains (playerConnectionId)) { // If connection is not yet on the list
            playersConnectionId.Add (playerConnectionId);
            playerNames.Add ("Player " + playerConnectionId);

            ScoreboardController.Instance.AssignPlayer (playerConnectionId);
        }
    }

    public void RenamePlayer (int playerConnectionId, string newPlayerName) {
        if (playersConnectionId.Contains (playerConnectionId)) {
            int indexInList = playersConnectionId.IndexOf (playerConnectionId);
            playerNames[indexInList] = newPlayerName;

            ScoreboardController.Instance.RenamePlayer (playerConnectionId, newPlayerName);
        }
    }

    public void UnassignPlayer (int playerConnectionId) {
        if (playersConnectionId.Contains (playerConnectionId)) {
            int indexInList = playersConnectionId.IndexOf (playerConnectionId);
            playersConnectionId.RemoveAt (indexInList);

            ScoreboardController.Instance.UnassignPlayer (playerConnectionId);
        }
    }

    public string GetPlayerName (int playerConnectionId) {
        if (playersConnectionId.Contains (playerConnectionId)) {
            int indexInList = playersConnectionId.IndexOf (playerConnectionId);
            return playerNames[indexInList];
        }
        return null;
    }

}
