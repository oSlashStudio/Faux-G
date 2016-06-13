using UnityEngine;
using System.Collections;

public class HealthController : Photon.MonoBehaviour {

    public GameObject calloutPrefab;

    public float maxHealth = 100.0f;
    [HideInInspector]
    public float currentHealth;
    public float armor;

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
                // Only add death data if dying object is a player
                networkManager.AddDeathData (PhotonNetwork.player.ID);
            }

            if (PhotonPlayer.Find (lastDamagerId) != null) {
                // Only add kill data if killing object is a player
                networkManager.AddKillData (lastDamagerId);
            }

            PhotonNetwork.Destroy (gameObject);
        }
	}

    [PunRPC]
    public void RpcHealOwner (float healAmount, Vector2 healPoint) {
        if (!photonView.isMine) { // Not owner, forward to owner
            photonView.RPC ("RpcHealOwner", photonView.owner, healAmount, healPoint);
        } else {
            Heal (healAmount, healPoint);
        }
    }

    /*
     * This function handles heal from player.
     */
    public void Heal (float healAmount, int healingPlayerId, Vector2 healPoint) {
        if (!photonView.isMine) {
            return;
        }
        if (healAmount == 0.0f) { // Ignore 0 heal
            return;
        }

        // Special case: if health after heal exceeds max health
        if (currentHealth + healAmount > maxHealth) {
            healAmount = maxHealth - currentHealth;
        }

        photonView.RPC ("RpcHeal", PhotonTargets.All, healAmount, healPoint);

        networkManager.AddHealData (healingPlayerId, healAmount);
    }

    /*
     * This function handles heal from enemy / unknown sources.
     */
    public void Heal (float healAmount, Vector2 healPoint) {
        if (!photonView.isMine) {
            return;
        }
        if (healAmount == 0.0f) { // Ignore 0 heal
            return;
        }

        // Special case: if health after heal exceeds max health
        if (currentHealth + healAmount > maxHealth) {
            healAmount = maxHealth - currentHealth;
        }

        photonView.RPC ("RpcHeal", PhotonTargets.All, healAmount, healPoint);
    }

    [PunRPC]
    void RpcHeal (float healAmount, Vector2 healPoint) {
        currentHealth += healAmount;

        InstantiateHealCallout (healAmount, healPoint);
    }

    void InstantiateHealCallout (float healAmount, Vector2 healPoint) {
        Vector3 calloutPosition = new Vector3 (healPoint.x, healPoint.y, -2.0f);
        Quaternion calloutRotation = (Camera.main == null) ? Quaternion.identity : Camera.main.transform.rotation;
        GameObject callout = (GameObject) Instantiate (calloutPrefab, calloutPosition, calloutRotation);
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

        // Scale damage amount depending on armor
        damageAmount = damageAmount * (1.0f - ArmorReduction ());
        // Special case: if health after damage goes below 0
        if (currentHealth - damageAmount < 0.0f) {
            damageAmount = currentHealth;
        }

        photonView.RPC ("RpcDamage", PhotonTargets.All, damageAmount, damagePoint);

        lastDamagerId = damagingPlayerId;
        networkManager.AddDamageData (damagingPlayerId, damageAmount);
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

        // Scale damage amount depending on armor
        damageAmount = damageAmount * (1.0f - ArmorReduction ());
        // Special case: if health after damage goes below 0
        if (currentHealth - damageAmount < 0.0f) {
            damageAmount = currentHealth;
        }

        photonView.RPC ("RpcDamage", PhotonTargets.All, damageAmount, damagePoint);
        lastDamagerId = 0;
    }

    [PunRPC]
    void RpcDamage (float damageAmount, Vector2 damagePoint) {
        currentHealth -= damageAmount;

        InstantiateDamageCallout (damageAmount, damagePoint);
    }

    void InstantiateDamageCallout (float damageAmount, Vector2 damagePoint) {
        Vector3 calloutPosition = new Vector3 (damagePoint.x, damagePoint.y, -2.0f);
        Quaternion calloutRotation = (Camera.main == null) ? Quaternion.identity : Camera.main.transform.rotation;
        GameObject callout = (GameObject) Instantiate (calloutPrefab, calloutPosition, calloutRotation);
        callout.GetComponent<TextMesh> ().text = "-" + damageAmount.ToString ("0");
        callout.GetComponent<TextMesh> ().color = Color.red;
    }

    /*
     * This method returns the percentage of damage blocked depending on the current armor.
     * Return 1 for 100% damage block, 0 for 0% damage block.
     */
    float ArmorReduction () {
        return armor / (10 + armor);
    }

}
