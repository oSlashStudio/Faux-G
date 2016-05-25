using UnityEngine;
using System.Collections;

public class BossSmallController : Photon.MonoBehaviour {

    public GameObject bulletPrefab;

    public float angularVelocity = 540.0f;
    public float PhysicalHitDamage = 10.0f; // Damage everytime boss hits player
    public float defaultFireDelay = 5.0f;
    public float defaultSpawnMinionDelay = 15.0f;
    public int numBulletsSpawned = 12;

    private float fireDelay;
    private float spawnMinionDelay;

    // Cached components
    private Rigidbody2D rigidBody;
    private GameObject minion;

    // Use this for initialization
    void Start () {
        rigidBody = GetComponent<Rigidbody2D> ();

        if (!photonView.isMine) {
            rigidBody.isKinematic = true; // If this client can't control, set isKinematic to true
        }

        fireDelay = defaultFireDelay;
        spawnMinionDelay = defaultSpawnMinionDelay;
    }

    // Update is called once per frame
    void Update () {
        if (!photonView.isMine) {
            return;
        }

        rigidBody.angularVelocity = angularVelocity;

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
    }

    void Fire () {
        for (int i = 0; i < numBulletsSpawned; i++) {
            Vector3 angle = new Vector3 (0.0f, 0.0f, i * 360.0f / numBulletsSpawned);
            Vector3 positionShift = Quaternion.Euler (angle) * Vector3.right * 1.0f;

            photonView.RPC ("RpcFire", PhotonTargets.All, transform.position + positionShift, Quaternion.LookRotation (positionShift));
        }
    }

    [PunRPC]
    void RpcFire (Vector3 projectilePosition, Quaternion projectileRotation) {
        GameObject projectile = (GameObject) Instantiate (bulletPrefab, projectilePosition, projectileRotation);

        Physics2D.IgnoreCollision (projectile.GetComponent<Collider2D> (), gameObject.GetComponent<Collider2D> ());
    }

    void SpawnMinion () {
        Vector3 spawnPositionShift = -transform.position.normalized * 5.0f;

        minion = PhotonNetwork.InstantiateSceneObject ("Boss Small Minion", transform.position + spawnPositionShift, Quaternion.identity, 0, null);
    }

    void OnCollisionEnter2D (Collision2D collision) {
        HealthController targetHealthController = collision.gameObject.GetComponent<HealthController> ();
        if (targetHealthController != null) { // If target has health component
            targetHealthController.Damage (PhysicalHitDamage);
        }
    }

}
