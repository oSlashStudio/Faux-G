using UnityEngine;
using System.Collections.Generic;

public class WeaponController : Photon.MonoBehaviour {

    public Weapon[] weapons;
    public GameObject mainCameraPrefab;
    public GameObject crosshairPrefab;
    public GameObject aimCameraPrefab;

    private int currentWeapon; // Id of the currently active weapon

    private List<float> fireDelays;
    public bool isThrowing;
    public float throwForce;

    // Cached components
    private GameObject player; // The player associated with this weapon
    private PlayerController playerController;
    private GameObject mainCamera;
    private Camera mainCameraComponent;
    private GameObject crosshair;
    private CrosshairController crosshairController;
    private SpriteRenderer crosshairSpriteRenderer;
    private GameObject aimCamera;
    private Camera aimCameraComponent;
    private AudioSource audioSource;

	// Use this for initialization
	void Start () {
        player = transform.parent.gameObject;
        playerController = player.GetComponent<PlayerController> ();
        InitializeFireDelays ();
        audioSource = gameObject.GetComponent<AudioSource> ();

        if (!photonView.isMine) {
            return;
        }
        // Client specific instantiation begins here

        // Instantiate main camera
        mainCamera = (GameObject) Instantiate (mainCameraPrefab, Vector3.zero, Quaternion.identity);
        mainCameraComponent = mainCamera.GetComponent<Camera> ();
        mainCamera.GetComponent<CameraController> ().player = gameObject;

        // Instantiate crosshair
        crosshair = (GameObject) Instantiate (crosshairPrefab, Vector3.zero, Quaternion.identity);
        crosshairController = crosshair.GetComponent<CrosshairController> ();
        crosshairController.referenceCamera = mainCameraComponent;
        crosshairSpriteRenderer = crosshair.GetComponent<SpriteRenderer> ();

        // Instantiate aim camera
        aimCamera = (GameObject) Instantiate (aimCameraPrefab, Vector3.zero, Quaternion.identity);
        aimCameraComponent = aimCamera.GetComponent<Camera> ();
        aimCamera.GetComponent<AimCameraController> ().crosshair = crosshair;

        ChangeWeapon (0); // Initialize equipped weapon (weapon 0 by default)
    }

    void InitializeFireDelays () {
        fireDelays = new List<float> ();
        for (int i = 0; i < weapons.Length; i++) {
            fireDelays.Add (0.0f);
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (!photonView.isMine) {
            return;
        }

        UpdateWeaponDirection ();
        UpdateFireDelays ();

        if (weapons[currentWeapon].isThrowable) {
            InputThrow ();
        } else {
            InputFire ();
        }
        InputChangeWeapon ();
        InputToggle ();
        InputAim ();
    }

    /*
     * This method updates the weapon rotation based on crosshair position.
     */
    void UpdateWeaponDirection () {
        transform.LookAt (new Vector3 (crosshair.transform.position.x, crosshair.transform.position.y, 0.0f));
        transform.Rotate (new Vector3 (-90.0f, 0.0f, 0.0f));
    }

    /*
     * This method updates the firing delay of all weapons.
     */
    void UpdateFireDelays () {
        for (int i = 0; i < fireDelays.Count; i++) {
            fireDelays[i] -= Time.deltaTime;
        }
    }

    void InputThrow () {
        if (Input.GetMouseButtonDown (0)) { // On mouse down, start charging
            CheckThrow ();
        } else if (Input.GetMouseButtonUp (0)) { // On mouse release, throw
            Throw ();
        } else if (Input.GetMouseButton (0)) { // On mouse still down, continue charging
            ChargeThrow ();
        }
    }

    void CheckThrow () {
        if (fireDelays[currentWeapon] <= 0.0f) {
            StartChargeThrow ();
            fireDelays[currentWeapon] = weapons[currentWeapon].defaultFireDelay;
        }
    }

    void StartChargeThrow () {
        // Initialize throw related variables
        isThrowing = true;
        throwForce = 0.0f;
    }

    void Throw () {
        if (!isThrowing) {
            return;
        }

        Vector3 throwDirection = (crosshair.transform.position - transform.position).normalized;
        Vector2 throwDirectionalForce = throwDirection * throwForce;
        photonView.RPC ("RpcThrow", PhotonTargets.All, transform.position, throwDirectionalForce);
        // Reset throw related variables
        isThrowing = false;
        throwForce = 0.0f;
    }

    [PunRPC]
    void RpcThrow (Vector3 throwPosition, Vector2 throwDirectionalForce) {
        weapons[currentWeapon].Throw (throwPosition, throwDirectionalForce, player, photonView.owner.ID);
    }

    void ChargeThrow () {
        if (!isThrowing) {
            return;
        }

        float throwForceIncrease = weapons[currentWeapon].throwForceIncreaseRate * Time.deltaTime;
        // Special case: throw force after increase exceeds max throw force
        if (throwForce + throwForceIncrease > weapons[currentWeapon].maxThrowForce) {
            throwForce = weapons[currentWeapon].maxThrowForce;
        } else {
            throwForce += throwForceIncrease;
        }
    }

    void InputFire () {
        if (Input.GetMouseButton (0)) { // On left click
            CheckFire ();
        }
    }

    void CheckFire () {
        if (fireDelays[currentWeapon] <= 0.0f) {
            if (weapons[currentWeapon].isHoming) { // Weapon has homing capabilities
                FireHoming ();
            } else {
                Fire ();
            }
            fireDelays[currentWeapon] = weapons[currentWeapon].defaultFireDelay;
        }
    }

    void Fire () {
        Vector3 directionVector = (crosshair.transform.position - transform.position).normalized;
        directionVector.z = 0.0f; // Z-coordinate of bullet is always 0

        Quaternion rotation = Quaternion.LookRotation (directionVector);
        Vector3 rotationVector = rotation.eulerAngles;
        // Introduce spread due to recoil
        rotationVector.x += Random.Range (-1.0f, 1.0f) * weapons[currentWeapon].maxSpreadAngle * (1.0f - crosshairController.accuracy);
        // Convert back into quaternion rotation
        rotation = Quaternion.Euler (rotationVector);

        photonView.RPC ("RpcFire", PhotonTargets.All, transform.position, rotation);

        // Firing after-effects
        IntroduceRecoil ();
        IntroduceKnockback (-directionVector);
    }

    [PunRPC]
    void RpcFire (Vector3 projectilePosition, Quaternion projectileRotation) {
        weapons[currentWeapon].Fire (projectilePosition, projectileRotation, player, photonView.owner.ID);

        // Networked effects
        PlayFireSoundClip ();
    }

    void FireHoming () {
        Vector3 directionVector = (crosshair.transform.position - transform.position).normalized;
        directionVector.z = 0.0f; // Z-coordinate of bullet is always 0

        Quaternion rotation = Quaternion.LookRotation (directionVector);
        Vector3 rotationVector = rotation.eulerAngles;
        // Introduce spread due to recoil
        rotationVector.x += Random.Range (-1.0f, 1.0f) * weapons[currentWeapon].maxSpreadAngle * (1.0f - crosshairController.accuracy);
        // Convert back into quaternion rotation
        rotation = Quaternion.Euler (rotationVector);

        int targetViewId = AcquireHomingTarget ();

        photonView.RPC ("RpcFireHoming", PhotonTargets.All, transform.position, rotation, targetViewId);

        // Firing after-effects
        IntroduceRecoil ();
        IntroduceKnockback (-directionVector);
    }

    [PunRPC]
    void RpcFireHoming (Vector3 projectilePosition, Quaternion projectileRotation, int targetViewId) {
        weapons[currentWeapon].FireHoming (projectilePosition, projectileRotation, player, photonView.owner.ID, targetViewId);

        // Networked effects
        PlayFireSoundClip ();
    }

    /*
     * This method handles target acquirement upon firing a homing-based weapon.
     * Returns the target's photon view ID if target is found, returns -1 otherwise.
     */
    int AcquireHomingTarget () {
        // Find all colliders within current weapon's homing search radius
        Collider2D[] targetColliders = Physics2D.OverlapCircleAll (crosshair.transform.position, weapons[currentWeapon].homingSearchRadius);

        if (targetColliders == null || targetColliders.Length == 0) {
            return -1; // No targets found in search radius, return -1
        }

        foreach (Collider2D targetCollider in targetColliders) {
            PhotonView photonView = targetCollider.GetComponent<PhotonView> ();
            if (photonView == null) {
                continue; // No photon view in this target, continue to check next target
            }

            return photonView.viewID; // This target has PhotonView component, return the view ID
        }

        return -1; // No targets with PhotonView in search radius is found, return -1
    }

    void IntroduceRecoil () {
        crosshairController.ReduceAccuracy (weapons[currentWeapon].recoil);
    }

    void IntroduceKnockback (Vector3 knockbackDirection) {
        player.GetComponent<Rigidbody2D> ().AddForce (knockbackDirection * weapons[currentWeapon].knockbackForce);
    }

    void PlayFireSoundClip () {
        if (weapons[currentWeapon].fireSoundClip != null) { // If this weapon has sound clip attached
            audioSource.PlayOneShot (weapons[currentWeapon].fireSoundClip);
        }
    }

    void InputChangeWeapon () {
        if (Input.GetKeyDown (KeyCode.Alpha1)) {
            CheckChangeWeapon (0);
        } else if (Input.GetKeyDown (KeyCode.Alpha2)) {
            CheckChangeWeapon (1);
        } else if (Input.GetKeyDown (KeyCode.Alpha3)) {
            CheckChangeWeapon (2);
        } else if (Input.GetKeyDown (KeyCode.Alpha4)) {
            CheckChangeWeapon (3);
        } else if (Input.GetKeyDown (KeyCode.Alpha5)) {
            CheckChangeWeapon (4);
        } else if (Input.GetKeyDown (KeyCode.Alpha6)) {
            CheckChangeWeapon (5);
        } else if (Input.GetKeyDown (KeyCode.Alpha7)) {
            CheckChangeWeapon (6);
        } else if (Input.GetKeyDown (KeyCode.Alpha8)) {
            CheckChangeWeapon (7);
        }
    }

    void CheckChangeWeapon (int weaponId) {
        if (playerController.isAiming) {
            return; // Can't change weapon while aiming
        }

        if ((0 <= weaponId) && (weaponId < weapons.Length)) { // Check for weapon id validity
            ChangeWeapon (weaponId);
        }
    }

    void ChangeWeapon (int weaponId) {
        // Change crosshair sprite
        crosshairSpriteRenderer.sprite = weapons[weaponId].crosshairSprite;
        // Call RpcChangeWeapon subroutine on all instances over the network
        photonView.RPC ("RpcChangeWeapon", PhotonTargets.AllBufferedViaServer, weaponId);
    }

    [PunRPC]
    void RpcChangeWeapon (int weaponId) {
        currentWeapon = weaponId;
    }

    void InputToggle () {
        if (Input.GetMouseButtonDown (1)) {
            CheckToggle ();
        }
    }

    void CheckToggle () {
        if (weapons[currentWeapon].canToggle) {
            ToggleWeapon ();
        }
    }

    void ToggleWeapon () {
        weapons[currentWeapon].Toggle ();
    }

    void InputAim () {
        if (Input.GetMouseButtonDown (1)) { // On right click
            CheckAim ();
        }
    }

    void CheckAim () {
        if (weapons[currentWeapon].canAim) {
            ToggleAim ();
        }
    }

    void ToggleAim () {
        if (playerController.isAiming) { // Toggle from aiming to normal
            playerController.isAiming = false;
            aimCameraComponent.enabled = false;
            mainCameraComponent.enabled = true;
            crosshairController.referenceCamera = mainCameraComponent;
        } else { // Toggle from normal to aiming
            playerController.isAiming = true;
            mainCameraComponent.enabled = false;
            aimCameraComponent.enabled = true;
            crosshairController.referenceCamera = aimCameraComponent;
        }
    }

    void OnDestroy () {
        Destroy (mainCamera);
        Destroy (crosshair);
        Destroy (aimCamera);
    }

}
