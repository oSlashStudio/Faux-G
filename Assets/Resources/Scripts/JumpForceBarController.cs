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
        float maxJumpForce = playerController.maxJumpForce;
        float jumpForce = playerController.jumpForce;
        UpdateJumpForceBarScale (jumpForce, maxJumpForce);
        UpdateJumpForceBarColor (jumpForce, maxJumpForce);
	}

    void UpdateJumpForceBarScale (float jumpForce, float maxJumpForce) {
        transform.localScale = new Vector3 (jumpForce / maxJumpForce, transform.localScale.y, transform.localScale.z);
    }

    void UpdateJumpForceBarColor (float jumpForce, float maxJumpForce) {
        spriteRenderer.color = Color.Lerp (new Color (1.0f, 1.0f, 0.0f), new Color (1.0f, 0.5f, 0.0f), jumpForce / maxJumpForce);
    }

}
