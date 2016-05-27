using UnityEngine;
using Photon;
using System.Collections.Generic;

public class InGameNetworkManager : Photon.PunBehaviour {

    public GameObject[] spawnLocations;

    // Current state related variables
    private bool isInGame = false;

    // Player respawn related variables
    public float defaultRespawnTimer = 10.0f;
    private bool isDead = false;
    private float respawnTimer = 0.0f;
    private int killerViewId;
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
        GUILayout.Label (PhotonNetwork.connectionStateDetailed.ToString ());
        if (!isInGame) { // Not in game yet
            return;
        }

        if (isDead) { // Player is currently dead
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

    void OnLevelWasLoaded () {
        isInGame = true;

        SpawnPlayer ();
    }

    void SpawnPlayer () {
        IsSpectating = false;

        int spawnLocationId = Random.Range (0, spawnLocations.Length);

        string className = GetClassName ((byte) PhotonNetwork.player.customProperties["class"]);
        PhotonNetwork.Instantiate (className, spawnLocations[spawnLocationId].transform.position, Quaternion.identity, 0);
    }

    string GetClassName (byte classId) {
        switch (classId) {
            case 0:
                return "Assaulter";
            case 1:
                return "Recon";
            case 2:
                return "Light";
            case 3:
                return "Demolitionist";
            case 4:
                return "Heatseeker";
            default:
                return null;
        }
    }

}
