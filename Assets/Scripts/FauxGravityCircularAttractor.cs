using UnityEngine;
using System.Collections;

public class FauxGravityCircularAttractor : Attractor {

	public float gravity = 9.8f;
	public float flipForce = 750.0f;

	public override void Attract (Transform targetTransform) {
		Vector3 attractorNormal = (targetTransform.position - transform.position).normalized;
		Vector3 targetNormal = -targetTransform.up;

		// Force component of faux gravity
		targetTransform.GetComponent<Rigidbody2D>().AddForce (attractorNormal * gravity);

		// Rotation / Torque component of faux gravity
		Quaternion targetRotation = Quaternion.FromToRotation (targetNormal, attractorNormal) * targetTransform.rotation;
		targetTransform.rotation = Quaternion.Slerp (targetTransform.rotation, targetRotation, 50 * Time.deltaTime);
	}

	public override void Repel (Transform targetTransform) {
		Vector3 repellerNormal = (targetTransform.position - transform.position).normalized;
		Vector3 targetNormal = targetTransform.up;
		
		// Force component of faux gravity
		targetTransform.GetComponent<Rigidbody2D>().AddForce (repellerNormal * -gravity);
		
		// Rotation / Torque component of faux gravity
		Quaternion targetRotation = Quaternion.FromToRotation (targetNormal, repellerNormal) * targetTransform.rotation;
		targetTransform.rotation = Quaternion.Slerp (targetTransform.rotation, targetRotation, 50 * Time.deltaTime);
	}

	public override void Flip (Transform targetTransform) {
		Vector3 flipDirection = (transform.position - targetTransform.transform.position).normalized;

		Rigidbody2D targetRigidBody = targetTransform.gameObject.GetComponent<Rigidbody2D>();
		// Disable any velocity acting on the player
		targetRigidBody.velocity = Vector2.zero;

		targetRigidBody.AddForce (new Vector2 (flipDirection.x, flipDirection.y) * flipForce);
	}

}
