using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {

    public float minAngularVelocity;
    public float maxAngularVelocity;
    public float angularVelocityChangeRate;

    private bool isIncreasing;

    // Cached components
    private Rigidbody2D rigidBody;

	// Use this for initialization
	void Start () {
        rigidBody = GetComponent<Rigidbody2D> ();
        rigidBody.angularVelocity = minAngularVelocity;
        isIncreasing = true;
	}
	
	// Update is called once per frame
	void Update () {
	    if (isIncreasing) {
            rigidBody.angularVelocity += angularVelocityChangeRate * Time.deltaTime;
            if (rigidBody.angularVelocity >= maxAngularVelocity) {
                isIncreasing = false;
            }
        } else {
            rigidBody.angularVelocity -= angularVelocityChangeRate * Time.deltaTime;
            if (rigidBody.angularVelocity <= minAngularVelocity) {
                isIncreasing = true;
            }
        }
	}

}
