using UnityEngine;
using System.Collections;

public class HealthBarController : MonoBehaviour {

    public float maxHealth = 1000.0f;
    public float currentHealth;

	// Use this for initialization
	void Start () {
        currentHealth = maxHealth;
	}
	
	// Update is called once per frame
	void Update () {
        // Check health threshold on every frame
        if (currentHealth <= 0.0f) {
            Destroy (transform.parent.gameObject);
        }
        // Rescale the healthbar based on current health percentage
        transform.localScale = new Vector3 (currentHealth / maxHealth, transform.localScale.y, transform.localScale.z);
	}

    public void increaseHealth (float healAmount) {
        currentHealth += healAmount;
    }

    public void reduceHealth (float damageAmount) {
        currentHealth -= damageAmount;
    }

}
