using UnityEngine;
using System.Collections;

public class LeapForceBarController : MonoBehaviour {

    PlayerController playerController;

	// Use this for initialization
	void Start () {
        playerController = GetComponentInParent<PlayerController> ();
	}
	
	// Update is called once per frame
	void Update () {
        float maxLeapForce = playerController.maxLeapForce;
        float leapForce = playerController.leapForce;
        // Rescale leap force bar based on leapForce : maxLeapForce ratio
        transform.localScale = new Vector3 (leapForce / maxLeapForce, transform.localScale.y, transform.localScale.z);
	}
}
