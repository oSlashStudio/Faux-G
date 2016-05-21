using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class HomingProjectileController : NetworkBehaviour {

    public GameObject explosionPrefab;
    public float projectileSpeed = 5.0f;
    public float projectileDamage = 10.0f;
    public float projectileLifetime = 2.0f;
    public float effectRange = 20.0f;
    public float effectIntensity = 0.2f;
    [SyncVar]
    public NetworkInstanceId playerNetId;
    [SyncVar]
    public int playerConnectionId;

    public NetworkInstanceId targetPlayerNetId;
    private GameObject targetPlayer;

    Rigidbody2D rigidBody;

    public override void OnStartServer () {
        rigidBody = GetComponent<Rigidbody2D> ();
        rigidBody.velocity = new Vector2 (transform.forward.x, transform.forward.y).normalized * projectileSpeed;

        GameObject player = NetworkServer.FindLocalObject (playerNetId);
        Physics2D.IgnoreCollision (GetComponent<Collider2D> (), player.GetComponent<Collider2D> ());

        if (player.tag.Equals ("Player")) {
            GetComponent<TrailRenderer> ().material.SetColor ("_TintColor", player.GetComponent<PlayerController> ().playerColor);
        }
    }

    public override void OnStartClient () {
        GameObject player = ClientScene.FindLocalObject (playerNetId);
        Physics2D.IgnoreCollision (GetComponent<Collider2D> (), player.GetComponent<Collider2D> ());

        if (player.tag.Equals ("Player")) {
            GetComponent<TrailRenderer> ().material.SetColor ("_TintColor", player.GetComponent<PlayerController> ().playerColor);
        }
    }

    // Use this for initialization
    void Start () {
        
    }

    void Awake () {
        
    }

    // Update is called once per frame
    void Update () {
        if (targetPlayer == null && !targetPlayerNetId.IsEmpty ()) {
            // Set homing target
            if (isServer) {
                targetPlayer = NetworkServer.FindLocalObject (targetPlayerNetId);
            } else if (isClient) {
                targetPlayer = ClientScene.FindLocalObject (targetPlayerNetId);
            }
        }

        if (targetPlayer != null) {
            rigidBody.velocity = (targetPlayer.transform.position - transform.position).normalized * projectileSpeed;
        }

        projectileLifetime -= Time.deltaTime;
        if (projectileLifetime <= 0.0f) {
            if (isClient) {
                ShakeCamerasInRange ();
            }
            if (isServer) {
                Destroy (gameObject);
            }
        }
    }

    void OnCollisionEnter2D (Collision2D collision) {
        if (collision.collider.tag.Equals ("Projectile")) { // Ignore projectile-to-projectile collisions
            Physics2D.IgnoreCollision (GetComponent<Collider2D> (), collision.collider);
        } else {
            if (isClient) { // Only simulate camera shake on client due to camera synchronization using Network Transform
                ShakeCamerasInRange ();
            }
            if (isServer) { // Only simulate collision event on server due to synchronization using SyncVar and server-side instantiation
                if (collision.collider.tag.Equals ("Player") || collision.collider.tag.Equals ("Enemy")) {
                    // Handle damage to player
                    HealthController playerHealthController = collision.gameObject.GetComponent<HealthController> ();
                    playerHealthController.ReduceHealth (projectileDamage, playerConnectionId);
                }
                // Destroy projectile
                Destroy (gameObject);
            }
        }
    }

    void OnDestroy () {
        if (!isServer) { // OnDestroy should only be invoked on the server
            return;
        }
        // Instantiate explosion prefab
        GameObject explosion = (GameObject) Instantiate (explosionPrefab, transform.position, transform.rotation);
        explosion.GetComponent<ExplosionController> ().playerConnectionId = playerConnectionId;
        NetworkServer.Spawn (explosion);
    }

    void ShakeCamerasInRange () {
        // Loop through all cameras on scene
        foreach (Camera currentCamera in Camera.allCameras) {
            if (currentCamera.tag.Equals ("MainCamera")) {
                Vector3 currentCameraPosition = currentCamera.transform.position;
                // Check if distance to currentCamera is within effect distance
                if (IsInEffectRange (new Vector2 (currentCameraPosition.x, currentCameraPosition.y))) {
                    currentCamera.GetComponent<CameraController> ().toggleShaking (effectIntensity);
                }
            }
        }
    }

    /*
	 * This method checks if a target position is within the specified effect range 
	 * from this object transform.
	 */
    bool IsInEffectRange (Vector2 targetPosition) {
        if (Vector2.Distance (new Vector2 (targetPosition.x, targetPosition.y),
                              new Vector2 (transform.position.x, transform.position.y)) < effectRange) {
            return true;
        }
        return false;
    }

}
