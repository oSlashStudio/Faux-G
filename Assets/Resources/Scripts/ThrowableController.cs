using UnityEngine;
using System.Collections;

public class ThrowableController : MonoBehaviour {

    public GameObject explosionPrefab;
    
    public float throwableLifetime;
    public bool isExplodingOnCollision;

    private float haloBlinkDelay;
    private bool isHaloEnabled;

    // Owner information variables
    private bool isPlayerInstantiated = false;
    private int instantiatorId;

    public int InstantiatorId {
        get {
            return instantiatorId;
        }
        set {
            isPlayerInstantiated = true;
            instantiatorId = value;
        }
    }

    // Cached components
    private Component halo;

    // Use this for initialization
    void Start () {
        halo = GetComponent ("Halo");
    }
	
	// Update is called once per frame
	void Update () {
        throwableLifetime -= Time.deltaTime;
        if (throwableLifetime <= 0.0f) {
            GameObject explosion = (GameObject) Instantiate (explosionPrefab, transform.position, Quaternion.identity);

            if (isPlayerInstantiated) {
                explosion.GetComponent<ExplosionController> ().InstantiatorId = instantiatorId;
            }

            Destroy (gameObject);
        }

        haloBlinkDelay -= Time.deltaTime;
        if (haloBlinkDelay <= 0.0f) {
            ToggleHalo ();
            haloBlinkDelay = throwableLifetime / 6.0f;
        }
	}

    void OnCollisionEnter2D (Collision2D collision) {
        if (!isExplodingOnCollision) { // Not exploding on collision, ignore collision event
            return;
        }

        GameObject explosion = (GameObject) Instantiate (explosionPrefab, transform.position, Quaternion.identity);

        if (isPlayerInstantiated) {
            explosion.GetComponent<ExplosionController> ().InstantiatorId = instantiatorId;
        }

        Destroy (gameObject);
    }

    void ToggleHalo () {
        isHaloEnabled = !isHaloEnabled;
        halo.GetType ().GetProperty ("enabled").SetValue (halo, isHaloEnabled, null);
    }

}
