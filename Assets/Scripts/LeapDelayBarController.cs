using UnityEngine;
using System.Collections;

public class LeapDelayBarController : MonoBehaviour {

    PlayerController playerController;

	// Use this for initialization
	void Start () {
        playerController = GetComponentInParent<PlayerController> ();
	}
	
	// Update is called once per frame
	void Update () {
        float defaultLeapDelay = playerController.defaultLeapDelay;
        float leapDelay = playerController.leapDelay;
        if (leapDelay == 0.0f) {
            transform.localScale = new Vector3 (0.0f, transform.localScale.y, transform.localScale.z);
        } else {
            transform.localScale = new Vector3 (1.0f - leapDelay / defaultLeapDelay, transform.localScale.y, transform.localScale.z);
        }
	}
}
