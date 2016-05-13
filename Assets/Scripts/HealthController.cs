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
        // Check health threshold on every frame
        if (currentHealth <= 0.0f) {
            Destroy (gameObject);
        }
        // Rescale the healthbar based on current health percentage
        healthBar.transform.localScale = new Vector3 (currentHealth / maxHealth, transform.localScale.y, transform.localScale.z);
    }

    void OnCollisionEnter2D (Collision2D collision) {
        // Upon collision of transform with damaging projectiles
        if (collision.collider.tag.Equals ("Projectile")) {
            // Instantiate damage callout and assign reference
            GameObject damageCallout = (GameObject) Instantiate (damageCalloutPrefab, 
                transform.position + transform.up * damageCalloutVerticalOffset + 
                new Vector3 (0.0f, 0.0f, -1.0f), // z-offset
                transform.rotation);
            // Check colliding projectile type
            switch (collision.collider.name) {
                case "Rifle Bullet(Clone)":
                    // Modify damage callout text to indicate damage
                    damageCallout.GetComponent<TextMesh> ().text = "-10";
                    CmdReduceHealth (10.0f); // TO-DO: Move rifle bullet damage to global preferences
                    break;
                case "Rocket Shell(Clone)":
                    // Modify damage callout text to indicate damage
                    damageCallout.GetComponent<TextMesh> ().text = "-100";
                    CmdReduceHealth (100.0f); // TO-DO: Move rocket shell damage to global preferences
                    break;
                case "Minigun Bullet(Clone)":
                    // Modify damage callout text to indicate damage
                    damageCallout.GetComponent<TextMesh> ().text = "-2";
                    CmdReduceHealth (2.0f); // TO-DO: Move minigun bullet damage to global preferences
                    break;
                default:
                    break;
            }
        }
    }
    
    public void CmdIncreaseHealth (float healAmount) {
        if (!isServer) {
            return;
        }
        currentHealth += healAmount;
    }
    
    public void CmdReduceHealth (float damageAmount) {
        if (!isServer) {
            return;
        }
        currentHealth -= damageAmount;
    }

}
