using UnityEngine;
using System.Collections;

public class ExplosionController : MonoBehaviour {

	private float explosionDuration;

	// Use this for initialization
	void Start () {
		explosionDuration = GetComponent<ParticleSystem>().duration;
	}
	
	// Update is called once per frame
	void Update () {
		explosionDuration -= Time.deltaTime;
		if (explosionDuration <= 0.0f) {
			Destroy (gameObject);
		}
	}
}
