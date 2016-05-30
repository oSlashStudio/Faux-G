using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ExplosionController : MonoBehaviour {

    public AudioClip explosionSoundClip;

    // Explosion related variables
    public bool hasExplosionEffect;
    public float explosionArea;
    public float explosionHeal;
    public float explosionDamage;
    private float explosionDuration; // Explosion duration (directly taken from particle emitter duration)
    public float explosionDurationOffset; // The offset from explosion duration, useful for slowly fading emitter

    // Owner information variables
    private bool isPlayerInstantiated = false;
    private int instantiatorId;

    // Cached components
    private AudioSource audioSource;

    public int InstantiatorId {
        get {
            return instantiatorId;
        }
        set {
            isPlayerInstantiated = true;
            instantiatorId = value;
        }
    }

    // Use this for initialization
    void Start () {
        audioSource = GetComponent<AudioSource> ();
        if (explosionSoundClip != null) {
            audioSource.PlayOneShot (explosionSoundClip);
        }

        explosionDuration = GetComponent<ParticleSystem> ().duration;
        explosionDuration += explosionDurationOffset;
        if (hasExplosionEffect) {
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
                    targetHealthController.Heal (explosionHeal, instantiatorId, currentCollider.transform.position);
                    targetHealthController.Damage (explosionDamage, instantiatorId, currentCollider.transform.position);
                } else {
                    targetHealthController.Heal (explosionHeal, currentCollider.transform.position);
                    targetHealthController.Damage (explosionDamage, currentCollider.transform.position);
                }
            }
        }
    }

}
