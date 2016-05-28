using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

    public GameObject explosionPrefab;

    // Physics related variables
    public float projectileSpeed;
    public float projectileLifetime;

    public float projectileHeal;
    public float projectileDamage;

    // Owner information variables
    private bool isPlayerInstantiated = false;
    private int instantiatorId;

    public int InstantiatorId {
        get {
            return instantiatorId;
        }
        set {
            isPlayerInstantiated = true;
            instantiatorId = value;
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
                explosion.GetComponent<ExplosionController> ().InstantiatorId = instantiatorId;
            }

            Destroy (gameObject);
        }
	}

    void OnCollisionEnter2D (Collision2D collision) {
        HealthController targetHealthController = collision.gameObject.GetComponent<HealthController> ();
        if (targetHealthController != null) { // If target has health component
            if (isPlayerInstantiated) {
                targetHealthController.Heal (projectileHeal, collision.contacts[0].point);
                targetHealthController.Damage (projectileDamage, instantiatorId, collision.contacts[0].point);
            } else {
                targetHealthController.Heal (projectileHeal, collision.contacts[0].point);
                targetHealthController.Damage (projectileDamage, collision.contacts[0].point);
            }
        }

        GameObject explosion = (GameObject) Instantiate (explosionPrefab, transform.position, Quaternion.identity);

        if (isPlayerInstantiated) {
            explosion.GetComponent<ExplosionController> ().InstantiatorId = instantiatorId;
        }

        Destroy (gameObject);
    }

}
