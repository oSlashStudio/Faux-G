using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerController : NetworkBehaviour {

	public Attractor attractor;
    public GameObject crosshairPrefab;
    public GameObject weaponPrefab;
    public GameObject cameraPrefab;
	public float moveSpeed = 15.0f;
	public float jumpForce = 350.0f;
    public float leapForce = 850.0f;
    public float defaultLeapDelay = 5.0f;
    
	private Vector3 movementDirection;
	private Rigidbody2D rigidBody;
	private bool canMove = false; // Initially, player is spawned airborne, unable to move
	private bool canLeap = false; // Initially, player is spawned airborne, unable to flip
	private bool canJump = false; // Initially, player is spawned airborne, unable to jump
    // Accessed by LeapDelayBarController
    public float leapDelay = 0.0f; // Initial leap delay is 0, player can instantly leap after touching ground

    public GameObject crosshair;
    private CrosshairController crosshairController;
    public GameObject weapon;
    public WeaponController weaponController;
    public GameObject mainCamera;

    public override void OnStartLocalPlayer () {
        GetComponent<MeshRenderer> ().material.color = Color.red;

        // Instantiate camera
        CmdInstantiateCamera ();

        // Instantiate crosshair locally
        crosshair = (GameObject) Instantiate (crosshairPrefab, transform.position, Quaternion.identity);
        crosshairController = crosshair.GetComponent<CrosshairController> ();

        // Instantiate weapon
        CmdInstantiateWeapon ();
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
        UpdateLeapDelay ();
        CmdUpdateWeaponPosition ();
        CmdUpdateWeaponDirection (crosshair.transform.position);
        if (canMove) {
            InputFire ();
            InputChangeWeapon ();
            InputMove ();
            InputLeap ();
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
		canLeap = true;
		canJump = true;
	}

    void OnDestroy () {
        Destroy (crosshair);
        Destroy (mainCamera);
    }

    [Command]
    void CmdInstantiateCamera () {
        GameObject camera = (GameObject) Instantiate (cameraPrefab, transform.position, Quaternion.identity);
        camera.GetComponent<CameraController> ().playerObject = gameObject;

        camera.GetComponent<CameraController> ().playerNetId = GetComponent<NetworkIdentity> ().netId;
        NetworkServer.SpawnWithClientAuthority (camera, connectionToClient);
        RpcInitializeCamera (camera, gameObject);
    }

    [ClientRpc]
    void RpcInitializeCamera (GameObject camera, GameObject player) {
        if (hasAuthority) {
            camera.GetComponent<Camera> ().enabled = true;
            camera.GetComponent<AudioListener> ().enabled = true;
        }
    }
    
    [Command]
    void CmdInstantiateWeapon () {
        weapon = (GameObject) Instantiate (weaponPrefab, transform.position + transform.up * 0.3f, Quaternion.identity);
        // Initialize weapon on server
        weapon.transform.parent = transform;
        weapon.transform.localPosition = transform.up * 0.3f;
        GetComponent<PlayerController> ().weapon = weapon;
        GetComponent<PlayerController> ().weaponController = weapon.GetComponent<WeaponController> ();

        weapon.GetComponent<WeaponController> ().playerNetId = GetComponent<NetworkIdentity> ().netId;
        NetworkServer.SpawnWithClientAuthority (weapon, connectionToClient);
    }

    [Command]
    void CmdUpdateWeaponPosition () {
        if (weaponController == null) {
            return;
        }
        weaponController.UpdateWeaponPosition ();
        RpcUpdateWeaponPosition ();
    }

    [ClientRpc]
    void RpcUpdateWeaponPosition () {
        if (weaponController == null) {
            return;
        }
        weaponController.UpdateWeaponPosition ();
    }

    [Command]
    void CmdUpdateWeaponDirection (Vector3 crosshairPosition) {
        if (weaponController == null) {
            return;
        }
        weaponController.UpdateWeaponDirection (crosshairPosition);
        RpcUpdateWeaponDirection (crosshairPosition);
    }

    [ClientRpc]
    void RpcUpdateWeaponDirection (Vector3 crosshairPosition) {
        if (weaponController == null) {
            return;
        }
        weaponController.UpdateWeaponDirection (crosshairPosition);
    }

    void InputFire () {
        if (Input.GetMouseButton (0)) { // Fire current weapon
            weaponController.CmdFire (weapon.transform.FindChild ("Weapon Muzzle").position, crosshair.transform.position, crosshairController.accuracy);
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

    void UpdateLeapDelay () {
        if (leapDelay < 0.0f) {
            leapDelay = 0.0f;
        } else if (leapDelay > 0.0f) {
            leapDelay -= Time.deltaTime;
        }
    }

	void InputLeap () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			if (canLeap && leapDelay <= 0.0f) {
                Leap ();
			}
		}
	}

    void Leap () {
        // Disable jump and leap while leaping
        canJump = false;
        canLeap = false;
        // Introduce leap delay
        leapDelay = defaultLeapDelay;
        rigidBody.AddForce (transform.up * leapForce);
    }

	void InputJump () {
		if (Input.GetKeyDown (KeyCode.W)) {
			if (canJump) {
				Jump ();
			}
		}
	}

	void Jump () {
		// Disable jump and leap while jumping
		canJump = false;
        canLeap = false;
		rigidBody.AddForce (transform.up * jumpForce);
	}

}
