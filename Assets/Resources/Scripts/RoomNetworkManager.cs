using UnityEngine;
using System.Collections;

public class RoomNetworkManager : Photon.PunBehaviour {

    private int selectedClassId = 0;
    private string[] classNames = new string[] {
        "Mercenary",
        "Virtuoso",
        "The Maniac",
        "NASA Fanboy",
        "Spinal Holler",
        "R.O.B.O.T",
        "Gargantuan",
        "Crazy Shaman",
        "Heavy Bomber"
    };
    private ExitGames.Client.Photon.Hashtable classHashtable = new ExitGames.Client.Photon.Hashtable ();

    // Cached component
    private GUIStyle centeredLabel;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    /*
     * Get a rectangle relative to full HD 1920:1080 screen
     */
    Rect RelativeRect (float x, float y, float w, float h) {
        float relativeX = Screen.width * x / 1920;
        float relativeY = Screen.height * y / 1080;
        float relativeW = Screen.width * w / 1920;
        float relativeH = Screen.height * h / 1080;

        return new Rect (relativeX, relativeY, relativeW, relativeH);
    }

    float RelativeWidth (float w) {
        float relativeW = Screen.width * w / 1920;

        return relativeW;
    }

    float RelativeHeight (float h) {
        float relativeH = Screen.height * h / 1080;

        return relativeH;
    }

    void OnGUI () {
        // Initialize GUI Styles
        if (centeredLabel == null) {
            centeredLabel = new GUIStyle (GUI.skin.label);
            centeredLabel.alignment = TextAnchor.MiddleCenter;
        }

        if (!PhotonNetwork.inRoom) { // Trying to enter room
            EnteringRoomGUI ();
        } else {
            RoomGUI ();
        }
    }

    void EnteringRoomGUI () {
        GUILayout.BeginArea (RelativeRect (0, 0, 1920, 1080));

        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace (); // Left padding

        GUILayout.BeginVertical ();
        GUILayout.FlexibleSpace (); // Top padding

        GUILayout.Label ("Trying to enter room", centeredLabel);
        GUILayout.Label ("Please wait...", centeredLabel);

        GUILayout.FlexibleSpace (); // Bottom padding
        GUILayout.EndVertical ();

        GUILayout.FlexibleSpace (); // Right padding
        GUILayout.EndHorizontal ();

        GUILayout.EndArea ();
    }

    void RoomGUI () {
        GUILayout.BeginArea (RelativeRect (0, 0, 1920, 1080));

        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace (); // Left padding

        GUILayout.BeginVertical ();
        GUILayout.FlexibleSpace (); // Top padding

        GUILayout.Label (PhotonNetwork.room.name + " (" + PhotonNetwork.room.playerCount + " / " + PhotonNetwork.room.maxPlayers + ")", centeredLabel);
        PhotonPlayer[] playersInRoom = PhotonNetwork.playerList;
        foreach (PhotonPlayer player in playersInRoom) {
            GUILayout.BeginHorizontal ();

            GUILayout.Label (player.name); // Player name label

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
        selectedClassId = GUILayout.SelectionGrid (selectedClassId, classNames, 3);

        if (PhotonNetwork.isMasterClient) {
            if (GUILayout.Button ("Start Game")) {
                StartGame ();
            }
        }

        GUILayout.FlexibleSpace (); // Bottom padding
        GUILayout.EndVertical ();

        GUILayout.FlexibleSpace (); // Right padding
        GUILayout.EndHorizontal ();

        GUILayout.EndArea ();
    }

    public override void OnLeftRoom () {
        PhotonNetwork.LoadLevel (1);
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
        PhotonNetwork.LoadLevel ((byte) PhotonNetwork.room.customProperties ["map"] + 3);
    }

}
