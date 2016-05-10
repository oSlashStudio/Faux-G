using UnityEngine;
using System.Collections;

abstract public class Attractor : MonoBehaviour {

	public abstract void Attract (Transform targetTransform);
	public abstract void Repel (Transform targetTransform);
	public abstract void Flip (Transform targetTransform);

}
