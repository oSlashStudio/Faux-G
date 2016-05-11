using UnityEngine;
using System.Collections;

public class DamageCalloutController : MonoBehaviour {

    public float maxLifeDuration = 1.0f;
    public float floatingSpeed = 1.0f;

    private float lifeDuration = 0.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        // Increment life duration based on time lapsed
        lifeDuration += Time.deltaTime;
        // Check life duration threshold on each frame
        if (lifeDuration >= maxLifeDuration) {
            Destroy (gameObject);
        }
        FloatUpwards ();
	}

    /*
     * This method moves the damage callout upwards by floatingSpeed unit and reduce the alpha component accordingly on each frame
     */
    void FloatUpwards () {
        GetComponent<Rigidbody2D> ().velocity = transform.up * floatingSpeed;
        GetComponent<TextMesh> ().color = new Color (1.0f, 1.0f, 1.0f, 1.0f - lifeDuration / maxLifeDuration);
    }
}
