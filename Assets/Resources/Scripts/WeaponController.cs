using UnityEngine;
using System.Collections.Generic;

public class WeaponController : Photon.MonoBehaviour {

    public Weapon[] weapons;
    public GameObject mainCameraPrefab;
    public GameObject crosshairPrefab;
    public GameObject aimCameraPrefab;
    public GameObject throwForceBarPrefab;
    public GameObject minimapCameraPrefab;
    public GUISkin customSkin;

    [HideInInspector]
    public int currentWeapon; // Id of the currently active weapon

    // Throwing related variables
    [HideInInspector]
    public bool isThrowing;
    [HideInInspector]
    public float throwForce;

    // Reload related variables
    private bool isReloading;
    private float reloadTimer;

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
    private GameObject throwForceBar;
    private GameObject minimapCamera;

	// Use this for initialization
	void Start () {
        player = transform.parent.gameObject;
        playerController = player.GetComponent<PlayerController> ();

        if (!photonView.isMine) {
            return;
        }

        // Client specific initialization begins here
        InstantiateWeapon ();

        InstantiateMainCamera ();
        InstantiateCrosshair ();
        InstantiateAimCamera ();
        InstantiateThrowForceBar ();
        InstantiateMinimapCamera ();

        ChangeWeapon (0); // Initialize equipped weapon (weapon 0 by default)
    }

    void InstantiateWeapon () {
        for (int i = 0; i < weapons.Length; i++) {
            // Make a gameobject of each weapon (otherwise they will still be prefabs)
            GameObject currentWeaponGameObject = PhotonNetwork.Instantiate ("Weapons/" + weapons[i].name, Vector3.zero, Quaternion.identity, 0);
            Weapon currentWeapon = currentWeaponGameObject.GetComponent<Weapon> ();
            // Set weapons' networked information for networked initialization
            currentWeapon.playerViewId = player.GetComponent<PhotonView> ().viewID;
            currentWeapon.weaponId = i;
        }
    }

    void InstantiateMainCamera () {
        mainCamera = (GameObject) Instantiate (mainCameraPrefab, Vector3.zero, Quaternion.identity);
        mainCameraComponent = mainCamera.GetComponent<Camera> ();
        mainCamera.GetComponent<CameraController> ().player = player;
    }

    void InstantiateCrosshair () {
        crosshair = (GameObject) Instantiate (crosshairPrefab, Vector3.zero, Quaternion.identity);
        crosshairController = crosshair.GetComponent<CrosshairController> ();
        crosshairController.referenceCamera = mainCameraComponent;
        crosshairSpriteRenderer = crosshair.GetComponent<SpriteRenderer> ();
    }

    void InstantiateAimCamera () {
        aimCamera = (GameObject) Instantiate (aimCameraPrefab, Vector3.zero, Quaternion.identity);
        aimCameraComponent = aimCamera.GetComponent<Camera> ();
        aimCamera.GetComponent<AimCameraController> ().crosshair = crosshair;
        aimCamera.GetComponent<AimCameraController> ().mainCamera = mainCamera;
    }

    void InstantiateThrowForceBar () {
        throwForceBar = (GameObject) Instantiate (throwForceBarPrefab, Vector3.zero, Quaternion.identity);
        throwForceBar.transform.parent = transform;
        throwForceBar.transform.localPosition = -throwForceBar.transform.parent.right * 1.0f;
        throwForceBar.transform.localRotation = Quaternion.Euler (new Vector3 (0.0f, 270.0f, 270.0f));
    }

    void InstantiateMinimapCamera () {
        minimapCamera = (GameObject) Instantiate (minimapCameraPrefab, new Vector3 (0, 0, -10), Quaternion.identity);
    }
	
	// Update is called once per frame
	void Update () {
        if (!photonView.isMine) {
            return;
        }

        UpdateWeaponDirection ();

        if (weapons[currentWeapon].isThrowable) {
            InputThrow ();
        } else {
            InputFire ();
        }
        InputChangeWeapon ();
        InputToggle ();
        InputAim ();
        InputReload ();
        AutoReload ();
        UpdateReload ();
    }

    /*
     * This method updates the weapon rotation based on crosshair position.
     */
    void UpdateWeaponDirection () {
        transform.LookAt (new Vector3 (crosshair.transform.position.x, crosshair.transform.position.y, 0.0f));
        transform.Rotate (new Vector3 (-90.0f, 0.0f, 0.0f));
    }

    void InputThrow () {
        if (Input.GetMouseButtonDown (0)) { // On mouse down, start charging
            if (CanThrow ()) {
                StartChargeThrow ();
            }
        } else if (Input.GetMouseButtonUp (0)) { // On mouse release, throw
            Throw ();
        } else if (Input.GetMouseButton (0)) { // On mouse still down, continue charging
            ChargeThrow ();
        }
    }

    bool CanThrow () {
        if (isReloading) { // Reloading, can't throw
            return false;
        }
        return weapons[currentWeapon].CanThrow ();
    }

    void StartChargeThrow () {
        isThrowing = true;
        throwForce = 0.0f;
    }

    void Throw () {
        if (!isThrowing) {
            return;
        }

        Vector3 throwDirection = (crosshair.transform.position - transform.position).normalized;
        Vector2 throwDirectionalForce = throwDirection * throwForce;

        weapons[currentWeapon].Throw (transform.position, throwDirectionalForce);
        
        // Reset throw related variables
        isThrowing = false;
        throwForce = 0.0f;
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
            if (CanFire ()) {
                if (weapons[currentWeapon].isHoming) {
                    FireHoming (); // Weapon has homing capabilities
                } else {
                    Fire (); // Normal fire
                }
            }
        }
    }

    bool CanFire () {
        if (isReloading) { // Reloading, can't fire
            return false;
        }
        return weapons[currentWeapon].CanFire ();
    }

    void FireHoming () {
        Vector3 directionVector = GetProjectileDirectionVector ();
        Quaternion rotation = GetProjectileRotation (directionVector);

        int targetViewId = AcquireHomingTarget ();

        weapons[currentWeapon].FireHoming (transform.position, rotation, targetViewId);

        // Firing after-effects
        IntroduceRecoil ();
        IntroduceKnockback (-directionVector);
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

    void Fire () {
        Vector3 directionVector = GetProjectileDirectionVector ();
        Quaternion rotation = GetProjectileRotation (directionVector);

        weapons[currentWeapon].Fire (transform.position, rotation);

        // Firing after-effects
        IntroduceRecoil ();
        IntroduceKnockback (-directionVector);
    }

    Vector3 GetProjectileDirectionVector () {
        Vector3 directionVector = (crosshair.transform.position - transform.position).normalized;
        directionVector.z = 0.0f; // Z-coordinate of bullet is always 0

        return directionVector;
    }

    Quaternion GetProjectileRotation (Vector3 directionVector) {
        Quaternion rotation = Quaternion.LookRotation (directionVector);
        Vector3 rotationVector = rotation.eulerAngles;
        // Introduce spread due to recoil
        rotationVector.x += Random.Range (-1.0f, 1.0f) * weapons[currentWeapon].maxSpreadAngle * (1.0f - crosshairController.accuracy);
        // Convert back into quaternion rotation
        rotation = Quaternion.Euler (rotationVector);

        return rotation;
    }

    void IntroduceRecoil () {
        crosshairController.ReduceAccuracy (weapons[currentWeapon].recoil);
    }

    void IntroduceKnockback (Vector3 knockbackDirection) {
        player.GetComponent<Rigidbody2D> ().AddForce (knockbackDirection * weapons[currentWeapon].knockbackForce);
    }

    void InputChangeWeapon () {
        for (int keyNum = 1; keyNum <= 3; keyNum++) {
            if (Input.GetKeyDown (keyNum.ToString ())) {
                if (CanChangeWeapon ()) {
                    ChangeWeapon (keyNum - 1);
                }
            }
        }
    }

    bool CanChangeWeapon () {
        if (isThrowing) { // Charging throw, can't change weapon
            return false;
        }
        return true;
    }

    void ChangeWeapon (int weaponId) {
        if (playerController.isAiming) {
            ToggleAim (); // Reset aiming status upon weapon change
        }
        if (isReloading) {
            isReloading = false; // Reset reloading upon weapon change
        }

        // Change crosshair sprite
        crosshairSpriteRenderer.sprite = weapons[weaponId].crosshairSprite;
        // Call RpcChangeWeapon subroutine on all instances over the network
        photonView.RPC ("RpcChangeWeapon", PhotonTargets.All, weaponId);
    }

    [PunRPC]
    void RpcChangeWeapon (int weaponId) {
        currentWeapon = weaponId;
    }

    void InputToggle () {
        if (Input.GetMouseButtonDown (1)) {
            if (CanToggle ()) {
                ToggleWeapon ();
            }
        }
    }

    bool CanToggle () {
        return weapons[currentWeapon].canToggle;
    }

    void ToggleWeapon () {
        photonView.RPC ("RpcToggleWeapon", PhotonTargets.All);
    }

    [PunRPC]
    void RpcToggleWeapon () {
        weapons[currentWeapon].Toggle ();
    }

    void InputAim () {
        if (Input.GetMouseButtonDown (1)) { // On right click
            if (CanAim ()) {
                ToggleAim ();
            }
        }
    }

    bool CanAim () {
        return weapons[currentWeapon].canAim;
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

    void InputReload () {
        if (Input.GetKey (KeyCode.R)) { // R button for manual reload
            if (CanReload ()) {
                Reload ();
            }
        }
    }

    void AutoReload () {
        if (weapons[currentWeapon].ammo <= 0) {
            if (CanReload ()) {
                Reload ();
            }
        }
    }

    bool CanReload () {
        if (isReloading) { // Already reloading, don't reload
            return false;
        }
        return weapons[currentWeapon].CanReload ();
    }

    void Reload () {
        isReloading = true;
        reloadTimer = weapons[currentWeapon].reloadTime;
    }

    void UpdateReload () {
        if (isReloading) {
            reloadTimer -= Time.deltaTime;

            if (reloadTimer <= 0.0f) {
                isReloading = false;
                weapons[currentWeapon].Reload ();
            }
        }
    }

    public void AddStock (int targetWeaponId, int numStock) {
        photonView.RPC ("RpcAddStock", photonView.owner, targetWeaponId, numStock);
    }

    [PunRPC]
    void RpcAddStock (int targetWeaponId, int numStock) {
        weapons[targetWeaponId].stock += numStock;
    }

    void OnDestroy () {
        Destroy (mainCamera);
        Destroy (crosshair);
        Destroy (aimCamera);
        Destroy (throwForceBar);
        Destroy (minimapCamera);
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

        if (isReloading) {
            GUILayout.BeginArea (RelativeRect (576, 780, 768, 100));

            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label ("Reloading finishes in " + reloadTimer.ToString("0") + " seconds");
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();

            GUILayout.BeginHorizontal (GUILayout.Width (RelativeWidth (512)));
            GUI.skin = customSkin;
            GUILayout.Label ("", "Green",
                GUILayout.Width (RelativeWidth (512.0f * (1.0f - reloadTimer / weapons[currentWeapon].reloadTime))));
            GUILayout.Label ("", "Red");
            GUI.skin = null;
            GUILayout.EndHorizontal ();

            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();

            GUILayout.EndArea ();
        }
        
        GUILayout.BeginArea (RelativeRect (576, 980, 768, 100));
        GUILayout.FlexibleSpace ();
        WeaponsGUI ();
        GUILayout.EndArea ();
    }

    void WeaponsGUI () {
        GUILayout.BeginHorizontal ();

        GUIStyle boxStyle = new GUIStyle (GUI.skin.box);
        boxStyle.alignment = TextAnchor.MiddleCenter;
        GUIStyle labelStyle = new GUIStyle (GUI.skin.label);
        labelStyle.alignment = TextAnchor.MiddleCenter;

        for (int i = 0; i < weapons.Length; i++) {
            GUILayout.BeginVertical ();
            if (i == currentWeapon) { // If this is the current weapon
                GUILayout.Box (weapons[i].ammo + " / " + weapons[i].defaultAmmo + "\n" + weapons[i].stock, boxStyle);
            } else {
                GUILayout.Label (weapons[i].ammo + " / " + weapons[i].defaultAmmo + "\n" + weapons[i].stock, labelStyle);
            }
            GUILayout.EndVertical ();
        }
        GUILayout.EndHorizontal ();
    }

}
