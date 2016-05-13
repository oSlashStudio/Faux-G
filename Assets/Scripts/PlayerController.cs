using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerController : NetworkBehaviour {

	public Attractor attractor;
    public GameObject crosshairPrefab;
    public GameObject weaponPrefab;
	public float moveSpeed = 15.0f;
	public float jumpForce = 250.0f;
    
	private Vector3 movementDirection;
	private Rigidbody2D rigidBody;
	private bool canMove = false; // Initially, player is spawned airborne, unable to move
	private bool canFlip = false; // Initially, player is spawned airborne, unable to flip
	private bool canJump = false; // Initially, player is spawned airborne, unable to jump

    private GameObject crosshair;
    private CrosshairController crosshairController;
    public GameObject weapon;
    public WeaponController weaponController;

    public override void OnStartLocalPlayer () {
        Camera.main.GetComponent<CameraController> ().playerObject = gameObject;
        GetComponent<MeshRenderer> ().material.color = Color.red;

        if (isLocalPlayer) {
            // Instantiate crosshair locally
            crosshair = (GameObject) Instantiate (crosshairPrefab, transform.position, Quaternion.identity);
            crosshairController = crosshair.GetComponent<CrosshairController> ();

            // Instantiate weapon
            CmdInstantiateWeapon ();
        }
    }

	// Use this for initialization
	void Start () {
		rigidBody = GetComponent<Rigidbody2D> ();
    }
	
	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer) {
            return;
        }
        UpdateCrosshairPosition ();
        UpdateWeaponDirection ();
        if (canMove) {
            InputFire ();
            InputChangeWeapon ();
            InputMove ();
            InputFlipGravity ();
            InputJump ();
		}
	}

	void FixedUpdate () {
        if (!isLocalPlayer) {
            return;
        }
        if (canMove) {
            Move ();
		}
	}

	void OnCollisionEnter2D (Collision2D collision) {
		canMove = true;
		canFlip = true;
		canJump = true;
	}

    [Command]
    void CmdInstantiateWeapon () {
        weapon = (GameObject) Instantiate (weaponPrefab, transform.position + transform.up * 0.3f, Quaternion.identity);
        // Initialize weapon on server
        weapon.transform.parent = transform;
        GetComponent<PlayerController> ().weapon = weapon;
        GetComponent<PlayerController> ().weaponController = weapon.GetComponent<WeaponController> ();

        NetworkServer.SpawnWithClientAuthority (weapon, connectionToClient);
        RpcInitializeWeapon (weapon, gameObject);
    }

    [ClientRpc]
    void RpcInitializeWeapon (GameObject weapon, GameObject player) {
        // Initialize weapon on client
        weapon.transform.parent = player.transform;
        player.GetComponent<PlayerController> ().weapon = weapon;
        player.GetComponent<PlayerController> ().weaponController = weapon.GetComponent<WeaponController> ();
    }

    void UpdateCrosshairPosition () {
        Vector2 mousePosition = (Vector2) Camera.main.ScreenToWorldPoint (Input.mousePosition);
        // Calculate direction vector
        Vector2 moveDirectionVector = mousePosition - (Vector2) crosshair.transform.position;
        // Calculate target position vector
        Vector2 targetPosition = (Vector2) crosshair.transform.position + moveDirectionVector * crosshairController.moveSpeed * Time.deltaTime;
        // Move crosshair towards target position
        crosshairController.MoveTowards (targetPosition);
    }

    void UpdateWeaponDirection () {
        if (weaponController == null) {
            return;
        }
        weaponController.UpdateWeaponDirection (crosshair.transform.position);
    }

    void InputFire () {
        if (Input.GetMouseButton (0)) { // Fire current weapon
            weaponController.CmdFire (crosshair.transform.position);
        }
    }

    void InputChangeWeapon () {
        if (Input.GetKeyDown (KeyCode.Alpha1)) {
            weaponController.CmdChangeWeapon (1);
        } else if (Input.GetKeyDown (KeyCode.Alpha2)) {
            weaponController.CmdChangeWeapon (2);
        } else if (Input.GetKeyDown (KeyCode.Alpha3)) {
            weaponController.CmdChangeWeapon (3);
        }
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
