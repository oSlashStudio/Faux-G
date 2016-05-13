using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

	public GameObject explosionPrefab;
	public float bulletSpeed = 5.0f;
    public float bulletDamage = 10.0f;
	public float effectRange = 20.0f;
	public float effectIntensity = 0.2f;

	Rigidbody2D rigidBody;

	// Use this for initialization
	void Start () {
		rigidBody = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
		rigidBody.velocity += new Vector2 (transform.forward.x, transform.forward.y) * bulletSpeed;
	}

	void OnCollisionEnter2D (Collision2D collision) {
        if (collision.collider.tag.Equals ("Projectile")) { // Ignore projectile-to-projectile collisions
            Physics2D.IgnoreCollision (GetComponent<Collider2D> (), collision.collider);
        } else {
            if (collision.collider.tag.Equals ("Player")) {
                // Handle damage to player
                HealthController playerHealthController = collision.gameObject.GetComponent<HealthController> ();
                playerHealthController.ReduceHealth (bulletDamage);
            }
            foreach (Camera currentCamera in Camera.allCameras) { // Loop through all cameras on scene
                Vector3 currentCameraPosition = currentCamera.transform.position;
                // Check if distance to currentCamera is within effect distance
                if (IsInEffectRange (new Vector2 (currentCameraPosition.x, currentCameraPosition.y))) {
                    currentCamera.GetComponent<CameraController> ().toggleShaking (effectIntensity);
                }
            }
            // Instantiate explosion prefab
            Instantiate (explosionPrefab, transform.position, transform.rotation);
            // Finally, destroy this game object
            Destroy (gameObject);
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
