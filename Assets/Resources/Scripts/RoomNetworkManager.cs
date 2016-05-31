using UnityEngine;
using System.Collections;

public class RoomNetworkManager : Photon.PunBehaviour {

    private int selectedClassId = 0;
    private string[] classNames = new string[] {
        "Assaulter",
        "Recon", 
        "Light", 
        "Demolitionist", 
        "Heatseeker", 
        "Sieger", 
        "Heavy", 
        "Healer"
    };
    private ExitGames.Client.Photon.Hashtable classHashtable = new ExitGames.Client.Photon.Hashtable ();

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI () {
        GUIStyle centeredLabel = GUI.skin.label;
        centeredLabel.alignment = TextAnchor.MiddleCenter;

        if (!PhotonNetwork.inRoom) { // Not in room yet
            GUILayout.BeginArea (new Rect (
                Screen.width / 2.0f - 100.0f,
                Screen.height / 2.0f - 50.0f,
                200.0f,
                100.0f
                ));


            GUILayout.Label ("Trying to enter room", centeredLabel);
            GUILayout.Label ("Please wait...", centeredLabel);

            GUILayout.EndArea ();
        } else {
            GUILayout.BeginArea (new Rect (
                    Screen.width / 2.0f - 200.0f,
                    Screen.height / 2.0f - 200.0f,
                    400.0f,
                    400.0f
                    ), GUI.skin.box);

            GUILayout.Label (PhotonNetwork.room.name + " (" + PhotonNetwork.room.playerCount + "/" + PhotonNetwork.room.maxPlayers + ")");
            PhotonPlayer[] playersInRoom = PhotonNetwork.playerList;
            foreach (PhotonPlayer player in playersInRoom) {
                GUILayout.BeginHorizontal ();

                GUILayout.Label (player.name, GUILayout.Width (200.0f)); // Player name label

                if (player.isLocal) { // Local player
                    if (GUILayout.Button ("Leave")) { // Leave button
                        PhotonNetwork.LeaveRoom ();
                    }
                } else if (PhotonNetwork.isMasterClient) { // Remote (other) player
                    if (GUILayout.Button ("Kick")) { // Kick button
                        PhotonNetwork.CloseConnection (player);
                    }
                }

                GUILayout.EndHorizontal ();
            }

            GUILayout.Label ("Select Class:");
            selectedClassId = GUILayout.SelectionGrid (selectedClassId, classNames, 4);

            if (PhotonNetwork.isMasterClient) {
                if (GUILayout.Button ("Start Game")) {
                    StartGame ();                    
                }
            }

            GUILayout.EndArea ();
        }
    }

    public override void OnLeftRoom () {
        PhotonNetwork.LoadLevel (0);
    }

    void StartGame () {
        // Close access to the room when starting, this includes reconnection
        PhotonNetwork.room.open = false;
        PhotonNetwork.room.visible = false;
        // Prompts all clients to load in-game scene
        photonView.RPC ("RpcLoadLevel", PhotonTargets.All);
    }

    [PunRPC]
    void RpcLoadLevel () {
        // Set class
        classHashtable["class"] = (byte) selectedClassId;
        PhotonNetwork.player.SetCustomProperties (classHashtable);
        // Temporarily pause message queue, to be resumed when the in-game scene loads
        PhotonNetwork.isMessageQueueRunning = false;
        // 2 is the offset taking lobby and room scenes into account
        PhotonNetwork.LoadLevel ((byte) PhotonNetwork.room.customProperties ["map"] + 2);
    }

}
