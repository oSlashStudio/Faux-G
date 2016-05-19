using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerController : NetworkBehaviour {

	public Attractor attractor;
    public GameObject crosshairPrefab;
    public GameObject weaponPrefab;
    public GameObject cameraPrefab;
    public GameObject aimCameraPrefab;

	public float moveSpeed = 15.0f;
    public float maxLeapForce = 1500.0f;
    public float leapForce = 0.0f;
    public float leapForceChargeRate = 1000.0f; // Leap force increase per second
    public float staminaPerLeapForce = 0.05f; // Amount of stamina consumed per leap force unit

    public float maxStamina = 100.0f;
    public float currentStamina = 100.0f;
    public float staminaRecoveryRate = 10.0f; // Amount of stamina recovered per second
    
	private Vector3 movementDirection;
	private Rigidbody2D rigidBody;
	private bool canMove = false; // Initially, player is spawned airborne, unable to move
	private bool canLeap = false; // Initially, player is spawned airborne, unable to flip

    public GameObject crosshair;
    public CrosshairController crosshairController;
    public GameObject weapon;
    public WeaponController weaponController;
    public GameObject mainCamera;
    public CameraController cameraController;
    public GameObject aimCamera;
    public AimCameraController aimCameraController;

    // Attributes required for managing on respawn spectate mode
    public bool isDead = false; // Initially, player is not dead
    private int targetPlayerId; // Index of spectated target on players array
    private GameObject[] players;

    private bool isAiming;

    public override void OnStartLocalPlayer () {
        GetComponent<MeshRenderer> ().material.color = Color.red;

        // Instantiate crosshair locally
        crosshair = (GameObject) Instantiate (crosshairPrefab, transform.position, Quaternion.identity);
        crosshairController = crosshair.GetComponent<CrosshairController> ();

        // Instantiate camera
        CmdInstantiateCamera ();

        // Instantiate aim camera
        aimCamera = (GameObject) Instantiate (aimCameraPrefab, transform.position, Quaternion.identity);
        aimCameraController = aimCamera.GetComponent<AimCameraController> ();
        aimCameraController.crosshair = crosshair;

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
        if (isDead) {
            InputCycleSpectateTarget ();
            return;
        }

        CmdUpdateWeaponDirection (crosshair.transform.position);

        RecoverStamina ();

        InputAim ();
        InputFire ();
        InputChangeWeapon ();
        InputMove ();
        InputLeap ();
	}

	void FixedUpdate () {
        if (!isLocalPlayer || isDead) {
            return;
        }
        if (canMove && !isAiming) {
            Move ();
		}
	}

	void OnCollisionEnter2D (Collision2D collision) {
		canMove = true;
		canLeap = true;
	}

    [Command]
    void CmdInstantiateCamera () {
        GameObject camera = (GameObject) Instantiate (cameraPrefab, transform.position, Quaternion.identity);
        camera.GetComponent<CameraController> ().playerObject = gameObject;
        GetComponent<PlayerController> ().mainCamera = camera;
        GetComponent<PlayerController> ().cameraController = camera.GetComponent<CameraController> ();
        camera.GetComponent<Camera> ().enabled = true;
        camera.GetComponent<AudioListener> ().enabled = true;

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
        weapon.GetComponent<WeaponController> ().playerConnectionId = connectionToClient.connectionId;
        NetworkServer.SpawnWithClientAuthority (weapon, connectionToClient);
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

    void RecoverStamina () {
        float staminaRecovered = staminaRecoveryRate * Time.deltaTime;
        if (currentStamina + staminaRecovered > maxStamina) {
            currentStamina = maxStamina;
        } else {
            currentStamina += staminaRecovered;
        }
    }

    void InputAim () {
        if (Input.GetMouseButtonDown (1)) {
            if (weaponController.currentWeapon == 4) {
                if (isAiming) {
                    isAiming = false;
                    aimCamera.GetComponent<Camera> ().enabled = false;
                    mainCamera.GetComponent<Camera> ().enabled = true;
                    crosshairController.referenceCamera = mainCamera.GetComponent<Camera> ();
                } else {
                    isAiming = true;
                    mainCamera.GetComponent<Camera> ().enabled = false;
                    aimCamera.GetComponent<Camera> ().enabled = true;
                    crosshairController.referenceCamera = aimCamera.GetComponent<Camera> ();
                }
            }
        }
    }

    void InputFire () {
        if (Input.GetMouseButton (0)) { // Fire current weapon
            weaponController.CmdFire (weapon.transform.FindChild ("Weapon Muzzle").position, crosshair.transform.position, crosshairController.accuracy);
        }
    }

    void InputChangeWeapon () {
        if (isAiming) {
            return;
        }
        if (Input.GetKeyDown (KeyCode.Alpha1)) {
            weaponController.CmdChangeWeapon (1);
        } else if (Input.GetKeyDown (KeyCode.Alpha2)) {
            weaponController.CmdChangeWeapon (2);
        } else if (Input.GetKeyDown (KeyCode.Alpha3)) {
            weaponController.CmdChangeWeapon (3);
        } else if (Input.GetKeyDown (KeyCode.Alpha4)) {
            weaponController.CmdChangeWeapon (4);
        }
    }

    void InputMove () {
        if (!canMove || isAiming) {
            return;
        }
        // Update movement direction based on currently pressed directional button
        movementDirection = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0.0f, 0.0f).normalized;
    }

    void Move () {
        if (!canMove || isAiming) {
            return;
        }
        Vector3 movementVector = transform.TransformDirection (movementDirection) * moveSpeed * Time.deltaTime;
        rigidBody.velocity += new Vector2 (movementVector.x, movementVector.y);
    }

	void InputLeap () {
        if (Input.GetKeyUp (KeyCode.Space)) {
            if (canMove && canLeap && !isAiming) {
                Leap ();
            }
        } else if (Input.GetKey (KeyCode.Space)) {
			if (canMove && canLeap && !isAiming) {
                ChargeLeap ();
			}
		}
	}

    void Leap () {
        // Disable leap while leaping
        canLeap = false;
        // Introduce upwards force
        rigidBody.AddForce (transform.up * leapForce);
        // Reset leapForce
        leapForce = 0.0f;
    }

    void ChargeLeap () {
        float leapForceIncrease = leapForceChargeRate * Time.deltaTime;
        if (leapForce + leapForceIncrease > maxLeapForce) { // If leap force after increase will exceed max leap force
            leapForceIncrease = maxLeapForce - leapForce;
        }

        float staminaRequired = leapForceIncrease * staminaPerLeapForce;
        if (staminaRequired > currentStamina) { // If not enough stamina
            leapForceIncrease = currentStamina / staminaPerLeapForce;
            leapForce += leapForceIncrease;
            currentStamina = 0.0f;
        } else {
            leapForce += leapForceIncrease;
            currentStamina -= staminaRequired;
        }
    }

    void InputCycleSpectateTarget () {
        if (Input.GetMouseButtonDown (0)) { // Left click
            targetPlayerId = (targetPlayerId - 1 + players.Length) % players.Length; // Cycle left
        } else if (Input.GetMouseButtonDown (1)) { // Right click
            targetPlayerId = (targetPlayerId + 1) % players.Length; // Cycle right
        }
        cameraController.playerObject = players[targetPlayerId];
    }

    [ClientRpc]
    public void RpcWaitForRespawn () {
        if (isLocalPlayer) {
            isDead = true;
            crosshair.GetComponent<SpriteRenderer> ().enabled = false;
            // Populate players attribute for player-cycling reference laters
            players = GameObject.FindGameObjectsWithTag ("Player");
            // Set main camera initial target
            targetPlayerId = players.Length - 1;
            cameraController.playerObject = players[targetPlayerId];
        }
    }

    void OnDestroy () {
        Destroy (crosshair);
        Destroy (mainCamera);
        Destroy (aimCamera);
    }

}
