using UnityEngine;
using System.Collections;

public class SaberController : MonoBehaviour {

    public float slashDamage;
    public bool isArmorPiercing;
    [HideInInspector]
    public int instantiatorId;

    private float lifeTime;

	// Use this for initialization
	void Start () {
        lifeTime = 0.0f;
	}

    void Update () {
        lifeTime += Time.deltaTime;

        if (lifeTime <= 0.25f) {
            transform.RotateAround (transform.parent.position, Vector3.forward, 1440f * Time.deltaTime);
        } else if (lifeTime <= 0.5f) {
            transform.RotateAround (transform.parent.position, Vector3.forward, -1440f * Time.deltaTime);
        } else {
            Destroy (gameObject, 0.25f);
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
