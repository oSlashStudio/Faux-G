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
                    GetComponent<PlayerController> ().RpcWaitForRespawn (respawnTimer);
                    // Wait for respawn timer before respawning
                    while (respawnTimer > 0.0f) {
                        respawnTimer -= Time.fixedDeltaTime;
                        return;
                    }
                    // Destroy player
                    NetworkServer.Destroy (gameObject);
                    // Respawn player
                    Transform spawn = NetworkManager.singleton.GetStartPosition ();
                    GameObject newPlayer = (GameObject) Instantiate (NetworkManager.singleton.playerPrefab, spawn.position, spawn.rotation);
                    NetworkServer.ReplacePlayerForConnection (connectionToClient, newPlayer, playerControllerId);
                } else {
                    NetworkServer.Destroy (gameObject);
                }
            }
        }
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

        GameObject damageCallout = (GameObject) Instantiate (damageCalloutPrefab,
                transform.position + transform.up * damageCalloutVerticalOffset +
                new Vector3 (0.0f, 0.0f, -1.0f), // z-offset
                transform.rotation);
        damageCallout.GetComponent<DamageCalloutController> ().text = "-" + damageAmount.ToString ("0");
        NetworkServer.Spawn (damageCallout);
    }

}
