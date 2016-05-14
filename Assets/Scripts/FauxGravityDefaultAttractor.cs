using UnityEngine;
using System.Collections;

public class FauxGravityDefaultAttractor : Attractor {

	public override void Attract (Transform targetTransform) {
        // Intentional blank method, default attractor does nothing to fauxGravityBody
    }

    public override void Repel (Transform targetTransform) {
        // Intentional blank method, default attractor does nothing to fauxGravityBody
    }

}
