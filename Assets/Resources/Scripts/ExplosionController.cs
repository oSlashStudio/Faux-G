using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class ExplosionController : MonoBehaviour {

    public AudioClip explosionSoundClip;

    // Explosion related variables
    public bool hasExplosionEffect;
    public float explosionArea;
    public float explosionHeal;
    public float explosionDamage;
    public bool isArmorPiercing;
    private float explosionDuration; // Explosion duration (directly taken from particle emitter duration)
    public float explosionDurationOffset; // The offset from explosion duration, useful for slowly fading emitter
    public float explosionForce;

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
            GameObject targetGameObject = currentCollider.gameObject;
            
            RaycastHit2D hitInfo = Physics2D.Raycast (transform.position, targetGameObject.transform.position - transform.position);
            if (hitInfo.transform.gameObject != targetGameObject) { // Target is shielded, ignore explosion effect
                continue;
            }

            HealthController targetHealthController = targetGameObject.GetComponent<HealthController> ();
            if (targetHealthController != null) { // If target has health component
                if (isPlayerInstantiated) {
                    targetHealthController.Heal (explosionHeal, currentCollider.transform.position, instantiatorId);
                    targetHealthController.Damage (explosionDamage, currentCollider.transform.position, isArmorPiercing, instantiatorId);
                } else {
                    targetHealthController.Heal (explosionHeal, currentCollider.transform.position);
                    targetHealthController.Damage (explosionDamage, currentCollider.transform.position, isArmorPiercing);
                }
            }
            if (targetGameObject.tag != "Projectile") { // Projectiles are not affected by explosion force
                Rigidbody2D targetRigidbody = targetGameObject.GetComponent<Rigidbody2D> ();
                if (targetRigidbody != null) {
                    Vector2 direction = (targetRigidbody.position - (Vector2) transform.position).normalized;
                    targetRigidbody.AddForce (direction * explosionForce, ForceMode2D.Impulse);
                }
            }
        }
    }

}
