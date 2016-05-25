using UnityEngine;
using Photon;
using System.Collections.Generic;

public class NetworkManager : Photon.PunBehaviour {

    public GameObject[] spawnLocations;

    // Current state related variables
    private bool isInLobby = false;
    private bool isInRoom = false;
    private string playerName = "";
    private string promptMessage = "";

    // Player respawn related variables
    public float defaultRespawnTimer = 10.0f;
    private bool isDead = false;
    private float respawnTimer = 0.0f;
    private int killerViewId;
    private string killerName;

    // Spectating related variables
    public GameObject spectateCamera;
    private bool isSpectating;
    private Camera spectateCameraComponent;
    private AudioListener spectateCameraAudioListener;

    public bool IsDead {
        get {
            return isDead;
        }
        set {
            isDead = true;
            respawnTimer = defaultRespawnTimer;

            IsSpectating = true;
        }
    }

    public int KillerViewId {
        get {
            return killerViewId;
        }
        set {
            killerViewId = value;
            PhotonView killerPhotonView = PhotonView.Find (killerViewId);
            if (killerPhotonView == null) {
                killerName = "Enemy";
            } else {
                PhotonPlayer killerPlayer = killerPhotonView.owner;
                killerName = killerPlayer.name;
            }
        }
    }

    public bool IsSpectating {
        get {
            return isSpectating;
        }
        set {
            isSpectating = value;
            if (isSpectating) {
                spectateCameraComponent.enabled = true;
                spectateCameraAudioListener.enabled = true;
            } else {
                spectateCameraComponent.enabled = false;
                spectateCameraAudioListener.enabled = false;
            }
        }
    }

	// Use this for initialization
	void Start () {
        spectateCameraComponent = spectateCamera.GetComponent<Camera> ();
        spectateCameraAudioListener = spectateCamera.GetComponent<AudioListener> ();

        PhotonNetwork.ConnectUsingSettings ("v0.1");
        PhotonNetwork.sendRate = 15;
        PhotonNetwork.sendRateOnSerialize = 15;
	}
	
	// Update is called once per frame
	void Update () {
        if (isDead) {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0.0f) {
                isDead = false;
                SpawnPlayer ();
            }
        }
    }

    void OnGUI () {
        GUILayout.Label (PhotonNetwork.connectionStateDetailed.ToString ());
        if (!isInLobby) { // Scenario 1: Player is not in lobby yet
            GUILayout.BeginArea (new Rect (
                Screen.width / 2.0f - 100.0f,
                Screen.height / 2.0f - 50.0f,
                200.0f,
                100.0f
                ));

            GUILayout.Label ("Connecting to Photon Cloud");
            GUILayout.Label ("Please wait...");

            GUILayout.EndArea ();
        } else if (!isInRoom) { // Scenario 2: Player is not in room yet
            GUILayout.BeginArea (new Rect (
                Screen.width / 2.0f - 100.0f,
                Screen.height / 2.0f - 100.0f,
                200.0f,
                200.0f
                ));

            GUILayout.BeginHorizontal ();
            GUILayout.Label ("Name:");
            playerName = GUILayout.TextField (playerName);
            GUILayout.EndHorizontal ();
            
            if (GUILayout.Button ("Create Room")) {
                if (playerName == "") { // Error: player name can't be empty
                    promptMessage = "Player name can't be empty";
                } else {
                    PhotonNetwork.player.name = playerName;
                    CreateRoom ();
                }
            }
            if (GUILayout.Button ("Join Random Room")) {
                if (playerName == "") { // Error: player name can't be empty
                    promptMessage = "Player name can't be empty";
                } else {
                    PhotonNetwork.player.name = playerName;
                    PhotonNetwork.JoinRandomRoom ();
                }
            }

            GUILayout.Label (promptMessage);

            GUILayout.EndArea ();
        } else if (isDead) { // Player is currently dead
            GUILayout.BeginArea (new Rect (
                Screen.width / 2.0f - 100.0f, 
                Screen.height / 2.0f - 50.0f, 
                200.0f, 
                100.0f
                ));

            GUIStyle respawnLabelStyle = GUI.skin.label;
            respawnLabelStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label ("Killed by " + killerName + ", respawning in " + respawnTimer.ToString ("0") + "...", respawnLabelStyle);
            GUILayout.Label ("In the meantime, you can spectate other players by moving this spectate camera around.", respawnLabelStyle);

            GUILayout.EndArea ();
        }
    }

    public override void OnJoinedLobby () {
        isInLobby = true;
    }

    public override void OnCreatedRoom () {
        PhotonNetwork.InstantiateSceneObject ("Boss Small", new Vector3 (1.0f, 1.0f, 0.0f), Quaternion.identity, 0, null);
    }

    public override void OnJoinedRoom () {
        isInRoom = true;

        SpawnPlayer ();
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
    }

    public void SpawnPlayer () {
        IsSpectating = false;

        int spawnLocationId = Random.Range (0, spawnLocations.Length);
        PhotonNetwork.Instantiate ("Sniper", spawnLocations[spawnLocationId].transform.position, Quaternion.identity, 0);
    }

}
