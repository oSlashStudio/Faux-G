using UnityEngine;
using System.Collections;

public class HomingProjectileController : MonoBehaviour {

    public GameObject explosionPrefab;

    // Physics related variables
    public float projectileSpeed = 20.0f;
    public float projectileLifetime = 1.0f;
    public float projectileDamage = 5.0f;
    public bool isArmorPiercing;

    // Homing related variables
    private int targetViewId;
    private GameObject target;

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

    public int Target {
        get {
            return targetViewId;
        }
        set {
            targetViewId = value;
            PhotonView targetPhotonView = PhotonView.Find (targetViewId);
            if (targetPhotonView == null) {
                return;
            }
            target = targetPhotonView.gameObject;
        }
    }

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
            explosion.GetComponent<ExplosionController> ().InstantiatorId = instantiatorId;

            Destroy (gameObject);
        }

        if (target != null) { // Has a homing target
            transform.LookAt (target.transform.position);
            rigidBody.velocity = transform.forward * projectileSpeed;
        }
    }

    void OnCollisionEnter2D (Collision2D collision) {
        HealthController targetHealthController = collision.gameObject.GetComponent<HealthController> ();
        if (targetHealthController != null) { // If target has health component
            if (isPlayerInstantiated) {
                targetHealthController.Damage (projectileDamage, collision.contacts[0].point, isArmorPiercing, instantiatorId);
            } else {
                targetHealthController.Damage (projectileDamage, collision.contacts[0].point, isArmorPiercing);
            }
        }

        GameObject explosion = (GameObject) Instantiate (explosionPrefab, transform.position, Quaternion.identity);
        explosion.GetComponent<ExplosionController> ().InstantiatorId = instantiatorId;

        Destroy (gameObject);
    }

}
