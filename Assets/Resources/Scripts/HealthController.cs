using UnityEngine;
using System.Collections;

public class HealthController : Photon.MonoBehaviour {

    public GameObject calloutPrefab;

    public float maxHealth = 100.0f;
    public float currentHealth;

    // Killer information variables
    private int lastDamagerId;

    // Cached components
    private InGameNetworkManager networkManager;

	// Use this for initialization
	void Start () {
        networkManager = GameObject.FindObjectOfType<InGameNetworkManager> ();
        currentHealth = maxHealth;
	}
	
	// Update is called once per frame
	void Update () {
        if (!photonView.isMine) {
            return;
        }
	    if (currentHealth == 0.0f) {
            if (gameObject.tag.Equals ("Player")) {
                // Handle player respawn
                networkManager.IsDead = true;
                networkManager.KillerId = lastDamagerId;
            }
            PhotonNetwork.Destroy (gameObject);
        }
	}

    public void Heal (float healAmount, Vector2 healPoint) {
        if (!photonView.isMine) {
            return;
        }
        if (healAmount == 0.0f) { // Ignore 0 heal
            return;
        }
        photonView.RPC ("RpcHeal", PhotonTargets.AllBufferedViaServer, healAmount, healPoint);
    }

    [PunRPC]
    void RpcHeal (float healAmount, Vector2 healPoint) {
        // Special case: if health after heal exceeds max health
        if (currentHealth + healAmount > maxHealth) {
            currentHealth = maxHealth;
        } else {
            currentHealth += healAmount;
        }

        InstantiateHealCallout (healAmount, healPoint);
    }

    void InstantiateHealCallout (float healAmount, Vector2 healPoint) {
        Vector3 calloutPosition = new Vector3 (healPoint.x, healPoint.y, -2.0f);
        GameObject callout = (GameObject) Instantiate (calloutPrefab, calloutPosition, Quaternion.identity);
        callout.GetComponent<TextMesh> ().text = "+" + healAmount.ToString ("0");
        callout.GetComponent<TextMesh> ().color = Color.green;
    }

    /*
     * This function handles damage from player.
     */
    public void Damage (float damageAmount, int damagingPlayerId, Vector2 damagePoint) {
        if (!photonView.isMine) {
            return;
        }
        if (damageAmount == 0.0f) { // Ignore 0 damage
            return;
        }
        photonView.RPC ("RpcDamage", PhotonTargets.AllBufferedViaServer, damageAmount, damagePoint);
        lastDamagerId = damagingPlayerId;
    }

    /*
     * This function handles damage from enemy / unknown sources.
     */
    public void Damage (float damageAmount, Vector2 damagePoint) {
        if (!photonView.isMine) {
            return;
        }
        if (damageAmount == 0.0f) { // Ignore 0 damage
            return;
        }
        photonView.RPC ("RpcDamage", PhotonTargets.AllBufferedViaServer, damageAmount, damagePoint);
        lastDamagerId = 0;
    }

    [PunRPC]
    void RpcDamage (float damageAmount, Vector2 damagePoint) {
        // Special case: if health after damage goes below 0
        if (currentHealth - damageAmount < 0.0f) {
            currentHealth = 0.0f;
        } else {
            currentHealth -= damageAmount;
        }

        InstantiateDamageCallout (damageAmount, damagePoint);
    }

    void InstantiateDamageCallout (float damageAmount, Vector2 damagePoint) {
        Vector3 calloutPosition = new Vector3 (damagePoint.x, damagePoint.y, -2.0f);
        GameObject callout = (GameObject) Instantiate (calloutPrefab, calloutPosition, Quaternion.identity);
        callout.GetComponent<TextMesh> ().text = "-" + damageAmount.ToString ("0");
        callout.GetComponent<TextMesh> ().color = Color.red;
    }

}
