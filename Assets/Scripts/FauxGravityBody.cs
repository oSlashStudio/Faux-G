using UnityEngine;
using System.Collections;

public class FauxGravityBody : MonoBehaviour {

	public Attractor attractor;
	public bool isAttracted = true;
    public bool isRotatable = true;

	private Rigidbody2D rigidBody;

	// Use this for initialization
	void Start () {
		rigidBody = GetComponent<Rigidbody2D>();

        if (isRotatable) {
            rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

		rigidBody.gravityScale = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {

	}

	void FixedUpdate () {
		if (isAttracted) {
			attractor.Attract (transform);
		} else {
			attractor.Repel (transform);
		}
	}

	public void ToggleAttraction () {
		isAttracted = !isAttracted;
	}

}
