using UnityEngine;
using System.Collections;

public class LobbyNetworkManager : Photon.PunBehaviour {

    private bool isInLobby = false;
    private bool isFindingRoom = false;

    private string playerName = "";
    private string promptMessage = "";

    private Vector2 scrollPos = Vector2.zero;

    // Use this for initialization
    void Start () {
        PhotonNetwork.ConnectUsingSettings ("v0.1");
        PhotonNetwork.sendRate = 15;
        PhotonNetwork.sendRateOnSerialize = 15;
    }

    // Update is called once per frame
    void Update () {

    }

    void OnGUI () {
        GUIStyle centeredLabel = GUI.skin.label;
        centeredLabel.alignment = TextAnchor.MiddleCenter;

        GUIStyle topScrollView = GUI.skin.scrollView;
        topScrollView.alignment = TextAnchor.UpperCenter;

        if (!isInLobby) { // Scenario 1: Player is not in lobby yet
            GUILayout.BeginArea (new Rect (
                Screen.width / 2.0f - 100.0f,
                Screen.height / 2.0f - 50.0f,
                200.0f,
                100.0f
                ));


            GUILayout.Label ("Connecting to Photon Cloud", centeredLabel);
            GUILayout.Label ("Please wait...", centeredLabel);

            GUILayout.EndArea ();
        } else if (isFindingRoom) {
            if (GUILayout.Button ("Back")) {
                isFindingRoom = false;
            }
            GUILayout.BeginArea (new Rect (
                Screen.width / 2.0f - 200.0f,
                Screen.height / 2.0f - 200.0f,
                400.0f,
                400.0f
                ));

            scrollPos = GUILayout.BeginScrollView (scrollPos, topScrollView, GUILayout.Width (400), GUILayout.Height (400));
            
            RoomInfo[] rooms = PhotonNetwork.GetRoomList ();
            foreach (RoomInfo room in rooms) {
                if (GUILayout.Button (room.name + " (" + room.playerCount + "/" + room.maxPlayers + " players)")) {
                    JoinRoom (room.name);
                }
            }

            GUILayout.EndScrollView ();

            GUILayout.EndArea ();
        } else {
            GUILayout.BeginArea (new Rect (
                Screen.width / 2.0f - 100.0f,
                Screen.height / 2.0f - 100.0f,
                200.0f,
                200.0f
                ));

            GUILayout.Label ("Player Name:");
            playerName = GUILayout.TextField (playerName);

            if (playerName == "") {
                promptMessage = "Player name can't be empty";
            } else {
                promptMessage = "";

                if (GUILayout.Button ("Create Room")) {
                    PhotonNetwork.player.name = playerName;
                    CreateRoom ();
                }
                if (GUILayout.Button ("Join Random Room")) {
                    PhotonNetwork.player.name = playerName;
                    PhotonNetwork.JoinRandomRoom ();
                }
                if (GUILayout.Button ("Find Room")) {
                    isFindingRoom = true;
                }
            }

            GUILayout.Label (promptMessage, centeredLabel);

            GUILayout.EndArea ();
        }
    }

    public override void OnJoinedLobby () {
        isInLobby = true;
    }

    public override void OnPhotonRandomJoinFailed (object[] codeAndMsg) {
        promptMessage = "No rooms can be joined right now";
    }

    void CreateRoom () {
        RoomOptions roomOptions = new RoomOptions ();
        roomOptions.isOpen = true;
        roomOptions.isVisible = true;
        roomOptions.maxPlayers = 4;

        PhotonNetwork.CreateRoom (null, roomOptions, TypedLobby.Default);

        PhotonNetwork.LoadLevel (1);
    }

    void JoinRoom (string roomName) {
        PhotonNetwork.JoinRoom (roomName);

        PhotonNetwork.LoadLevel (1);
    }

}
