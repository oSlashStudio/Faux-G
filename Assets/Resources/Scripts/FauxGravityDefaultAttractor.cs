using UnityEngine;
using System.Collections;

public class FauxGravityDefaultAttractor : Attractor {

	public override void Attract (Transform targetTransform, Rigidbody2D targetRigidbody) {
        // Intentional blank method, default attractor does nothing to fauxGravityBody
    }

    public override void Repel (Transform targetTransform, Rigidbody2D targetRigidbody) {
        // Intentional blank method, default attractor does nothing to fauxGravityBody
    }

}
