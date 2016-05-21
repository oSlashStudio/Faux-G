using UnityEngine;
using System.Collections;

public class FauxGravityCircularAttractor : Attractor {

	public float gravity = 9.8f;

    public override void Attract (Transform targetTransform) {
		Vector3 attractorNormal = (targetTransform.position - transform.position).normalized;
		Vector3 targetNormal = -targetTransform.up;

		// Force component of faux gravity
		targetTransform.GetComponent<Rigidbody2D>().AddForce (attractorNormal * gravity);

        if (!targetTransform.GetComponent<FauxGravityBody> ().isRotatable) {
            return;
        }

		// Rotation / Torque component of faux gravity
		Quaternion targetRotation = Quaternion.FromToRotation (targetNormal, attractorNormal) * targetTransform.rotation;
        // Keep track of old x and y-rotation
        Vector3 targetTransformOldRotation = targetTransform.rotation.eulerAngles;
        // Rotate transform according to gravity
        targetTransform.rotation = Quaternion.Slerp (targetTransform.rotation, targetRotation, 50 * Time.deltaTime);
        // Assert old x and y-rotation
        Vector3 targetTransformRotation = targetTransform.rotation.eulerAngles;
        targetTransform.rotation = Quaternion.Euler (targetTransformOldRotation.x, targetTransformOldRotation.y, targetTransformRotation.z);
	}

	public override void Repel (Transform targetTransform) {
		Vector3 repellerNormal = (targetTransform.position - transform.position).normalized;
		Vector3 targetNormal = targetTransform.up;
		
		// Force component of faux gravity
		targetTransform.GetComponent<Rigidbody2D>().AddForce (repellerNormal * -gravity);

        if (!targetTransform.GetComponent<FauxGravityBody> ().isRotatable) {
            return;
        }

        // Rotation / Torque component of faux gravity
        Quaternion targetRotation = Quaternion.FromToRotation (targetNormal, repellerNormal) * targetTransform.rotation;
        // Keep track of old x and y-rotation
        Vector3 targetTransformOldRotation = targetTransform.rotation.eulerAngles;
        // Rotate transform according to gravity
        targetTransform.rotation = Quaternion.Slerp (targetTransform.rotation, targetRotation, 50 * Time.deltaTime);
        // Assert old x and y-rotation
        Vector3 targetTransformRotation = targetTransform.rotation.eulerAngles;
        targetTransform.rotation = Quaternion.Euler (targetTransformOldRotation.x, targetTransformOldRotation.y, targetTransformRotation.z);
    }

    void OnTriggerEnter2D (Collider2D collider) {
        if (collider.tag.Equals ("Player")) {
            collider.gameObject.GetComponent<FauxGravityBody> ().attractor = GetComponent<Attractor> ();
            collider.gameObject.GetComponent<PlayerController> ().attractor = GetComponent<Attractor> ();
        } else if (collider.tag.Equals ("Attractable")) {
            collider.gameObject.GetComponent<FauxGravityBody> ().attractor = GetComponent<Attractor> ();
        }
    }

}
