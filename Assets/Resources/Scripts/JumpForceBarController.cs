using UnityEngine;
using System.Collections;

public class JumpForceBarController : MonoBehaviour {

    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Start () {
        playerController = GetComponentInParent<PlayerController> ();
        spriteRenderer = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
        float maxLeapForce = playerController.maxJumpForce;
        float leapForce = playerController.jumpForce;
        UpdateLeapForceBarScale (leapForce, maxLeapForce);
        UpdateLeapForceBarColor (leapForce, maxLeapForce);
	}

    void UpdateLeapForceBarScale (float leapForce, float maxLeapForce) {
        transform.localScale = new Vector3 (leapForce / maxLeapForce, transform.localScale.y, transform.localScale.z);
    }

    void UpdateLeapForceBarColor (float leapForce, float maxLeapForce) {
        spriteRenderer.color = Color.Lerp (new Color (1.0f, 1.0f, 0.0f), new Color (1.0f, 0.5f, 0.0f), leapForce / maxLeapForce);
    }

}
