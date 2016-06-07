using UnityEngine;
using Photon;
using System.Collections.Generic;
using PhotonPlayerExtension;

public class InGameNetworkManager : Photon.PunBehaviour {

    public GameObject[] spawnLocations;

    [HideInInspector]
    public Dictionary<int, PlayerData> playerData = new Dictionary<int, PlayerData> ();
    [HideInInspector]
    public Dictionary<int, TeamData> teamData = new Dictionary<int, TeamData> ();

    private ExitGames.Client.Photon.Hashtable classHashtable;
    private int selectedClassId;
    private string[] classNames = new string[] {
        "Mercenary",
        "Virtuoso",
        "The Maniac",
        "NASA Fanboy",
        "Spinal Holler",
        "R.O.B.O.T",
        "Gargantuan",
        "Crazy Shaman", 
        "Heavy Bomber"
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
    private Broadcast[] broadcasts; // Broadcast circular buffer
    private int broadcastHead = 0;
    private int broadcastSize = 0;
    public float defaultBroadcastTimer = 10.0f;

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

    // Sound clips
    public AudioClip doubleKillSoundClip;
    public AudioClip killingSpreeSoundClip;
    public AudioClip dominatingSoundClip;
    public AudioClip godlikeSoundClip;

    // Cached components
    private AudioSource audioSource;
    private GUIStyle centeredLabel;

    // Use this for initialization
    void Start () {
        audioSource = GetComponent<AudioSource> ();
        broadcasts = new Broadcast[3];
        for (int i = 0; i < 3; i++) {
            broadcasts[i] = new Broadcast ();
        }
    }

    // Update is called once per frame
    protected virtual void Update () {
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
        
        int size = broadcastSize;
        int head = broadcastHead;
        while (size > 0) {
            head = (head - 1 + broadcasts.Length) % broadcasts.Length;
            broadcasts[head].time -= Time.deltaTime;
            if (broadcasts[head].time <= 0.0f) {
                broadcastSize--;
            }
            size--;
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

        // Initialize GUI Styles
        if (centeredLabel == null) {
            centeredLabel = new GUIStyle (GUI.skin.label);
            centeredLabel.alignment = TextAnchor.MiddleCenter;
        }

        GUILayout.BeginArea (RelativeRect (0, 0, 640, 300));
        BroadcastGUI ();
        GUILayout.FlexibleSpace ();
        GUILayout.EndArea ();

        GUILayout.BeginArea (RelativeRect (660, 0, 600, 100));
        TeamScoreGUI ();
        GUILayout.EndArea ();

        if (isDead) {
            GUILayout.BeginArea (RelativeRect (0, 240, 1920, 600));
            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            RespawnGUI ();
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();
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
        int size = broadcastSize;
        int head = broadcastHead;
        while (size > 0) {
            head = (head - 1 + broadcasts.Length) % broadcasts.Length;
            GUI.skin.label.normal.textColor = new Color (
                GUI.skin.label.normal.textColor.r,
                GUI.skin.label.normal.textColor.g,
                GUI.skin.label.normal.textColor.b,
                broadcasts[head].time / defaultBroadcastTimer
                );

            GUILayout.Label (broadcasts[head].message);

            GUI.skin.label.normal.textColor = new Color (
                GUI.skin.label.normal.textColor.r,
                GUI.skin.label.normal.textColor.g,
                GUI.skin.label.normal.textColor.b,
                1.0f
                );
            size--;
        }
    }

    void TeamScoreGUI () {
        GUIStyle boxStyle = new GUIStyle (GUI.skin.box);

        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace ();
        foreach (KeyValuePair<int, TeamData> entry in teamData) {
            string name = entry.Value.name;
            Color color = entry.Value.color;
            float score = entry.Value.score;

            boxStyle.normal.textColor = color;
            GUILayout.Box (name + ": " + score.ToString ("0"), boxStyle);
            GUILayout.FlexibleSpace ();
        }
        GUILayout.EndHorizontal ();
    }

    void RespawnGUI () {
        GUILayout.BeginVertical ();
        GUILayout.FlexibleSpace ();

        GUILayout.Label ("Killed by " + killerName + ", respawning in " + respawnTimer.ToString ("0") + "...", centeredLabel);
        GUILayout.Label ("Spectate other players by moving this spectate camera around.", centeredLabel);

        // Class selection window
        GUILayout.Label ("Select Class:", centeredLabel);
        selectedClassId = GUILayout.SelectionGrid (selectedClassId, classNames, 3);
        
        GUILayout.FlexibleSpace ();
        GUILayout.EndVertical ();
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
    protected virtual void RpcSendChatInput (string chatMessage, PhotonMessageInfo info) {
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
        GameObject player = PhotonNetwork.Instantiate ("Classes/" + className, spawnLocations[spawnLocationId].transform.position, Quaternion.identity, 0);

        byte rColor = (byte) PhotonNetwork.player.customProperties["rColor"];
        byte gColor = (byte) PhotonNetwork.player.customProperties["gColor"];
        byte bColor = (byte) PhotonNetwork.player.customProperties["bColor"];
        player.GetComponent<MeshRenderer> ().material.color = new Color (rColor, gColor, bColor);
    }

    string GetClassName (byte classId) {
        return classNames[classId];
    }

    void Broadcast (string message) {
        photonView.RPC ("RpcBroadcast", PhotonTargets.All, message);
    }

    [PunRPC]
    protected virtual void RpcBroadcast (string message) {
        PushBroadcast (message, defaultBroadcastTimer);
    }

    void PushBroadcast (string message, float time) {
        broadcasts[broadcastHead].message = message;
        broadcasts[broadcastHead].time = time;
        broadcastHead = (broadcastHead + 1) % broadcasts.Length;
        if (broadcastSize < broadcasts.Length) {
            broadcastSize++;
        }
    }

    void RegisterPlayer (PhotonPlayer player) {
        photonView.RPC ("RpcRegisterPlayer", PhotonTargets.AllBuffered, player.ID, player.name);
    }

    [PunRPC]
    protected virtual void RpcRegisterPlayer (int playerId, string playerName) {
        playerData[playerId] = new PlayerData (playerName);

        PhotonPlayer player = PhotonPlayer.Find (playerId);
        int teamId = (byte) player.customProperties["team"];
        if (!teamData.ContainsKey (teamId)) {
            string teamName = (string) player.customProperties["teamName"];
            Color teamColor = new Color (
                (byte) player.customProperties["rColor"],
                (byte) player.customProperties["gColor"],
                (byte) player.customProperties["bColor"]
                );
            teamData[teamId] = new TeamData (teamId, teamName, teamColor);
        }
        teamData[teamId].AssignPlayer (player);
    }

    public virtual void AddKillData (int killingPlayerId) {
        photonView.RPC ("RpcAddKillData", PhotonTargets.All, killingPlayerId);
    }

    [PunRPC]
    protected virtual void RpcAddKillData (int killingPlayerId) {
        playerData[killingPlayerId].AddKill ();

        switch (playerData[killingPlayerId].killStreak) {
            case 1:
                break;
            case 2:
                RpcBroadcast ("<b>" + playerData[killingPlayerId].playerName  + "</b> got a Double Kill!");
                audioSource.PlayOneShot (doubleKillSoundClip);
                break;
            case 3:
                RpcBroadcast ("<b>" + playerData[killingPlayerId].playerName + "</b> is on a Killing Spree!");
                audioSource.PlayOneShot (killingSpreeSoundClip);
                break;
            case 4:
                RpcBroadcast ("<b>" + playerData[killingPlayerId].playerName + "</b> is Dominating!");
                audioSource.PlayOneShot (dominatingSoundClip);
                break;
            default: // More than or equals to 5
                RpcBroadcast ("<b>" + playerData[killingPlayerId].playerName + "</b> is GODLIKE!");
                audioSource.PlayOneShot (godlikeSoundClip);
                break;
        }
    }

    public virtual void AddDeathData (int dyingPlayerId) {
        photonView.RPC ("RpcAddDeathData", PhotonTargets.All, dyingPlayerId);
    }

    [PunRPC]
    protected virtual void RpcAddDeathData (int dyingPlayerId) {
        playerData[dyingPlayerId].AddDeath ();
    }

    public virtual void AddDamageData (int damagingPlayerId, float damage) {
        photonView.RPC ("RpcAddDamageData", PhotonTargets.All, damagingPlayerId, damage);
    }

    [PunRPC]
    protected virtual void RpcAddDamageData (int damagingPlayerId, float damage) {
        playerData[damagingPlayerId].AddDamage (damage);
    }

    public virtual void AddHealData (int healingPlayerId, float heal) {
        photonView.RPC ("RpcAddHealData", PhotonTargets.All, healingPlayerId, heal);
    }

    [PunRPC]
    protected virtual void RpcAddHealData (int healingPlayerId, float heal) {
        playerData[healingPlayerId].AddHeal (heal);
    }

    public virtual void AddScore (int teamId, float scoreIncrease) {
        photonView.RPC ("RpcAddScore", PhotonTargets.All, teamId, scoreIncrease);
    }

    [PunRPC]
    protected virtual void RpcAddScore (int teamId, float scoreIncrease) {
        teamData[teamId].score += scoreIncrease;
    }

    public virtual void EndGame () {
        photonView.RPC ("RpcEndGame", PhotonTargets.All);
    }

    [PunRPC]
    protected virtual void RpcEndGame () {
        PhotonNetwork.LeaveRoom ();
    }

    public override void OnLeftRoom () {
        PhotonNetwork.LoadLevel (1);
    }

}
