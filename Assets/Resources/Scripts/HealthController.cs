using UnityEngine;
using System.Collections;

public class HealthController : Photon.MonoBehaviour {

    public GameObject calloutPrefab;

    public float maxHealth = 100.0f;
    public float currentHealth;

    // Killer information variables
    private int lastDamagerViewId;

    // Cached components
    private NetworkManager networkManager;

	// Use this for initialization
	void Start () {
        networkManager = GameObject.FindObjectOfType<NetworkManager> ();
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
                networkManager.KillerViewId = lastDamagerViewId;
            }
            PhotonNetwork.Destroy (gameObject);
        }
	}

    public void Heal (float healAmount) {
        if (!photonView.isMine) {
            return;
        }
        photonView.RPC ("RpcHeal", PhotonTargets.AllBufferedViaServer, healAmount);
    }

    [PunRPC]
    void RpcHeal (float healAmount) {
        // Special case: if health after heal exceeds max health
        if (currentHealth + healAmount > maxHealth) {
            currentHealth = maxHealth;
        } else {
            currentHealth += healAmount;
        }

        InstantiateHealCallout (healAmount);
    }

    void InstantiateHealCallout (float healAmount) {
        Vector3 calloutPosition = new Vector3 (transform.position.x, transform.position.y, -2.0f);
        GameObject callout = (GameObject) Instantiate (calloutPrefab, calloutPosition, Quaternion.identity);
        callout.GetComponent<TextMesh> ().text = "+" + healAmount.ToString ("0");
        callout.GetComponent<TextMesh> ().color = Color.green;
    }

    /*
     * This function handles damage from player.
     */
    public void Damage (float damageAmount, int damagingPlayerViewId) {
        if (!photonView.isMine) {
            return;
        }
        photonView.RPC ("RpcDamage", PhotonTargets.AllBufferedViaServer, damageAmount);
        lastDamagerViewId = damagingPlayerViewId;
    }

    /*
     * This function handles damage from enemy / unknown sources.
     */
    public void Damage (float damageAmount) {
        if (!photonView.isMine) {
            return;
        }
        photonView.RPC ("RpcDamage", PhotonTargets.AllBufferedViaServer, damageAmount);
        lastDamagerViewId = 0;
    }

    [PunRPC]
    void RpcDamage (float damageAmount) {
        // Special case: if health after damage goes below 0
        if (currentHealth - damageAmount < 0.0f) {
            currentHealth = 0.0f;
        } else {
            currentHealth -= damageAmount;
        }

        InstantiateDamageCallout (damageAmount);
    }

    void InstantiateDamageCallout (float damageAmount) {
        Vector3 calloutPosition = new Vector3 (transform.position.x, transform.position.y, -2.0f);
        GameObject callout = (GameObject) Instantiate (calloutPrefab, calloutPosition, Quaternion.identity);
        callout.GetComponent<TextMesh> ().text = "-" + damageAmount.ToString ("0");
        callout.GetComponent<TextMesh> ().color = Color.red;
    }

}
