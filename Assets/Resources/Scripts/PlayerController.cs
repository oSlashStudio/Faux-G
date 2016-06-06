using UnityEngine;
using System.Collections;

public class PlayerController : Photon.MonoBehaviour {
    
    public GameObject jumpForceBarPrefab;
    public GameObject sprintTrailPrefab;

    // Move related variables
    public float walkSpeed; // The movement speed of this player
    private Vector3 moveDirection;
    private float moveSpeed;

    // Sprint related variables
    public float sprintSpeed;
    public float staminaPerSprintSecond;
    private bool isSprinting;

    // Jump related variables
    public float maxJumpForce; // The maximum jump force of this player
    public float jumpForceChargeRate; // The increase in jump force per second when charged
    public float staminaPerJumpForce; // The amount of stamina consumed per unit of jump force
    [HideInInspector]
    public float jumpForce;

    public GUISkin customSkin;

    // State related variables
    [HideInInspector]
    public bool isAiming = false;

    // Cached components
    private Rigidbody2D rigidBody;
    private GameObject jumpForceBar;
    private HealthController healthController;
    private StaminaController staminaController;
    private PhotonTransformView photonTransformView;
    private GameObject sprintTrail;

	// Use this for initialization
	void Start () {
        rigidBody = GetComponent<Rigidbody2D> ();
        healthController = GetComponent<HealthController> ();
        staminaController = GetComponent<StaminaController> ();
        photonTransformView = GetComponent<PhotonTransformView> ();

        moveSpeed = walkSpeed;
        jumpForce = 0.0f;

        if (!photonView.isMine) {
            rigidBody.isKinematic = true; // If this client can't control, set isKinematic to true
            return;
        }

        // Client specific instantiation begins here

        // Synchronize color to instances over the network
        photonView.RPC ("RpcChangeColor", PhotonTargets.Others,
            GetComponent<MeshRenderer> ().material.color.r,
            GetComponent<MeshRenderer> ().material.color.g,
            GetComponent<MeshRenderer> ().material.color.b
            );
        
        InstantiateJumpForceBar ();
	}

    void InstantiateJumpForceBar () {
        jumpForceBar = (GameObject) Instantiate (jumpForceBarPrefab, Vector3.zero, Quaternion.identity);
        jumpForceBar.transform.parent = transform;
        jumpForceBar.transform.localPosition = new Vector3 (0.0f, 1.6f, -1.0f);
    }

	// Update is called once per frame
	void Update () {
	    if (!photonView.isMine) { // Unable to control other players
            return;
        }

        // Handle movement related routines
        InputMove ();
        InputSprint ();
        InputJump ();

        photonTransformView.SetSynchronizedValues (rigidBody.velocity, rigidBody.angularVelocity);
    }

    void FixedUpdate () {
        Move ();
    }

    [PunRPC]
    void RpcChangeColor (float rColor, float gColor, float bColor) {
        GetComponent<MeshRenderer> ().material.color = new Color (rColor, gColor, bColor);
    }

    /*
     * This method handles the movement input from player (input horizontal buttons).
     */
    void InputMove () {
        // Update movement direction based on currently pressed directional button
        moveDirection = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0.0f, 0.0f).normalized;
    }

    /*
     * This method handles the movement of player based on the current movement direction.
     * Should be called from FixedUpdate ().
     */
    void Move () {
        // Translates movementDirection from local space to world space
        Vector3 movementVector = transform.TransformDirection (moveDirection) * moveSpeed * Time.fixedDeltaTime;
        // Apply velocity to player
        rigidBody.velocity += (Vector2) movementVector;
    }

    void InputSprint () {
        if (Input.GetKeyUp (KeyCode.LeftShift)) {
            if (isSprinting) {
                isSprinting = false;
                moveSpeed = walkSpeed;
                photonView.RPC ("RpcSprint", PhotonTargets.All, false);
            }
        } else if (Input.GetKeyDown (KeyCode.LeftShift)) {
            isSprinting = true;
            moveSpeed = sprintSpeed;
            photonView.RPC ("RpcSprint", PhotonTargets.All, true);
        }

        if (isSprinting) {
            float staminaRequired = staminaPerSprintSecond * Time.deltaTime;
            if (staminaController.currentStamina < staminaRequired) { // Special case: not enough stamina
                isSprinting = false; // Stop sprinting
                moveSpeed = walkSpeed;
                photonView.RPC ("RpcSprint", PhotonTargets.All, false);
            } else {
                staminaController.currentStamina -= staminaPerSprintSecond * Time.deltaTime;
            }
        }
    }

    [PunRPC]
    void RpcSprint (bool flag) {
        if (flag) {
            sprintTrail = (GameObject) Instantiate (sprintTrailPrefab, transform.position, transform.rotation);
            sprintTrail.GetComponent<TrailRenderer> ().material.SetColor ("_TintColor", GetComponent<MeshRenderer> ().material.color);
            sprintTrail.transform.parent = transform;
        } else {
            if (sprintTrail == null) {
                return;
            }
            sprintTrail.transform.parent = null;
        }
    }

    /*
     * This method handles the jump input from player (space bar button).
     */
    void InputJump () {
        if (Input.GetKeyUp (KeyCode.Space)) {
            Jump ();
        } else if (Input.GetKey (KeyCode.Space)) {
            ChargeJump ();
        }
    }

    /*
     * This method handles the jumping movement of player based on the currently charged jump force.
     */
    void Jump () {
        // Add upwards force equivalent to charged jump force
        rigidBody.AddForce (transform.up * jumpForce);
        // Reset charged jump force after jumping
        jumpForce = 0.0f;
    }

    /*
     * This method handles the charging jump routine when player holds the space bar.
     */
    void ChargeJump () {
        float jumpForceIncrease = jumpForceChargeRate * Time.deltaTime;

        // Special case #1: jump force exceeds max jump force after increase
        if (jumpForce + jumpForceIncrease > maxJumpForce) {
            jumpForceIncrease = maxJumpForce - jumpForce;
        }

        float staminaRequired = jumpForceIncrease * staminaPerJumpForce; // Calculate stamina required based on increase in jump force

        // Special case #2: not enough stamina
        if (staminaController.currentStamina < staminaRequired) {
            jumpForceIncrease = staminaController.currentStamina / staminaPerJumpForce; // Maximum increase in jump force with current stamina
            jumpForce += jumpForceIncrease;
            staminaController.currentStamina = 0.0f; // Stamina is drained entirely in this case
            return;
        }

        // Normal case
        jumpForce += jumpForceIncrease;
        staminaController.currentStamina -= staminaRequired;
    }

    void OnDestroy () {
        Destroy (jumpForceBar);
    }

    /*
     * Get a rectangle relative to full HD 1920:1080 screen
     */
    Rect RelativeRect (float x, float y, float w, float h) {
        float relativeX = Screen.width * x / 1920;
        float relativeY = Screen.height * y / 1080;
        float relativeW = Screen.width * w / 1920;
        float relativeH = Screen.height * h / 1080;

        return new Rect (relativeX, relativeY, relativeW, relativeH);
    }

    float RelativeWidth (float w) {
        float relativeW = Screen.width * w / 1920;

        return relativeW;
    }

    float RelativeHeight (float h) {
        float relativeH = Screen.height * h / 1080;

        return relativeH;
    }

    void OnGUI () {
        if (!photonView.isMine) {
            return;
        }

        GUILayout.BeginArea (RelativeRect (576, 880, 768, 100));
        GUILayout.FlexibleSpace ();
        StatusGUI ();
        GUILayout.EndArea ();
    }

    void StatusGUI () {
        GUILayout.BeginHorizontal ();

        GUILayout.BeginHorizontal (GUILayout.Width (RelativeWidth (300)));
        GUI.skin = customSkin;
        GUILayout.Label ("", "Green", 
            GUILayout.Width (RelativeWidth (300.0f * healthController.currentHealth / healthController.maxHealth)));
        GUILayout.Label ("", "Red");
        GUI.skin = null;
        GUILayout.EndHorizontal ();

        GUILayout.FlexibleSpace ();

        GUILayout.BeginHorizontal (GUILayout.Width (RelativeWidth (300)));
        GUI.skin = customSkin;
        GUILayout.Label ("", "Cyan",
            GUILayout.Width (RelativeWidth (300.0f * staminaController.currentStamina / staminaController.maxStamina)));
        GUILayout.Label ("", "White");
        GUI.skin = null;
        GUILayout.EndHorizontal ();

        GUILayout.EndHorizontal ();
    }

}
