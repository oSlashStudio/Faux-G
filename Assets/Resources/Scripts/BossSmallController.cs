using UnityEngine;
using System.Collections;

public class BossSmallController : Photon.MonoBehaviour {

    public GameObject bulletPrefab;

    public float angularVelocity = 540.0f;
    public float physicalHitDamage = 10.0f; // Damage everytime boss hits player
    public float defaultFireDelay = 5.0f;
    public float defaultSpawnMinionDelay = 15.0f;
    public int numBulletsSpawned = 12;

    private float fireDelay;
    private float spawnMinionDelay;

    // Cached components
    private Rigidbody2D rigidBody;
    private GameObject minion;
    private PhotonTransformView photonTransformView;
    private InGameNetworkManager networkManager;

    // Use this for initialization
    void Start () {
        rigidBody = GetComponent<Rigidbody2D> ();
        photonTransformView = GetComponent<PhotonTransformView> ();
        networkManager = GameObject.FindObjectOfType<InGameNetworkManager> ();

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

        // Synchronize velocity and angular velocity over the network
        photonTransformView.SetSynchronizedValues (rigidBody.velocity, rigidBody.angularVelocity);
    }

    void Fire () {
        photonView.RPC ("RpcFire", PhotonTargets.All, transform.position);
    }

    [PunRPC]
    void RpcFire (Vector3 position) {
        for (int i = 0; i < numBulletsSpawned; i++) {
            Vector3 angle = new Vector3 (0.0f, 0.0f, i * 360.0f / numBulletsSpawned);
            Vector3 positionShift = Quaternion.Euler (angle) * Vector3.right * 1.0f;

            GameObject projectile = (GameObject) Instantiate (bulletPrefab, position + positionShift, Quaternion.LookRotation (positionShift));

            Physics2D.IgnoreCollision (projectile.GetComponent<Collider2D> (), gameObject.GetComponent<Collider2D> ());
        }
    }

    void SpawnMinion () {
        Vector3 spawnPositionShift = -transform.position.normalized * 10.0f;

        minion = PhotonNetwork.InstantiateSceneObject ("Boss Small Minion", transform.position + spawnPositionShift, Quaternion.identity, 0, null);
    }

    void OnCollisionEnter2D (Collision2D collision) {
        if (!photonView.isMine) { // Only check collision on master client
            return;
        }

        HealthController targetHealthController = collision.gameObject.GetComponent<HealthController> ();
        if (targetHealthController != null) { // If target has health component
            // Assumption: A target with health also has a photon view component
            PhotonView targetPhotonView = collision.gameObject.GetComponent<PhotonView> ();
            photonView.RPC ("RpcApplyPhysicalHitDamage", targetPhotonView.owner, targetPhotonView.viewID, physicalHitDamage, collision.contacts[0].point);
        }
    }

    [PunRPC]
    void RpcApplyPhysicalHitDamage (int targetViewId, float physicalHitDamage, Vector2 damagePoint) {
        PhotonView.Find (targetViewId).GetComponent<HealthController> ().Damage (physicalHitDamage, damagePoint);
    }

    void OnDestroy () {
        if (!photonView.isMine) {
            return;
        }

        if (minion != null) {
            PhotonNetwork.Destroy (minion);
        }
        networkManager.EndGame ();
    }

}
