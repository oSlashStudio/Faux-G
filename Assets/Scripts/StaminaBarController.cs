using UnityEngine;
using System.Collections;

public class StaminaBarController : MonoBehaviour {

    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;

    // Use this for initialization
    void Start () {
        playerController = GetComponentInParent<PlayerController> ();
        spriteRenderer = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
        float maxStamina = playerController.maxStamina;
        float currentStamina = playerController.currentStamina;
        UpdateStaminaBarScale (currentStamina, maxStamina);
        UpdateStaminaBarColor (currentStamina, maxStamina);
    }

    void UpdateStaminaBarScale (float currentStamina, float maxStamina) {
        transform.localScale = new Vector3 (currentStamina / maxStamina, transform.localScale.y, transform.localScale.z);
    }

    void UpdateStaminaBarColor (float currentStamina, float maxStamina) {
        spriteRenderer.color = Color.Lerp (Color.red, Color.cyan, currentStamina / maxStamina);
    }

}
