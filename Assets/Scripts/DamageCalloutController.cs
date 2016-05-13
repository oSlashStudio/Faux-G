using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class DamageCalloutController : NetworkBehaviour {

    public float maxLifeDuration = 2.0f;
    public float floatingSpeed = 1.0f;

    [SyncVar]
    public string text;
    
    private float lifeDuration = 0.0f;

    public override void OnStartClient () {
        GetComponent<Rigidbody2D> ().GetComponent<TextMesh> ().text = text;
    }

    public override void OnStartServer () {
        GetComponent<Rigidbody2D> ().GetComponent<TextMesh> ().text = text;
        GetComponent<Rigidbody2D> ().velocity = transform.up * floatingSpeed;
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (isServer) {
            // Increment life duration based on time lapsed
            lifeDuration += Time.deltaTime;
            // Check life duration threshold on each frame
            if (lifeDuration >= maxLifeDuration) {
                Destroy (gameObject);
            }
        }
        // Update color alpha on both client and server
        UpdateAlpha ();
	}

    /*
     * This method moves the damage callout upwards by floatingSpeed unit and reduce the alpha component accordingly on each frame
     */
    void UpdateAlpha () {
        GetComponent<TextMesh> ().color = new Color (1.0f, 1.0f, 1.0f, 1.0f - lifeDuration / maxLifeDuration);
    }
}
