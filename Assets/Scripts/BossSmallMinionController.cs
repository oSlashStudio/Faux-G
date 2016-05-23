using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[NetworkSettings (sendInterval = 0.05f)]
public class BossSmallMinionController : NetworkBehaviour {

    public GameObject bulletPrefab;
    public float defaultFireDelay = 5.0f;
    public float movementRadius = 5.0f;
    public float moveSpeed = 5.0f;

    private Rigidbody2D rigidBody;
    private float fireDelay;
    private Vector2 targetPosition;

    [SyncVar (hook = "OnPositionSync")]
    private Vector3 position;
    [SyncVar (hook = "OnVelocitySync")]
    private Vector3 velocity;

    // Use this for initialization
    void Start () {
        fireDelay = defaultFireDelay;
        rigidBody = GetComponent<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {
	    if (isServer) {
            fireDelay -= Time.deltaTime;

            if (fireDelay <= 0.0f) {
                Fire ();
                fireDelay = defaultFireDelay;
            }

            if ((targetPosition - (Vector2) transform.position).magnitude <= 1.0f) {
                Reroute ();
            }

            Move ();

            SyncRigidbody ();
        }
	}

    void Fire () {
        GameObject slowestPlayer = GetSlowestPlayer ();

        GameObject bullet = (GameObject) Instantiate (bulletPrefab, 
            transform.position, 
            Quaternion.LookRotation (slowestPlayer.transform.position - transform.position));
        bullet.GetComponent<ProjectileController> ().playerNetId = GetComponent<NetworkIdentity> ().netId;
        bullet.GetComponent<ProjectileController> ().playerConnectionId = -1;

        Physics2D.IgnoreCollision (bullet.GetComponent<Collider2D> (), gameObject.GetComponent<Collider2D> ());
        NetworkServer.Spawn (bullet);
    }

    GameObject GetSlowestPlayer () {
        GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");

        float minVelocity = Mathf.Infinity;
        GameObject minVelocityPlayer = null;

        foreach (GameObject player in players) {
            float playerVelocity = player.GetComponent<Rigidbody2D> ().velocity.magnitude;
            if (playerVelocity < minVelocity) {
                minVelocity = playerVelocity;
                minVelocityPlayer = player;
            }
        }

        return minVelocityPlayer;
    }

    void Reroute () {
        targetPosition = Random.insideUnitCircle.normalized * movementRadius;
    }

    void Move () {
        rigidBody.velocity = (targetPosition - (Vector2) transform.position).normalized * moveSpeed;
    }

    void SyncRigidbody () {
        position = transform.position;
        velocity = rigidBody.velocity;
    }

    void OnPositionSync (Vector3 newPosition) {
        position = newPosition;

        transform.position = position;
    }

    void OnVelocitySync (Vector3 newVelocity) {
        velocity = newVelocity;

        rigidBody.velocity = velocity;
    }

}
