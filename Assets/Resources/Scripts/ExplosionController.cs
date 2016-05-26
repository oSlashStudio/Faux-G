using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ExplosionController : MonoBehaviour {

    // Explosion related variables
    public bool hasExplosionDamage = false;
    public float explosionArea = 3.0f;
    public float explosionDamage = 1.0f;
    private float explosionDuration; // Explosion duration (directly taken from particle emitter duration)
    public float explosionDurationOffset; // The offset from explosion duration, useful for slowly fading emitter

    // Owner information variables
    private bool isPlayerInstantiated = false;
    private int instantiatorViewId;

    public int InstantiatorViewId {
        get {
            return instantiatorViewId;
        }
        set {
            isPlayerInstantiated = true;
            instantiatorViewId = value;
        }
    }

    // Use this for initialization
    void Start () {
        explosionDuration = GetComponent<ParticleSystem> ().duration;
        explosionDuration += explosionDurationOffset;
        if (hasExplosionDamage) {
            DamagePlayersInArea ();
        }
    }

    // Update is called once per frame
    void Update () {
        explosionDuration -= Time.deltaTime;
        if (explosionDuration <= 0.0f) {
            Destroy (gameObject);
        }
    }

    void DamagePlayersInArea () {
        Collider2D[] collidersInArea = Physics2D.OverlapCircleAll ((Vector2) transform.position, explosionArea);
        foreach (Collider2D currentCollider in collidersInArea) {
            HealthController targetHealthController = currentCollider.gameObject.GetComponent<HealthController> ();
            if (targetHealthController != null) { // If target has health component
                if (isPlayerInstantiated) {
                    targetHealthController.Damage (explosionDamage, instantiatorViewId);
                } else {
                    targetHealthController.Damage (explosionDamage);
                }
            }
        }
    }

}
