using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class ScoreboardController : NetworkBehaviour {

    public SyncListInt playersConnectionId = new SyncListInt ();
    public SyncListString playerNames = new SyncListString ();
    public SyncListInt playerScores = new SyncListInt ();

    public static ScoreboardController Instance { get; private set; }

    public override void OnStartServer () {
        Instance = this;
    }

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update () {

    }

    void OnGUI () {
        GUIStyle style = GUI.skin.GetStyle ("Label");
        style.alignment = TextAnchor.UpperCenter;
        string scoreText = "";
        for (int i = 0; i < playersConnectionId.Count; i++) {
            if (i != 0) {
                scoreText += " | ";
            }
            scoreText += playerNames[i] + ": " + playerScores[i];
        }
        GUI.Label (new Rect (0, 0, Screen.width, Screen.height), scoreText, style);
    }

    public void AssignPlayer (int playerConnectionId, string playerName) {
        if (!playersConnectionId.Contains (playerConnectionId)) { // If connection is not yet on the list
            playersConnectionId.Add (playerConnectionId);
            playerNames.Add (playerName);
            playerScores.Add (0);
        }
    }

    public void RenamePlayer (int playerConnectionId, string newPlayerName) {
        if (playersConnectionId.Contains (playerConnectionId)) {
            int indexInList = playersConnectionId.IndexOf (playerConnectionId);
            playerNames[indexInList] = newPlayerName;
        }
    }

    public void IncreaseScore (int playerConnectionId, int scoreIncrease) {
        if (playersConnectionId.Contains (playerConnectionId)) {
            int indexInList = playersConnectionId.IndexOf (playerConnectionId);
            playerScores[indexInList] += scoreIncrease;
        }
    }

    public void ReduceScore (int playerConnectionId, int scoreReduction) {
        if (playersConnectionId.Contains (playerConnectionId)) {
            int indexInList = playersConnectionId.IndexOf (playerConnectionId);
            playerScores[indexInList] -= scoreReduction;
        }
    }

    public void UnassignPlayer (int playerConnectionId) {
        if (playersConnectionId.Contains (playerConnectionId)) {
            int indexInList = playersConnectionId.IndexOf (playerConnectionId);
            playersConnectionId.RemoveAt (indexInList);
            playerScores.RemoveAt (indexInList);
        }
    }

}
