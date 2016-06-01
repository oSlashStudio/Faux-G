using UnityEngine;
using System.Collections;

public class RotatingLaserController : Photon.MonoBehaviour {

    public GameObject hitEffect;

    public float angularVelocity;
    public float laserDamage;

    // Cached components
    private LineRenderer lineRenderer;

	// Use this for initialization
	void Start () {
        lineRenderer = GetComponent<LineRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
        
    }

    void FixedUpdate () {
        if (photonView.isMine) { // Only master client modifies rotation
            transform.rotation = Quaternion.Euler (
                transform.rotation.eulerAngles + new Vector3 (0.0f, 0.0f, angularVelocity * Time.deltaTime)
                );
        }
        RaycastHit2D raycastHit = Physics2D.Raycast (transform.position, transform.rotation * Vector3.up);
        lineRenderer.SetPosition (1, new Vector3 (raycastHit.point.x, raycastHit.point.y, 0.0f));
        Instantiate (hitEffect, raycastHit.point, transform.rotation);

        // Handle damage
        if (raycastHit.rigidbody == null) { // Not a target
            return;
        }
        if (raycastHit.rigidbody.gameObject.tag == "Enemy") { // Ignore damage to enemy
            return;
        }
        HealthController targetHealthController = raycastHit.rigidbody.gameObject.GetComponent<HealthController> ();
        if (targetHealthController == null) {
            return;
        }
        targetHealthController.Damage (laserDamage, raycastHit.point);
    }

}
