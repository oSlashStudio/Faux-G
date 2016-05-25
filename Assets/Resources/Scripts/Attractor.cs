using UnityEngine;
using System.Collections;

abstract public class Attractor : MonoBehaviour {

	public abstract void Attract (Transform targetTransform, Rigidbody2D targetRigidbody);
	public abstract void Repel (Transform targetTransform, Rigidbody2D targetRigidbody);

}
