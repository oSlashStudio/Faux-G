using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public static float DEFAULT_SHAKE_INTENSITY = 0.5f;
	public static float DEFAULT_SHAKE_THRESHOLD = 0.1f;

	public GameObject playerObject;
	public bool isShaking = false;
	public float intensity = DEFAULT_SHAKE_INTENSITY;
	public float dampingFactor = 0.975f;

	// Use this for initialization
	void Start () {
		transform.position = playerObject.transform.position + new Vector3 (0.0f, 0.0f, -10.0f);
	}

	// Update is called once per frame
	void Update () {
		if (isShaking) {
			Shake ();
		} else {
			transform.position = playerObject.transform.position + new Vector3 (0.0f, 0.0f, -10.0f);
		}
	}

	public void toggleShaking () {
		intensity = DEFAULT_SHAKE_INTENSITY;
		isShaking = true;
	}

	public void toggleShaking (float shakeIntensity) {
		intensity = shakeIntensity;
		isShaking = true;
	}

	void Shake () {
		Vector2 newPosition = Random.insideUnitCircle * intensity;
		transform.position = new Vector3 (playerObject.transform.position.x + newPosition.x, 
		                                  playerObject.transform.position.y + newPosition.y, 
		                                  transform.position.z);
		// Reduce intensity based on exponential damping
		intensity = intensity * dampingFactor;
		if (intensity <= DEFAULT_SHAKE_THRESHOLD) {
			isShaking = false;
		}
	}

}
