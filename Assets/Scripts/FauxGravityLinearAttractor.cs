using UnityEngine;
using System.Collections;

public class FauxGravityLinearAttractor : Attractor {

	public float gravity = 9.8f;
	public float flipForce = 750.0f;
	
	public override void Attract (Transform targetTransform) {
		// Calculate attractor normal using dot product - IMPROVE IF POSSIBLE (hefty calculations)
		// http://stackoverflow.com/questions/5227373/minimal-perpendicular-vector-between-a-point-and-a-line
		Vector2 targetToCenterOfGravityVector = targetTransform.position - transform.position;
		Vector2 directionVector = transform.right;
		Vector2 attractorPositionVector = (Vector2) transform.position + 
			Vector2.Dot (targetToCenterOfGravityVector, directionVector) * directionVector;
		Vector2 attractorNormal = ((Vector2) targetTransform.position - attractorPositionVector).normalized;

		Vector3 targetNormal = -targetTransform.up;
		
		// Force component of faux gravity
		targetTransform.GetComponent<Rigidbody2D>().AddForce (attractorNormal * gravity);
		
		// Rotation / Torque component of faux gravity
		Quaternion targetRotation = Quaternion.FromToRotation (targetNormal, attractorNormal) * targetTransform.rotation;
		targetTransform.rotation = Quaternion.Slerp (targetTransform.rotation, targetRotation, 50 * Time.deltaTime);
	}
	
	public override void Repel (Transform targetTransform) {
		// Calculate repeller normal using dot product - IMPROVE IF POSSIBLE (hefty calculations)
		// http://stackoverflow.com/questions/5227373/minimal-perpendicular-vector-between-a-point-and-a-line
		Vector2 targetToCenterOfGravityVector = targetTransform.position - transform.position;
		Vector2 directionVector = transform.right;
		Vector2 repellerPositionVector = (Vector2) transform.position + 
			Vector2.Dot (targetToCenterOfGravityVector, directionVector) * directionVector;
		Vector2 repellerNormal = ((Vector2) targetTransform.position - repellerPositionVector).normalized;

		Vector3 targetNormal = targetTransform.up;
		
		// Force component of faux gravity
		targetTransform.GetComponent<Rigidbody2D>().AddForce (repellerNormal * -gravity);
		
		// Rotation / Torque component of faux gravity
		Quaternion targetRotation = Quaternion.FromToRotation (targetNormal, repellerNormal) * targetTransform.rotation;
		targetTransform.rotation = Quaternion.Slerp (targetTransform.rotation, targetRotation, 50 * Time.deltaTime);
	}

	public override void Flip (Transform targetTransform) {
		// Calculate flip direction using dot product - IMPROVE IF POSSIBLE (hefty calculations)
		// http://stackoverflow.com/questions/5227373/minimal-perpendicular-vector-between-a-point-and-a-line
		Vector2 targetToCenterOfGravityVector = targetTransform.position - transform.position;
		Vector2 directionVector = transform.right;
		Vector2 attractorPositionVector = (Vector2) transform.position + 
			Vector2.Dot (targetToCenterOfGravityVector, directionVector) * directionVector;
		Vector2 flipDirection = (attractorPositionVector - (Vector2) targetTransform.position).normalized;

		Rigidbody2D targetRigidBody = targetTransform.gameObject.GetComponent<Rigidbody2D>();
		// Disable any velocity acting on the player
		targetRigidBody.velocity = Vector2.zero;

		targetRigidBody.AddForce (flipDirection * flipForce);
	}

}
