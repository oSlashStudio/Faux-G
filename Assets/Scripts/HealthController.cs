using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class HealthController : NetworkBehaviour {

    public GameObject healthBar;
    public GameObject damageCalloutPrefab;
    public float healthBarVerticalOffset = 1.5f;
    public float damageCalloutVerticalOffset = 1.5f;

    public float maxHealth = 100.0f;
    [SyncVar]
    public float currentHealth;

	// Use this for initialization
	void Start () {
        currentHealth = maxHealth;
	}
	
	// Update is called once per frame
	void Update () {
        // Check health threshold on each frame @server
        if (isServer) {
            if (currentHealth <= 0.0f) {
                Destroy (gameObject);
            }
        }
        // Rescale the healthbar based on current health percentage
        healthBar.transform.localScale = new Vector3 (currentHealth / maxHealth, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
    }
    
    public void IncreaseHealth (float healAmount) {
        if (!isServer) {
            return;
        }
        currentHealth += healAmount;
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
        currentHealth -= damageAmount;
        GameObject damageCallout = (GameObject) Instantiate (damageCalloutPrefab,
                transform.position + transform.up * damageCalloutVerticalOffset +
                new Vector3 (0.0f, 0.0f, -1.0f), // z-offset
                transform.rotation);
        damageCallout.GetComponent<DamageCalloutController> ().text = "-" + damageAmount.ToString ("0");
        NetworkServer.Spawn (damageCallout);
    }

}
