using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class BossSmallController : NetworkBehaviour {

    public float maxAngularVelocity = 360.0f;
    public float angularVelocityIncreaseRate = 30.0f; // Increase in angular velocity per second
    public float physicalHitDamage = 20.0f;
    public float defaultPhysicalHitCooldown = 3.0f;
    public float defaultFireDelay = 5.0f;
    public int numBulletsSpawned = 18;

    public GameObject bulletPrefab;

    private Rigidbody2D rigidBody;
    private Component halo;
    
    [SyncVar]
    private Vector3 position;
    [SyncVar]
    private Vector3 velocity;
    [SyncVar]
    private float angularVelocity = 0.0f;
    [SyncVar]
    private bool isDamaging = false;

    private float physicalHitCooldown;
    private float fireDelay;

	// Use this for initialization
	void Start () {
        rigidBody = GetComponent<Rigidbody2D> ();
        halo = GetComponent ("Halo");

        physicalHitCooldown = defaultPhysicalHitCooldown;
        fireDelay = defaultFireDelay;
	}
	
	// Update is called once per frame
	void Update () {
        if (isServer) {
            IncreaseAngularVelocity ();
            rigidBody.angularVelocity = angularVelocity;

            physicalHitCooldown -= Time.deltaTime;

            if (physicalHitCooldown < 0.0f && !isDamaging) {
                isDamaging = true;
            }

            fireDelay -= Time.deltaTime;

            if (fireDelay < 0.0f) {
                Fire ();
                fireDelay = defaultFireDelay;
            }
        }

        SyncRigidbody ();

        if (isDamaging) {
            halo.GetType ().GetProperty ("enabled").SetValue (halo, true, null);
        } else {
            halo.GetType ().GetProperty ("enabled").SetValue (halo, false, null);
        }
    }

    void IncreaseAngularVelocity () {
        float angularVelocityIncrease = angularVelocityIncreaseRate * Time.deltaTime;
        if (angularVelocity + angularVelocityIncrease > maxAngularVelocity) {
            angularVelocity = maxAngularVelocity;
        } else {
            angularVelocity += angularVelocityIncrease;
        }
    }

    void Fire () {
        for (int i = 0; i < numBulletsSpawned; i++) {
            Vector3 angle = new Vector3 (0.0f, 0.0f, i * 360.0f / numBulletsSpawned);
            Vector3 positionShift = Quaternion.Euler (angle) * Vector3.right * 1.0f;

            GameObject bullet = (GameObject) Instantiate (bulletPrefab, position + positionShift, Quaternion.LookRotation (positionShift));

            bullet.GetComponent<ProjectileController> ().playerNetId = GetComponent<NetworkIdentity> ().netId;
            bullet.GetComponent<ProjectileController> ().playerConnectionId = -1;

            Physics2D.IgnoreCollision (bullet.GetComponent<Collider2D> (), gameObject.GetComponent<Collider2D> ());
            // Create projectile on client
            NetworkServer.Spawn (bullet);
        }
    }

    void SyncRigidbody () {
        if (isServer) {
            position = transform.position;
            velocity = rigidBody.velocity;
            // Angular velocity is already synced from server
        } else if (isClient) {
            transform.position = position;
            rigidBody.velocity = velocity;
            rigidBody.angularVelocity = angularVelocity;
        }
    }

    void OnCollisionEnter2D (Collision2D collision) {
        if (isServer) {
            if (collision.collider.tag.Equals ("Player")) {
                if (isDamaging) {
                    // Handle damage to player
                    HealthController playerHealthController = collision.gameObject.GetComponent<HealthController> ();
                    playerHealthController.ReduceHealth (physicalHitDamage, -1);

                    physicalHitCooldown = defaultPhysicalHitCooldown;
                    isDamaging = false;
                }

                angularVelocity = 0.0f;
            }
        }
    }

}
