using UnityEngine;
using Photon;
using System.Collections.Generic;

public class InGameNetworkManager : Photon.PunBehaviour {

    public GameObject[] spawnLocations;

    private ExitGames.Client.Photon.Hashtable classHashtable;
    private int selectedClassId;
    private string[] classNames = new string[] {
        "Assaulter",
        "Recon",
        "Light",
        "Demolitionist",
        "Heatseeker",
        "Sieger", 
        "Heavy"
    };

    // Current state related variables
    private bool isInGame = false;

    // Player respawn related variables
    public float defaultRespawnTimer = 10.0f;
    private bool isDead = false;
    private float respawnTimer = 0.0f;
    private int killerId;
    private string killerName;

    // Spectating related variables
    public GameObject spectateCamera;
    private bool isSpectating;

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

    public int KillerId {
        get {
            return killerId;
        }
        set {
            killerId = value;
            PhotonPlayer killerPlayer = PhotonPlayer.Find (killerId);
            if (killerPlayer == null) {
                killerName = "Enemy";
            } else {
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
                spectateCamera.GetComponent<Camera> ().enabled = true;
                spectateCamera.GetComponent<AudioListener> ().enabled = true;
            } else {
                spectateCamera.GetComponent<Camera> ().enabled = false;
                spectateCamera.GetComponent<AudioListener> ().enabled = false;
            }
        }
    }

	// Use this for initialization
	void Start () {
        
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
        GUILayout.Label (PhotonNetwork.GetPing ().ToString () + " ms");
        if (!isInGame) { // Not in game yet
            return;
        }

        if (isDead) { // Player is currently dead
            GUILayout.BeginArea (new Rect (
                Screen.width / 2.0f - 200.0f, 
                Screen.height / 2.0f - 100.0f, 
                400.0f, 
                200.0f
                ));

            GUIStyle respawnLabelStyle = GUI.skin.label;
            respawnLabelStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label ("Killed by " + killerName + ", respawning in " + respawnTimer.ToString ("0") + "...", respawnLabelStyle);
            GUILayout.Label ("Spectate other players by moving this spectate camera around.", respawnLabelStyle);

            // Class selection window
            GUILayout.Label ("Select Class:");
            selectedClassId = GUILayout.SelectionGrid (selectedClassId, classNames, 4);
            classHashtable["class"] = (byte) selectedClassId;
            PhotonNetwork.player.SetCustomProperties (classHashtable);

            GUILayout.EndArea ();
        }
    }

    void OnLevelWasLoaded () {
        isInGame = true;
        // Scene has been loaded, resume in-game instantiation
        PhotonNetwork.isMessageQueueRunning = true;

        classHashtable = PhotonNetwork.player.customProperties;
        selectedClassId = (byte) classHashtable["class"];
        SpawnPlayer ();
    }

    void SpawnPlayer () {
        IsSpectating = false;

        int spawnLocationId = Random.Range (0, spawnLocations.Length);

        string className = GetClassName ((byte) PhotonNetwork.player.customProperties["class"]);
        PhotonNetwork.Instantiate ("Classes/" + className, spawnLocations[spawnLocationId].transform.position, Quaternion.identity, 0);
    }

    string GetClassName (byte classId) {
        return classNames[classId];
    }

}
