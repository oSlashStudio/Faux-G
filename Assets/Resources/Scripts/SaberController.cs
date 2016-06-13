using UnityEngine;
using System.Collections;

public class SaberController : MonoBehaviour {

    public float slashDamage;
    public bool isArmorPiercing;
    [HideInInspector]
    public int instantiatorId;

	// Use this for initialization
	void Start () {
        Destroy (gameObject, 0.25f);
	}

    void Update () {
        transform.RotateAround (transform.parent.position, Vector3.forward, 1440.0f * Time.deltaTime);
    }

    void OnTriggerEnter2D (Collider2D collider) {
        GameObject collidingObject = collider.gameObject;
        HealthController collidingHealthController = collidingObject.GetComponent<HealthController> ();

        if (collidingHealthController != null) {
            collidingHealthController.Damage (slashDamage, collidingObject.transform.position, isArmorPiercing, instantiatorId);
        }
    }

}
