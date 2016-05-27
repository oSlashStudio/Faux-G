using UnityEngine;
using System.Collections;

public class LobbyNetworkManager : Photon.PunBehaviour {

    private bool isInLobby = false;
    private bool isFindingRoom = false;
    private bool isCreatingRoom = false;

    private string playerName = "";
    private string promptMessage = "";
    private string roomName = "";

    private int selectedMapId = 0;
    private string[] mapNames = new string[] {
        "Boss Small", 
        "FFA Small", 
        "Practice"
    };

    private Vector2 scrollPos = Vector2.zero;

    private GUIStyle centeredLabel;
    private GUIStyle topScrollView;

    // Use this for initialization
    void Start () {
        if (PhotonNetwork.connected || PhotonNetwork.connecting) {
            if (PhotonNetwork.player.name != null) { // If already has a name
                playerName = PhotonNetwork.player.name;
            }
            return; // Already connected, skip return;
        }

        PhotonNetwork.ConnectUsingSettings ("v0.1");
        PhotonNetwork.sendRate = 15;
        PhotonNetwork.sendRateOnSerialize = 15;
    }

    // Update is called once per frame
    void Update () {

    }

    void OnGUI () {
        if (centeredLabel == null) {
            centeredLabel = GUI.skin.label;
            centeredLabel.alignment = TextAnchor.MiddleCenter;
        }
        if (topScrollView == null) {
            topScrollView = GUI.skin.scrollView;
            topScrollView.alignment = TextAnchor.UpperCenter;
        }

        if (!isInLobby) { // Scenario 1: Player is not in lobby yet
            ConnectingGUI ();
        } else if (isFindingRoom) {
            FindRoomGUI ();
        } else if (isCreatingRoom) {
            CreateRoomGUI ();
        } else {
            LobbyGUI ();
        }
    }

    void ConnectingGUI () {
        GUILayout.BeginArea (new Rect (
                Screen.width / 2.0f - 100.0f,
                Screen.height / 2.0f - 50.0f,
                200.0f,
                100.0f
                ));


        GUILayout.Label ("Connecting to Photon Cloud", centeredLabel);
        GUILayout.Label ("Please wait...", centeredLabel);

        GUILayout.EndArea ();
    }

    void FindRoomGUI () {
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
            GUILayout.BeginHorizontal ();
            GUILayout.Label (room.name + " (" + room.playerCount + "/" + room.maxPlayers + ")", GUILayout.Width (150));

            byte selectedMapId = (byte) room.customProperties["map"];
            switch (selectedMapId) {
                case 0:
                    GUILayout.Label ("Boss Small", GUILayout.Width (150.0f));
                    break;
                case 1:
                    GUILayout.Label ("FFA Small", GUILayout.Width (150.0f));
                    break;
                case 2:
                    GUILayout.Label ("Practice", GUILayout.Width (150.0f));
                    break;
                default:
                    GUILayout.Label ("Unknown", GUILayout.Width (150.0f));
                    break;
            }
            if (GUILayout.Button ("Join")) {
                JoinRoom (room.name);
            }
            GUILayout.EndHorizontal ();
        }

        GUILayout.EndScrollView ();

        GUILayout.EndArea ();
    }

    void CreateRoomGUI () {
        GUILayout.BeginArea (new Rect (
                Screen.width / 2.0f - 200.0f,
                Screen.height / 2.0f - 100.0f,
                400.0f,
                200.0f
                ));

        GUILayout.BeginHorizontal ();
        GUILayout.Label ("Room Name:", GUILayout.Width (150.0f));
        roomName = GUILayout.TextField (roomName);
        GUILayout.EndHorizontal ();

        selectedMapId = GUILayout.Toolbar (selectedMapId, mapNames);

        if (GUILayout.Button ("Create")) {
            CreateRoom ();
        }

        if (GUILayout.Button ("Back")) {
            isCreatingRoom = false;
        }

        GUILayout.EndArea ();
    }

    void LobbyGUI () {
        GUILayout.BeginArea (new Rect (
                Screen.width / 2.0f - 100.0f,
                Screen.height / 2.0f - 100.0f,
                200.0f,
                200.0f
                ));

        GUILayout.BeginHorizontal ();
        GUILayout.Label ("Player Name:", GUILayout.Width (100.0f));
        playerName = GUILayout.TextField (playerName);
        GUILayout.EndHorizontal ();

        if (playerName == "") {
            promptMessage = "Player name can't be empty";
        } else {
            if (GUILayout.Button ("Create Room")) {
                PhotonNetwork.player.name = playerName;
                isCreatingRoom = true;
            }
            if (GUILayout.Button ("Join Random Room")) {
                PhotonNetwork.player.name = playerName;
                PhotonNetwork.JoinRandomRoom ();
            }
            if (GUILayout.Button ("Find Room")) {
                PhotonNetwork.player.name = playerName;
                isFindingRoom = true;
            }
        }

        GUILayout.Label (promptMessage, centeredLabel);

        GUILayout.EndArea ();
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

        roomOptions.customRoomProperties = new ExitGames.Client.Photon.Hashtable ();
        roomOptions.customRoomProperties.Add ("map", (byte) selectedMapId);
        roomOptions.customRoomPropertiesForLobby = new string[] { "map" };

        PhotonNetwork.CreateRoom (roomName, roomOptions, TypedLobby.Default);

        PhotonNetwork.LoadLevel (1);
    }

    void JoinRoom (string roomName) {
        PhotonNetwork.JoinRoom (roomName);

        PhotonNetwork.LoadLevel (1);
    }

}
