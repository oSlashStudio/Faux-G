using UnityEngine;
using System.Collections;

public class FauxGravityLinearAttractor : Attractor {

	public float gravity = 9.8f;
    public bool isOutwards = true;
	
	public override void Attract (Transform targetTransform, Rigidbody2D targetRigidbody) {
        // Calculate attractor normal using dot product - IMPROVE IF POSSIBLE (hefty calculations)
        // http://stackoverflow.com/questions/5227373/minimal-perpendicular-vector-between-a-point-and-a-line
        Vector3 targetToCenterOfGravityVector = targetTransform.position - transform.position;
        Vector3 directionVector = transform.right;
        Vector3 attractorPositionVector = transform.position +
            Vector2.Dot (targetToCenterOfGravityVector, directionVector) * directionVector;
        Vector3 attractorNormal = (targetTransform.position - attractorPositionVector).normalized;

        // Force component of faux gravity
        targetRigidbody.AddForce (attractorNormal * gravity * targetRigidbody.mass); // F = ma

        if (!targetTransform.GetComponent<FauxGravityBody> ().isRotatable) {
            return;
        }

        // Rotation / Torque component of faux gravity
        Quaternion targetRotation = Quaternion.LookRotation (Vector3.forward, -attractorNormal);
		targetTransform.rotation = Quaternion.Slerp (targetTransform.rotation, targetRotation, 50 * Time.deltaTime);
	}
	
	public override void Repel (Transform targetTransform, Rigidbody2D targetRigidbody) {
		// Calculate repeller normal using dot product - IMPROVE IF POSSIBLE (hefty calculations)
		// http://stackoverflow.com/questions/5227373/minimal-perpendicular-vector-between-a-point-and-a-line
		Vector3 targetToCenterOfGravityVector = targetTransform.position - transform.position;
		Vector3 directionVector = transform.right;
		Vector3 repellerPositionVector = transform.position + 
			Vector2.Dot (targetToCenterOfGravityVector, directionVector) * directionVector;
		Vector3 repellerNormal = (targetTransform.position - repellerPositionVector).normalized;

        // Force component of faux gravity
        targetRigidbody.AddForce (repellerNormal * -gravity * targetRigidbody.mass); // F = ma

        if (!targetTransform.GetComponent<FauxGravityBody> ().isRotatable) {
            return;
        }

        // Rotation / Torque component of faux gravity
        Quaternion targetRotation = Quaternion.LookRotation (Vector3.forward, -repellerNormal);
        targetTransform.rotation = Quaternion.Slerp (targetTransform.rotation, targetRotation, 50 * Time.deltaTime);
	}

    void OnTriggerEnter2D (Collider2D collider) {
        if (collider.gameObject.GetComponent<FauxGravityBody> () == null) {
            return;
        }

        collider.gameObject.GetComponent<FauxGravityBody> ().attractor = GetComponent<Attractor> ();
        collider.gameObject.GetComponent<FauxGravityBody> ().isAttracted = isOutwards;
    }

}
