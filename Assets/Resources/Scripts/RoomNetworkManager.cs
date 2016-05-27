using UnityEngine;
using System.Collections;

public class RoomNetworkManager : Photon.PunBehaviour {

    private bool isInRoom = false;

    private int selectedClassId = 0;
    private string[] classNames = new string[] {
        "Assaulter",
        "Recon", 
        "Light", 
        "Demolitionist", 
        "Heatseeker"
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

        if (!isInRoom) { // Not in room yet
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
                    Screen.height / 2.0f - 100.0f,
                    400.0f,
                    200.0f
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
            classHashtable["class"] = (byte) selectedClassId;
            PhotonNetwork.player.SetCustomProperties (classHashtable);

            if (PhotonNetwork.isMasterClient) {
                if (GUILayout.Button ("Start Game")) {
                    photonView.RPC ("RpcLoadLevel", PhotonTargets.AllBufferedViaServer);
                }
            }

            GUILayout.EndArea ();
        }
    }

    public override void OnJoinedRoom () {
        isInRoom = true;
    }

    public override void OnLeftRoom () {
        isInRoom = false;
        PhotonNetwork.LoadLevel (0);
    }

    [PunRPC]
    void RpcLoadLevel () {
        // Temporarily pause message queue, to be resumed when the in-game scene loads
        PhotonNetwork.isMessageQueueRunning = false;
        // 2 is the offset taking lobby and room scenes into account
        PhotonNetwork.LoadLevel ((byte) PhotonNetwork.room.customProperties ["map"] + 2);
    }

}
