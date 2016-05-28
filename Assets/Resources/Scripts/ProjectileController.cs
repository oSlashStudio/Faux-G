using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

    public GameObject explosionPrefab;
    
    // Physics related variables
    public float projectileSpeed = 20.0f;
    public float projectileLifetime = 1.0f;
    public float projectileDamage = 5.0f;

    // Owner information variables
    private bool isPlayerInstantiated = false;
    private int instantiatorViewId;

    public int InstantiatorViewId {
        get {
            return instantiatorViewId;
        }
        set {
            isPlayerInstantiated = true;
            instantiatorViewId = value;
        }
    }

    // Cached components
    private Rigidbody2D rigidBody;

	// Use this for initialization
	void Start () {
        rigidBody = GetComponent<Rigidbody2D> ();
        rigidBody.velocity = transform.forward * projectileSpeed;
    }
	
	// Update is called once per frame
	void Update () {
        projectileLifetime -= Time.deltaTime;
        if (projectileLifetime <= 0.0f) {
            GameObject explosion = (GameObject) Instantiate (explosionPrefab, transform.position, Quaternion.identity);

            if (isPlayerInstantiated) {
                explosion.GetComponent<ExplosionController> ().InstantiatorViewId = instantiatorViewId;
            }

            Destroy (gameObject);
        }
	}

    void OnCollisionEnter2D (Collision2D collision) {
        HealthController targetHealthController = collision.gameObject.GetComponent<HealthController> ();
        if (targetHealthController != null) { // If target has health component
            if (isPlayerInstantiated) {
                targetHealthController.Damage (projectileDamage, instantiatorViewId, collision.contacts[0].point);
            } else {
                targetHealthController.Damage (projectileDamage, collision.contacts[0].point);
            }
        }

        GameObject explosion = (GameObject) Instantiate (explosionPrefab, transform.position, Quaternion.identity);

        if (isPlayerInstantiated) {
            explosion.GetComponent<ExplosionController> ().InstantiatorViewId = instantiatorViewId;
        }

        Destroy (gameObject);
    }

}
