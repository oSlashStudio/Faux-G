using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ExplosionController : NetworkBehaviour {

    public bool hasExplosionDamage = false;
    public float explosionArea = 3.0f;
    public float explosionDamage = 1.0f;
    [SyncVar]
    public int playerConnectionId;

    private float explosionDuration;

	// Use this for initialization
	void Start () {
		explosionDuration = GetComponent<ParticleSystem>().duration;
        if (isServer) { // Only does explosion damage on server
            if (hasExplosionDamage) {
                DamagePlayersInArea ();
            }
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
            if (currentCollider.tag.Equals ("Player") || currentCollider.tag.Equals("Enemy")) {
                // Handle damage to player
                HealthController playerHealthController = currentCollider.gameObject.GetComponent<HealthController> ();
                playerHealthController.ReduceHealth (explosionDamage, playerConnectionId);
            }
        }
    }

}
