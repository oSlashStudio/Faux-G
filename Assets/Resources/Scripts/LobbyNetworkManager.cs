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
    private GUIStyle leftAlignedLabel;
    private GUIStyle topScrollView;

    // Use this for initialization
    void Start () {
        if (PhotonNetwork.connected || PhotonNetwork.connecting) {
            if (PhotonNetwork.player.name != null) { // If already has a name
                playerName = PhotonNetwork.player.name;
            }
        }
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
        if (leftAlignedLabel == null) {
            leftAlignedLabel = new GUIStyle (GUI.skin.label);
            leftAlignedLabel.alignment = TextAnchor.MiddleLeft;
        }
        if (topScrollView == null) {
            topScrollView = new GUIStyle (GUI.skin.scrollView);
            topScrollView.alignment = TextAnchor.UpperCenter;
        }

        if (!isInLobby) { // Scenario 1: Player is not in lobby yet
            ConnectingGUI ();
        } else if (isFindingRoom) { // Scenario 2: Player is finding room
            FindRoomGUI ();
        } else if (isCreatingRoom) { // Scenario 3: Player is creating room
            CreateRoomGUI ();
        } else { // Scenario 4: Player is still in lobby
            LobbyGUI ();
        }
    }

    void ConnectingGUI () {
        GUILayout.BeginArea (RelativeRect (0, 0, 1920, 1080));

        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace (); // Left padding

        GUILayout.BeginVertical ();
        GUILayout.FlexibleSpace (); // Top padding

        GUILayout.Label ("Connecting to server", centeredLabel);
        GUILayout.Label ("Please wait...", centeredLabel);

        GUILayout.FlexibleSpace (); // Bottom padding
        GUILayout.EndVertical ();

        GUILayout.FlexibleSpace (); // Right padding
        GUILayout.EndHorizontal ();

        GUILayout.EndArea ();
    }

    void FindRoomGUI () {
        GUILayout.BeginArea (RelativeRect (0, 0, 1920, 1080));

        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace (); // Left padding

        GUILayout.BeginVertical ();
        GUILayout.FlexibleSpace (); // Top padding

        scrollPos = GUILayout.BeginScrollView (scrollPos, topScrollView, GUILayout.Height (RelativeHeight (800)));
        RoomInfo[] rooms = PhotonNetwork.GetRoomList ();
        if (rooms.Length == 0) { // No rooms
            GUILayout.Label ("No rooms are available right now");
        }
        foreach (RoomInfo room in rooms) {
            GUILayout.BeginHorizontal ();
            GUILayout.Label (room.name + " (" + room.playerCount + " / " + room.maxPlayers + ")");
            // Room's currently selected map
            byte selectedMapId = (byte) room.customProperties["map"];
            switch (selectedMapId) {
                case 0:
                    GUILayout.Label ("Boss Small");
                    break;
                case 1:
                    GUILayout.Label ("FFA Small");
                    break;
                case 2:
                    GUILayout.Label ("Practice");
                    break;
                default:
                    GUILayout.Label ("Unknown");
                    break;
            }
            if (GUILayout.Button ("Join")) { // Join button
                JoinRoom (room.name);
            }
            GUILayout.EndHorizontal ();
        }
        GUILayout.EndScrollView ();

        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace (); // Left padding for back button
        if (GUILayout.Button ("Back")) { // Back button
            isFindingRoom = false;
        }
        GUILayout.EndHorizontal ();

        GUILayout.FlexibleSpace (); // Bottom padding
        GUILayout.EndVertical ();

        GUILayout.FlexibleSpace (); // Right padding
        GUILayout.EndHorizontal ();

        GUILayout.EndArea ();
    }

    void CreateRoomGUI () {
        GUILayout.BeginArea (RelativeRect (0, 0, 1920, 1080));

        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace (); // Left padding

        GUILayout.BeginVertical ();
        GUILayout.FlexibleSpace (); // Top padding

        GUILayout.BeginHorizontal ();

        GUILayout.Label ("Room Name:", leftAlignedLabel);

        GUI.SetNextControlName ("Room Name");
        roomName = GUILayout.TextField (roomName, 32, GUILayout.Width (RelativeWidth (400)));
        if (roomName == "") {
            GUI.FocusControl ("Room Name");
        }

        GUILayout.EndHorizontal ();

        selectedMapId = GUILayout.Toolbar (selectedMapId, mapNames);

        if (GUILayout.Button ("Create")) {
            CreateRoom ();
        }

        if (GUILayout.Button ("Back")) {
            isCreatingRoom = false;
        }

        GUILayout.FlexibleSpace (); // Bottom padding
        GUILayout.EndVertical ();

        GUILayout.FlexibleSpace (); // Right padding
        GUILayout.EndHorizontal ();

        GUILayout.EndArea ();
    }

    void LobbyGUI () {
        GUILayout.BeginArea (RelativeRect (0, 0, 1920, 1080));

        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace (); // Left padding

        GUILayout.BeginVertical ();
        GUILayout.FlexibleSpace (); // Top padding

        GUILayout.BeginHorizontal ();
        GUILayout.Label ("Player Name:");
        GUI.SetNextControlName ("Player Name");
        playerName = GUILayout.TextField (playerName, 32, GUILayout.Width (RelativeWidth (400)));
        if (playerName == "") {
            GUI.FocusControl ("Player Name"); // Focus while empty
        }
        GUILayout.EndHorizontal ();

        if (GUILayout.Button ("Create Room")) {
            if (playerName == "") {
                promptMessage = "Player name can't be empty";
            } else {
                PhotonNetwork.player.name = playerName;
                isCreatingRoom = true;
            }
        }
        if (GUILayout.Button ("Join Random Room")) {
            if (playerName == "") {
                promptMessage = "Player name can't be empty";
            } else {
                PhotonNetwork.player.name = playerName;
                JoinRandomRoom ();
            }
        }
        if (GUILayout.Button ("Find Room")) {
            if (playerName == "") {
                promptMessage = "Player name can't be empty";
            } else {
                PhotonNetwork.player.name = playerName;
                isFindingRoom = true;
            }
        }
        GUILayout.Label (promptMessage, centeredLabel);

        GUILayout.FlexibleSpace (); // Bottom padding
        GUILayout.EndVertical ();

        GUILayout.FlexibleSpace (); // Right padding
        GUILayout.EndHorizontal ();

        GUILayout.EndArea ();
    }

    public override void OnJoinedLobby () {
        isInLobby = true;
    }

    void CreateRoom () {
        RoomOptions roomOptions = new RoomOptions ();
        roomOptions.isOpen = true;
        roomOptions.isVisible = true;
        roomOptions.maxPlayers = 4;
        // Setup custom room properties (map, etc.)
        roomOptions.customRoomProperties = new ExitGames.Client.Photon.Hashtable ();
        roomOptions.customRoomProperties.Add ("map", (byte) selectedMapId);
        roomOptions.customRoomPropertiesForLobby = new string[] { "map" };

        PhotonNetwork.CreateRoom (roomName, roomOptions, TypedLobby.Default);
    }

    void JoinRandomRoom () {
        PhotonNetwork.JoinRandomRoom ();
    }

    public override void OnPhotonRandomJoinFailed (object[] codeAndMsg) {
        promptMessage = "No rooms can be joined right now";
    }

    void JoinRoom (string roomName) {
        PhotonNetwork.JoinRoom (roomName);
    }

    public override void OnJoinedRoom () {
        PhotonNetwork.LoadLevel (2); // Load room scene
    }

}
