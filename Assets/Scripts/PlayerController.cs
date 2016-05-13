using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerController : NetworkBehaviour {

	public Attractor attractor;
	public float moveSpeed = 15.0f;
	public float jumpForce = 250.0f;
    
	private Vector3 movementDirection;
	private Rigidbody2D rigidBody;
	private bool canMove = false; // Initially, player is spawned airborne, unable to move
	private bool canFlip = false; // Initially, player is spawned airborne, unable to flip
	private bool canJump = false; // Initially, player is spawned airborne, unable to jump

    public override void OnStartLocalPlayer () {
        Camera.main.GetComponent<CameraController> ().playerObject = gameObject;
        GetComponent<MeshRenderer> ().material.color = Color.red;
    }

	// Use this for initialization
	void Start () {
		rigidBody = GetComponent<Rigidbody2D> ();
    }
	
	// Update is called once per frame
	void Update () {
		if (canMove) {
            if (!isLocalPlayer) {
                return;
            }
            InputMove ();
            InputFlipGravity ();
            InputJump ();
		}
	}

	void FixedUpdate () {
		if (canMove) {
            if (!isLocalPlayer) {
                return;
            }
            Move ();
		}
	}

	void OnCollisionEnter2D (Collision2D collision) {
		canMove = true;
		canFlip = true;
		canJump = true;
	}

    void InputMove () {
        // Update movement direction based on currently pressed directional button
        movementDirection = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0.0f, 0.0f).normalized;
    }

    void Move () {
        Vector3 movementVector = transform.TransformDirection (movementDirection) * moveSpeed * Time.deltaTime;
        rigidBody.velocity += new Vector2 (movementVector.x, movementVector.y);
    }

	void InputFlipGravity () {
		if (Input.GetKeyDown (KeyCode.Space)) { // Flip gravity
			if (canFlip) {
				// Disable flip while flipping
				canFlip = false;
				attractor.Flip (transform);
			}
		}
	}

	void InputJump () {
		if (Input.GetKeyDown (KeyCode.W)) { // Jump
			if (canJump) {
				Jump ();
			}
		}
	}

	void Jump () {
		Vector3 jumpDirection = (attractor.transform.position - transform.position).normalized;
		// Disable jump while jumping
		canJump = false;
		rigidBody.AddForce (new Vector2 (jumpDirection.x, jumpDirection.y) * jumpForce);
	}

}
