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
        "Healer", 
        "Explosive Master"
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

    // Chat related variables
    private Vector2 chatScrollPos = new Vector2 (0.0f, Mathf.Infinity);
    private string chatInput = "";
    private List<string> chatMessages = new List<string> ();

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
                // Update selected class information
                classHashtable["class"] = (byte) selectedClassId;
                PhotonNetwork.player.SetCustomProperties (classHashtable);
                isDead = false;
                SpawnPlayer ();
            }
        }

        broadcastTimer -= Time.deltaTime;
        if (broadcastTimer <= 0.0f) {
            broadcastMessage = "";
        }
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
        if (!isInGame) { // Not in game yet
            return;
        }

        GUILayout.BeginArea (RelativeRect (0, 0, 400, 100));
        BroadcastGUI ();
        GUILayout.EndArea ();

        if (isDead) {
            GUILayout.BeginArea (RelativeRect (160, 340, 1600, 400));
            RespawnGUI ();
            GUILayout.EndArea ();
        }

        GUILayout.BeginArea (RelativeRect (0, 756, 576, 324));
        ChatBoxGUI ();
        ChatKeyListener ();
        ChatFieldGUI ();
        GUILayout.EndArea ();

        GUILayout.BeginArea (RelativeRect (1344, 756, 576, 324));
        PlayerDataGUI ();
        GUILayout.EndArea ();
    }

    void BroadcastGUI () {
        GUI.skin.label.normal.textColor = new Color (
            GUI.skin.label.normal.textColor.r,
            GUI.skin.label.normal.textColor.g,
            GUI.skin.label.normal.textColor.b,
            broadcastTimer / defaultBroadcastTimer
            );

        GUILayout.Label (broadcastMessage);

        GUI.skin.label.normal.textColor = new Color (
            GUI.skin.label.normal.textColor.r,
            GUI.skin.label.normal.textColor.g,
            GUI.skin.label.normal.textColor.b,
            1.0f
            );
    }

    void RespawnGUI () {
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;

        GUILayout.Label ("Killed by " + killerName + ", respawning in " + respawnTimer.ToString ("0") + "...");
        GUILayout.Label ("Spectate other players by moving this spectate camera around.");

        // Class selection window
        GUILayout.Label ("Select Class:");
        selectedClassId = GUILayout.SelectionGrid (selectedClassId, classNames, 4);

        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
    }

    void ChatBoxGUI () {
        GUI.SetNextControlName ("ChatBox");
        chatScrollPos = GUILayout.BeginScrollView (chatScrollPos, GUIStyle.none, GUIStyle.none);
        GUILayout.FlexibleSpace ();
        foreach (string message in chatMessages) {
            GUILayout.Label (message);
        }
        // Auto scrolling
        chatScrollPos = new Vector2 (0.0f, Mathf.Infinity);
        GUILayout.EndScrollView ();
    }

    void ChatKeyListener () {
        // Handle enter key press
        if ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return)) {
            if (string.IsNullOrEmpty (chatInput)) { // Empty chat input
                ToggleFocus ("ChatField");
            } else { // Non-empty chat input
                if (GUI.GetNameOfFocusedControl () != "ChatField") { // Not focused yet
                    ToggleFocus ("ChatField");
                } else if (IsSafeForWork (chatInput)) { // Chat filter
                    photonView.RPC ("RpcSendChatInput", PhotonTargets.All, chatInput);
                    chatInput = "";
                    ToggleFocus ("ChatField");
                }
            }
        }
    }

    void ChatFieldGUI () {
        GUI.SetNextControlName ("ChatField");
        chatInput = GUILayout.TextField (chatInput);
    }

    void ToggleFocus (string focusTarget) {
        if (GUI.GetNameOfFocusedControl () == focusTarget) { // Currently focused
            GUI.FocusControl ("");
        } else { // Currently not focused
            GUI.FocusControl (focusTarget);
        }
    }

    /*
     * This method handles message filtering.
     */
    bool IsSafeForWork (string chatInput) {
        return true;
    }

    [PunRPC]
    void RpcSendChatInput (string chatMessage, PhotonMessageInfo info) {
        string senderName;

        if (info == null || info.sender == null) { // Empty message info
            senderName = "Anonymous";
        } else {
            if (string.IsNullOrEmpty (info.sender.name.Trim (' '))) { // Empty player name
                senderName = "Player " + info.sender.ID;
            } else { // Normal case, player has a name
                senderName = info.sender.name;
            }
        }
        
        chatMessages.Add (senderName + ": " + chatMessage);
    }

    void PlayerDataGUI () {
        GUILayout.BeginVertical ();
        // Row #1
        GUILayout.BeginHorizontal ();
        GUILayout.Label ("Name");
        GUILayout.Label ("K", GUILayout.Width (RelativeWidth (100)));
        GUILayout.Label ("D", GUILayout.Width (RelativeWidth (100)));
        GUILayout.Label ("Dmg", GUILayout.Width (RelativeWidth (100)));
        GUILayout.Label ("Heal", GUILayout.Width (RelativeWidth (100)));
        GUILayout.EndHorizontal ();
        // Row #2 ~ #N
        foreach (KeyValuePair<int, PlayerData> data in playerData) {
            GUILayout.BeginHorizontal ();
            GUILayout.Label (data.Value.playerName);
            GUILayout.Label (data.Value.kill.ToString (), GUILayout.Width (RelativeWidth (100)));
            GUILayout.Label (data.Value.death.ToString (), GUILayout.Width (RelativeWidth (100)));
            GUILayout.Label (data.Value.damage.ToString ("0"), GUILayout.Width (RelativeWidth (100)));
            GUILayout.Label (data.Value.heal.ToString ("0"), GUILayout.Width (RelativeWidth (100)));
            GUILayout.EndHorizontal ();
        }
        GUILayout.EndVertical ();
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
