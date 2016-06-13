using UnityEngine;
using System.Collections;

public class SaberController : MonoBehaviour {

    public float slashDamage;
    public bool isArmorPiercing;
    [HideInInspector]
    public int instantiatorId;

    private float lifetime;

	// Use this for initialization
	void Start () {
        lifetime = 0.0f;
	}

    void Update () {
        lifetime += Time.deltaTime;
        
        if (lifetime <= 0.25f) {
            float angularSpeed = Mathf.Lerp (0, 2880, lifetime / 0.25f);
            transform.RotateAround (transform.parent.position, Vector3.forward, angularSpeed * Time.deltaTime);
        } else if (lifetime <= 0.5f) {
            float angularSpeed = Mathf.Lerp (0, 2880, (lifetime - 0.25f) / 0.25f);
            transform.RotateAround (transform.parent.position, Vector3.forward, -angularSpeed * Time.deltaTime);
        } else {
            Destroy (gameObject);
        }
    }

    void OnCollisionEnter2D (Collision2D collision) {
        GameObject collidingObject = collision.gameObject;
        HealthController collidingHealthController = collidingObject.GetComponent<HealthController> ();

        if (collidingHealthController != null) {
            collidingHealthController.Damage (slashDamage, collision.contacts[0].point, isArmorPiercing, instantiatorId);
        }
    }

}
