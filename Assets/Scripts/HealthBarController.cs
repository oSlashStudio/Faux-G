using UnityEngine;
using System.Collections;

public class HealthBarController : MonoBehaviour {

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
        spriteRenderer.color = Color.Lerp (Color.red, Color.green, currentHealth / maxHealth);
    }

}
