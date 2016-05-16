using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class HealthController : NetworkBehaviour {

    public GameObject healthBar;
    public GameObject damageCalloutPrefab;
    public float healthBarVerticalOffset = 1.5f;
    public float damageCalloutVerticalOffset = 1.5f;
    public float respawnTimer = 5.0f;
    public float maxHealth = 100.0f;
    [SyncVar]
    public float currentHealth;
    private bool isDead = false;

    public override void OnStartServer () {
        currentHealth = maxHealth;
    }

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        // Rescale the healthbar based on current health percentage
        healthBar.transform.localScale = new Vector3 (currentHealth / maxHealth, healthBar.transform.localScale.y, healthBar.transform.localScale.z);

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
            GetComponent<PlayerController> ().RpcWaitForRespawn ();
        } else {
            // Wait for respawn timer before respawning
            if (respawnTimer > 0.0f) {
                UpdateRespawnTimer ();
                return;
            }

            RespawnPlayer ();
            // Destroy old player object
            NetworkServer.Destroy (gameObject);
        }
    }

    void SetAsDead () {
        isDead = true; // Set as dead
        RpcSetAsDead (); // Transmit dead status to client
    }

    [ClientRpc]
    void RpcSetAsDead () {
        if (isLocalPlayer) {
            isDead = true;
        }
    }

    void UpdateRespawnTimer () {
        respawnTimer -= Time.fixedDeltaTime; // Decrease current respawn timer
        RpcUpdateRespawnTimer (respawnTimer); // Transmit current respawn timer to client
    }

    [ClientRpc]
    void RpcUpdateRespawnTimer (float respawnTimer) {
        if (isLocalPlayer) {
            this.respawnTimer = respawnTimer;
        }
    }

    void RespawnPlayer () {
        // Respawn player routine
        Transform spawn = NetworkManager.singleton.GetStartPosition ();
        GameObject newPlayer = (GameObject) Instantiate (NetworkManager.singleton.playerPrefab, spawn.position, spawn.rotation);
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
    
    public void ReduceHealth (float damageAmount) {
        if (!isServer) {
            return;
        }

        if (currentHealth - damageAmount < 0.0f) {
            currentHealth = 0.0f;
        } else {
            currentHealth -= damageAmount;
        }

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
        if (isDead) {
            GUIStyle style = GUI.skin.GetStyle ("Label");
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label (new Rect (0, 0, Screen.width, Screen.height), "Respawning in " + respawnTimer.ToString ("0.00") + " seconds", style);
        }
    }

}
