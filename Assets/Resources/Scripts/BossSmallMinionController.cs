using UnityEngine;
using System.Collections;

public class BossSmallMinionController : Photon.MonoBehaviour {

    public GameObject bulletPrefab;

    // Fire related variables
    public float defaultFireDelay = 3.0f;
    private float fireDelay;

    // Movement related variables
    public float movementRadius = 10.0f;
    public float moveSpeed = 5.0f;
    private Vector2 targetPosition;

    // Cached components
    private Rigidbody2D rigidBody;

    // Use this for initialization
    void Start () {
        rigidBody = GetComponent<Rigidbody2D> ();

        if (!photonView.isMine) {
            rigidBody.isKinematic = true; // If this client can't control, set isKinematic to true
        }

        fireDelay = defaultFireDelay;
    }
	
	// Update is called once per frame
	void Update () {
	    if (!photonView.isMine) {
            return;
        }

        fireDelay -= Time.deltaTime;
        if (fireDelay <= 0.0f) {
            Fire ();
            fireDelay = defaultFireDelay;
        }

        if ((targetPosition - (Vector2) transform.position).magnitude <= 1.0f) {
            Reroute ();
        }

        Move ();
    }

    void Fire () {
        GameObject slowestPlayer = GetSlowestPlayer ();
        if (slowestPlayer == null) { // No player on scene, stop firing
            return;
        }

        Quaternion projectileRotation = Quaternion.LookRotation (slowestPlayer.transform.position - transform.position);

        photonView.RPC ("RpcFire", PhotonTargets.All, transform.position, projectileRotation);
    }

    [PunRPC]
    void RpcFire (Vector3 projectilePosition, Quaternion projectileRotation) {
        GameObject projectile = (GameObject) Instantiate (bulletPrefab, projectilePosition, projectileRotation);

        Physics2D.IgnoreCollision (projectile.GetComponent<Collider2D> (), gameObject.GetComponent<Collider2D> ());
    }

    /*
     * This method returns the game object of the slowest player.
     */
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

}
