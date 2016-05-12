using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class HealthController : NetworkBehaviour {

    public GameObject healthBarPrefab;
    public GameObject damageCalloutPrefab;
    public float healthBarVerticalOffset = -1.0f;
    public float damageCalloutVerticalOffset = 1.0f;

    public HealthBarController healthBarController;

	// Use this for initialization
	void Start () {
        if (isLocalPlayer) {
            CmdSpawnHealthBar (GetComponent<NetworkIdentity> ());
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    [Command]
    void CmdSpawnHealthBar (NetworkIdentity parentNetworkIdentity) {
        GameObject healthBar = (GameObject) Instantiate (healthBarPrefab,
            new Vector3 (transform.position.x, transform.position.y + healthBarVerticalOffset, transform.position.z),
            transform.rotation);
        // Assign transform as health bar's parent
        healthBar.transform.parent = parentNetworkIdentity.transform;
        // Assign health bar's controller to parameter (access optimization)
        healthBarController = healthBar.GetComponent<HealthBarController> ();
        // Spawn health bar on server side
        NetworkServer.SpawnWithClientAuthority (healthBar, base.connectionToClient);
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
                    healthBarController.reduceHealth (10.0f); // TO-DO: Move rifle bullet damage to global preferences
                    break;
                case "Rocket Shell(Clone)":
                    // Modify damage callout text to indicate damage
                    damageCallout.GetComponent<TextMesh> ().text = "-100";
                    healthBarController.reduceHealth (100.0f); // TO-DO: Move rocket shell damage to global preferences
                    break;
                case "Minigun Bullet(Clone)":
                    // Modify damage callout text to indicate damage
                    damageCallout.GetComponent<TextMesh> ().text = "-2";
                    healthBarController.reduceHealth (2.0f); // TO-DO: Move minigun bullet damage to global preferences
                    break;
                default:
                    break;
            }
        }
    }
}
