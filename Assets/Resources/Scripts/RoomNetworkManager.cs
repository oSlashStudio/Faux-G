using UnityEngine;
using System.Collections;

public class RoomNetworkManager : Photon.PunBehaviour {

    private bool isInRoom = false;

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

            if (PhotonNetwork.isMasterClient) {
                if (GUILayout.Button ("Start")) { // Start button
                    PhotonNetwork.LoadLevel (2);
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

}
