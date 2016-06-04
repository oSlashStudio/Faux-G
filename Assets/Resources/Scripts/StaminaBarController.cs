using UnityEngine;
using System.Collections;

public class StaminaBarController : MonoBehaviour {

    private StaminaController staminaController;
    private SpriteRenderer spriteRenderer;

    // Use this for initialization
    void Start () {
        staminaController = GetComponentInParent<StaminaController> ();
        spriteRenderer = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
        float maxStamina = staminaController.maxStamina;
        float currentStamina = staminaController.currentStamina;
        UpdateStaminaBarScale (currentStamina, maxStamina);
        UpdateStaminaBarColor (currentStamina, maxStamina);
    }

    void UpdateStaminaBarScale (float currentStamina, float maxStamina) {
        transform.localScale = new Vector3 (currentStamina / maxStamina, transform.localScale.y, transform.localScale.z);
    }

    void UpdateStaminaBarColor (float currentStamina, float maxStamina) {
        spriteRenderer.color = Color.Lerp (Color.gray, Color.white, currentStamina / maxStamina);
    }

}
