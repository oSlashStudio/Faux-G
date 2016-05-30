using UnityEngine;
using Photon;
using System.Collections.Generic;

public class InGameNetworkManager : Photon.PunBehaviour {

    public GameObject[] spawnLocations;

    private Dictionary<int, PlayerData> playerData = new Dictionary<int, PlayerData> ();

    private ExitGames.Client.Photon.Hashtable classHashtable;
    private int selectedClassId;
    private string[] classNames = new string[] {
        "Assaulter",
        "Recon",
        "Light",
        "Demolitionist",
        "Heatseeker",
        "Sieger",
        "Heavy",
        "Healer"
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

    // Broadcast message related variables
    private string broadcastMessage;
    public float defaultBroadcastTimer = 10.0f;
    private float broadcastTimer;

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

            Broadcast ("<b>" + killerName + "</b>" + " just killed " + "<b>" + PhotonNetwork.player.name + "</b>");
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

        broadcastTimer -= Time.deltaTime;
        if (broadcastTimer <= 0.0f) {
            broadcastMessage = "";
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

        if (!string.IsNullOrEmpty (broadcastMessage) && broadcastTimer >= 0.0f) {
            GUIStyle broadcastStyle = GUI.skin.label;
            broadcastStyle.alignment = TextAnchor.UpperCenter;
            broadcastStyle.richText = true;
            broadcastStyle.normal.textColor = new Color (
                broadcastStyle.normal.textColor.r,
                broadcastStyle.normal.textColor.g,
                broadcastStyle.normal.textColor.b,
                broadcastTimer / defaultBroadcastTimer
                );

            GUILayout.BeginArea (new Rect (Screen.width / 2.0f - 100.0f, 0.0f, 200.0f, 50.0f));
            GUILayout.Label (broadcastMessage, broadcastStyle);
            GUILayout.EndArea ();

            // Reset style (without this reset, other GUI components will be affected)
            broadcastStyle.normal.textColor = new Color (
                broadcastStyle.normal.textColor.r,
                broadcastStyle.normal.textColor.g,
                broadcastStyle.normal.textColor.b,
                1.0f
                );
        }

        GUILayout.BeginArea (new Rect (Screen.width - 250.0f, Screen.height - 150.0f, 250.0f, 150.0f));
        GUILayout.BeginVertical ();
        // Row #1
        GUILayout.BeginHorizontal (GUILayout.Height (25.0f));
        GUILayout.Label ("Name", GUILayout.Width (50.0f));
        GUILayout.Label ("K", GUILayout.Width (25.0f));
        GUILayout.Label ("D", GUILayout.Width (25.0f));
        GUILayout.Label ("Dmg", GUILayout.Width (50.0f));
        GUILayout.Label ("Heal", GUILayout.Width (50.0f));
        GUILayout.EndHorizontal ();
        // Row #2 ~ #N
        foreach (KeyValuePair<int, PlayerData> data in playerData) {
            GUILayout.BeginHorizontal (GUILayout.Height (25.0f));
            GUILayout.Label (data.Value.playerName, GUILayout.Width (50.0f));
            GUILayout.Label (data.Value.kill.ToString (), GUILayout.Width (25.0f));
            GUILayout.Label (data.Value.death.ToString (), GUILayout.Width (25.0f));
            GUILayout.Label (data.Value.damage.ToString ("0"), GUILayout.Width (50.0f));
            GUILayout.Label (data.Value.heal.ToString ("0"), GUILayout.Width (50.0f));
            GUILayout.EndHorizontal ();
        }
        GUILayout.EndVertical ();
        GUILayout.EndArea ();
    }

    void OnLevelWasLoaded () {
        isInGame = true;
        // Scene has been loaded, resume in-game instantiation
        PhotonNetwork.isMessageQueueRunning = true;

        classHashtable = PhotonNetwork.player.customProperties;
        selectedClassId = (byte) classHashtable["class"];
        SpawnPlayer ();

        RegisterPlayer (PhotonNetwork.player);
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

    void Broadcast (string message) {
        photonView.RPC ("RpcBroadcast", PhotonTargets.All, message);
    }

    [PunRPC]
    void RpcBroadcast (string message) {
        broadcastMessage = message;
        broadcastTimer = defaultBroadcastTimer;
    }

    void RegisterPlayer (PhotonPlayer player) {
        photonView.RPC ("RpcRegisterPlayer", PhotonTargets.AllBuffered, player.ID, player.name);
    }

    [PunRPC]
    void RpcRegisterPlayer (int playerId, string playerName) {
        playerData[playerId] = new PlayerData (playerName);
    }

    public void AddKillData (int killingPlayerId) {
        photonView.RPC ("RpcAddKillData", PhotonTargets.All, killingPlayerId);
    }

    [PunRPC]
    void RpcAddKillData (int killingPlayerId) {
        playerData[killingPlayerId].AddKill ();
    }

    public void AddDeathData (int dyingPlayerId) {
        photonView.RPC ("RpcAddDeathData", PhotonTargets.All, dyingPlayerId);
    }

    [PunRPC]
    void RpcAddDeathData (int dyingPlayerId) {
        playerData[dyingPlayerId].AddDeath ();
    }

    public void AddDamageData (int damagingPlayerId, float damage) {
        photonView.RPC ("RpcAddDamageData", PhotonTargets.All, damagingPlayerId, damage);
    }

    [PunRPC]
    void RpcAddDamageData (int damagingPlayerId, float damage) {
        playerData[damagingPlayerId].AddDamage (damage);
    }

    public void AddHealData (int healingPlayerId, float heal) {
        photonView.RPC ("RpcAddHealData", PhotonTargets.All, healingPlayerId, heal);
    }

    [PunRPC]
    void RpcAddHealData (int healingPlayerId, float heal) {
        playerData[healingPlayerId].AddHeal (heal);
    }

}
