using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[NetworkSettings(sendInterval = 0.05f)]
public class BossSmallController : NetworkBehaviour {

    public float maxAngularVelocity = 360.0f;
    public float angularVelocityIncreaseRate = 30.0f; // Increase in angular velocity per second
    public float physicalHitDamage = 20.0f;
    public float defaultPhysicalHitCooldown = 3.0f;
    public float defaultFireDelay = 5.0f;
    public float defaultSpawnMinionDelay = 10.0f;
    public int numBulletsSpawned = 18;

    public GameObject bulletPrefab;
    public GameObject minionPrefab;

    private Rigidbody2D rigidBody;
    private Component halo;
    
    [SyncVar(hook = "OnPositionSync")]
    private Vector3 position;
    [SyncVar(hook = "OnVelocitySync")]
    private Vector3 velocity;
    [SyncVar(hook = "OnAngularVelocitySync")]
    private float angularVelocity = 0.0f;
    [SyncVar]
    private bool isDamaging = false;

    private float physicalHitCooldown;
    private float fireDelay;
    private float spawnMinionDelay;

    private GameObject minion;

	// Use this for initialization
	void Start () {
        rigidBody = GetComponent<Rigidbody2D> ();
        halo = GetComponent ("Halo");

        physicalHitCooldown = defaultPhysicalHitCooldown;
        fireDelay = defaultFireDelay;
        spawnMinionDelay = defaultSpawnMinionDelay;
	}
	
	// Update is called once per frame
	void Update () {
        if (isServer) {
            IncreaseAngularVelocity ();
            rigidBody.angularVelocity = angularVelocity;

            physicalHitCooldown -= Time.deltaTime;
            if (physicalHitCooldown <= 0.0f && !isDamaging) {
                isDamaging = true;
            }

            fireDelay -= Time.deltaTime;
            if (fireDelay <= 0.0f) {
                Fire ();
                fireDelay = defaultFireDelay;
            }

            if (minion == null) {
                spawnMinionDelay -= Time.deltaTime;
                if (spawnMinionDelay <= 0.0f) {
                    SpawnMinion ();
                    spawnMinionDelay = defaultSpawnMinionDelay;
                }
            }

            SyncRigidbody ();
        }

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

    void SpawnMinion () {
        Vector3 spawnPositionShift = -transform.position.normalized * 5.0f;
        minion = (GameObject) Instantiate (minionPrefab, transform.position + spawnPositionShift, Quaternion.identity);

        NetworkServer.Spawn (minion);
    }

    void SyncRigidbody () {
        position = transform.position;
        velocity = rigidBody.velocity;
        // Angular velocity is already synced from server
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

    void OnPositionSync (Vector3 newPosition) {
        position = newPosition;

        transform.position = position;
    }

    void OnVelocitySync (Vector3 newVelocity) {
        velocity = newVelocity;

        rigidBody.velocity = velocity;
    }

    void OnAngularVelocitySync (float newAngularVelocity) {
        angularVelocity = newAngularVelocity;

        rigidBody.angularVelocity = angularVelocity;
    }

}
