using UnityEngine;
using System.Collections;

public class StaminaBarController : MonoBehaviour {

    PlayerController playerController;

    // Use this for initialization
    void Start () {
        playerController = GetComponentInParent<PlayerController> ();
	}
	
	// Update is called once per frame
	void Update () {
        float maxStamina = playerController.maxStamina;
        float currentStamina = playerController.currentStamina;
        // Rescale stamina bar based on currentStamina : maxStamina ratio
        transform.localScale = new Vector3 (currentStamina / maxStamina, transform.localScale.y, transform.localScale.z);
    }
}
