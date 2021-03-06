using UnityEngine;
using System.Collections;

public class HealthBarController : MonoBehaviour {

    // Cached components
    private HealthController healthController;
    private SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Start () {
        healthController = GetComponentInParent<HealthController> ();
        spriteRenderer = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
        float maxHealth = healthController.maxHealth;
        float currentHealth = healthController.currentHealth;
        UpdateHealthBarScale (currentHealth, maxHealth);
        UpdateHealthBarColor (currentHealth, maxHealth);
	}

    void UpdateHealthBarScale (float currentHealth, float maxHealth) {
        transform.localScale = new Vector3 (currentHealth / maxHealth, transform.localScale.y, transform.localScale.z);
    }

    void UpdateHealthBarColor (float currentHealth, float maxHealth) {
        if (currentHealth / maxHealth <= 0.5f) { // From 0 to 0.5, lerps from red to yellow
            spriteRenderer.color = Color.Lerp (Color.red, Color.yellow, currentHealth / maxHealth * 2.0f);
        } else { // From 0.5 to 1, lerps from yellow to green
            spriteRenderer.color = Color.Lerp (Color.yellow, Color.green, currentHealth / maxHealth * 2.0f - 1.0f);
        }
    }

}
