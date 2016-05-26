using UnityEngine;
using Photon;
using System.Collections.Generic;

public class InGameNetworkManager : Photon.PunBehaviour {

    public GameObject[] spawnLocations;

    // Current state related variables
    private bool isInRoom = false;

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
        if (!isInRoom) { // Not in room yet
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

    public override void OnCreatedRoom () {
        PhotonNetwork.InstantiateSceneObject ("Boss Small", new Vector3 (1.0f, 1.0f, 0.0f), Quaternion.identity, 0, null);
    }

    public override void OnJoinedRoom () {
        isInRoom = true;

        SpawnPlayer ();
    }

    public void SpawnPlayer () {
        IsSpectating = false;

        int spawnLocationId = Random.Range (0, spawnLocations.Length);
        PhotonNetwork.Instantiate ("Sniper", spawnLocations[spawnLocationId].transform.position, Quaternion.identity, 0);
    }

}
