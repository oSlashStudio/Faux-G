using UnityEngine;
using System.Collections;

public class LeapForceBarController : MonoBehaviour {

    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Start () {
        playerController = GetComponentInParent<PlayerController> ();
        spriteRenderer = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
        float maxLeapForce = playerController.maxLeapForce;
        float leapForce = playerController.leapForce;
        UpdateLeapForceBarScale (leapForce, maxLeapForce);
        UpdateLeapForceBarColor (leapForce, maxLeapForce);
	}

    void UpdateLeapForceBarScale (float leapForce, float maxLeapForce) {
        transform.localScale = new Vector3 (leapForce / maxLeapForce, transform.localScale.y, transform.localScale.z);
    }

    void UpdateLeapForceBarColor (float leapForce, float maxLeapForce) {
        spriteRenderer.color = Color.Lerp (Color.yellow, Color.white, leapForce / maxLeapForce);
    }

}
