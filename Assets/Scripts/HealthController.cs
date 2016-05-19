using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class HealthController : NetworkBehaviour {
    
    public GameObject damageCalloutPrefab;
    public float healthBarVerticalOffset = 1.5f;
    public float damageCalloutVerticalOffset = 1.5f;
    public float defaultRespawnTime = 5.0f;
    public float maxHealth = 100.0f;
    [SyncVar]
    public float currentHealth;
    [SyncVar]
    private bool isDead = false;
    [SyncVar]
    private int respawnTimeNormalized;
    private float respawnTime;
    
    private int lastDamagingPlayerConnectionId; // The connection id of the last damaging player
    [SyncVar]
    private string lastDamagingPlayerName;

    public override void OnStartServer () {
        currentHealth = maxHealth;
    }

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        // Check health threshold on each frame at server
        if (isServer) {
            if (currentHealth <= 0.0f) {
                if (gameObject.tag.Equals ("Player")) {
                    HandlePlayerDeath ();
                } else {
                    HandleObjectDeath ();
                }
            }
        }
    }

    void HandlePlayerDeath () {
        if (!isDead) { // If player has not been set as dead
            SetAsDead ();
            if (lastDamagingPlayerConnectionId == connectionToClient.connectionId) { // Suicide scenario
                ScoreboardController.Instance.ReduceScore (lastDamagingPlayerConnectionId, 1);
            } else { // Killed by others scenario
                ScoreboardController.Instance.IncreaseScore (lastDamagingPlayerConnectionId, 1);
            }
            GetComponent<PlayerController> ().RpcWaitForRespawn ();
        } else {
            // Wait for respawn timer before respawning
            if (respawnTime > 0.0f) {
                UpdateRespawnTime ();
                return;
            }

            RespawnPlayer ();
            // Destroy old player object
            NetworkServer.Destroy (gameObject);
        }
    }

    void SetAsDead () {
        isDead = true; // Set as dead
        respawnTime = defaultRespawnTime;
        lastDamagingPlayerName = GameManagerController.Instance.GetPlayerName (lastDamagingPlayerConnectionId);
    }

    void UpdateRespawnTime () {
        respawnTime -= Time.deltaTime; // Decrease current respawn timer
        int respawnTimeNormalized = Mathf.RoundToInt (respawnTime);
        if (this.respawnTimeNormalized != respawnTimeNormalized) {
            this.respawnTimeNormalized = respawnTimeNormalized;
        }
    }

    void RespawnPlayer () {
        // Respawn player routine
        Transform spawn = NetworkManager.singleton.GetStartPosition ();
        GameObject newPlayer = (GameObject) Instantiate (NetworkManager.singleton.playerPrefab, spawn.position, Quaternion.identity);
        newPlayer.GetComponent<NameTagController> ().playerName = GetComponent<NameTagController> ().playerName;
        newPlayer.GetComponent<PlayerController> ().playerColor = GetComponent<PlayerController> ().playerColor;
        NetworkServer.ReplacePlayerForConnection (connectionToClient, newPlayer, playerControllerId);
    }

    void HandleObjectDeath () {
        NetworkServer.Destroy (gameObject);
    }
    
    public void IncreaseHealth (float healAmount) {
        if (!isServer) {
            return;
        }

        if (currentHealth + healAmount > maxHealth) {
            currentHealth = maxHealth;
        } else {
            currentHealth += healAmount;
        }

        InstantiateHealCallout (healAmount);
    }

    void InstantiateHealCallout (float healAmount) {
        GameObject healCallout = (GameObject) Instantiate (damageCalloutPrefab,
                transform.position + transform.up * damageCalloutVerticalOffset +
                new Vector3 (0.0f, 0.0f, -1.0f), // z-offset
                transform.rotation);
        healCallout.GetComponent<DamageCalloutController> ().text = "+" + healAmount.ToString ("0");
        NetworkServer.Spawn (healCallout);
    }
    
    public void ReduceHealth (float damageAmount, int damagingPlayerConnectionId) {
        if (!isServer) {
            return;
        }

        if (currentHealth - damageAmount < 0.0f) {
            currentHealth = 0.0f;
        } else {
            currentHealth -= damageAmount;
        }

        lastDamagingPlayerConnectionId = damagingPlayerConnectionId;

        InstantiateDamageCallout (damageAmount);
    }

    void InstantiateDamageCallout (float damageAmount) {
        GameObject damageCallout = (GameObject) Instantiate (damageCalloutPrefab,
                transform.position + transform.up * damageCalloutVerticalOffset +
                new Vector3 (0.0f, 0.0f, -1.0f), // z-offset
                transform.rotation);
        damageCallout.GetComponent<DamageCalloutController> ().text = "-" + damageAmount.ToString ("0");
        NetworkServer.Spawn (damageCallout);
    }

    void OnGUI () {
        if (isLocalPlayer && isDead) {
            GUIStyle style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 16;
            GUI.Label (
                new Rect (0, 0, Screen.width, Screen.height), 
                "Killed by " + lastDamagingPlayerName + "\nRespawning in " + respawnTimeNormalized + " seconds", 
                style
            );
        }
    }

}
