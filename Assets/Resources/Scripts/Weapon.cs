using UnityEngine;
using System.Collections;

public class Weapon : Photon.MonoBehaviour {

    public Sprite crosshairSprite;
    public GameObject projectilePrefab;
    public AudioClip fireSoundClip;
    [HideInInspector]
    public AudioSource audioSource;

    public int defaultAmmo; // The number of ammo on full load
    public float reloadTime;
    public int defaultStock; // The default number of magazines / stock

    // Throw related variables
    public bool isThrowable;
    public float maxThrowForce;
    public float throwForceIncreaseRate;

    // Fire related variables
    public float defaultFireDelay;
    public float maxSpreadAngle;
    public float recoil;
    public float knockbackForce;
    public bool canToggle;
    public bool canAim;
    public bool isHoming;
    public float homingSearchRadius;

    // Runtime related variables
    protected float fireDelay;
    [HideInInspector]
    public int ammo;
    [HideInInspector]
    public int stock;
    
    // Network related variables
    [HideInInspector]
    public int playerViewId;
    protected GameObject player;
    [HideInInspector]
    public int weaponId;

    void Start () {
        // Initialize weapon data
        fireDelay = 0.0f;
        InitializeAmmo ();

        if (!photonView.isMine) {
            return;
        }

        // Client specific instantiation begins here
        photonView.RPC ("RpcInitializeWeapon", PhotonTargets.All, playerViewId, weaponId);
    }

    [PunRPC]
    protected virtual void RpcInitializeWeapon (int playerViewId, int weaponId) {
        player = PhotonView.Find (playerViewId).gameObject;
        transform.parent = player.transform;
        audioSource = player.GetComponentInChildren<AudioSource> ();
        player.GetComponentInChildren<WeaponController> ().weapons[weaponId] = this;
    }

    public virtual void InitializeAmmo () {
        ammo = defaultAmmo;
        stock = defaultStock - 1; // 1 stock is already used for the initial ammo
    }

    void Update () {
        UpdateFireDelay ();
    }

    void UpdateFireDelay () {
        if (fireDelay - Time.deltaTime < 0.0f) {
            fireDelay = 0.0f;
        } else {
            fireDelay -= Time.deltaTime;
        }
    }

    public virtual void Toggle () {
        // By default this does nothing
    }

    public virtual void Throw (Vector3 throwPosition, Vector2 throwDirectionalForce) {
        photonView.RPC ("RpcThrow", PhotonTargets.AllViaServer, throwPosition, throwDirectionalForce);

        fireDelay = defaultFireDelay;
        ammo -= 1;
    }

    [PunRPC]
    protected virtual void RpcThrow (Vector3 throwPosition, Vector2 throwDirectionalForce) {
        GameObject throwableObject = (GameObject) Instantiate (projectilePrefab, throwPosition, Quaternion.identity);
        // Ignore collision with instantiating player
        Physics2D.IgnoreCollision (throwableObject.GetComponent<Collider2D> (), player.GetComponent<Collider2D> ());
        // Set throwable object owner
        throwableObject.GetComponent<ThrowableController> ().InstantiatorId = photonView.owner.ID;
        // Introduce throw force
        throwableObject.GetComponent<Rigidbody2D> ().AddForce (throwDirectionalForce);

        PlayFireSoundClip ();
    }

    public virtual void Fire (Vector3 projectilePosition, Quaternion projectileRotation) {
        photonView.RPC ("RpcFire", PhotonTargets.AllViaServer, projectilePosition, projectileRotation);

        fireDelay = defaultFireDelay;
        ammo -= 1;
    }

    [PunRPC]
    protected virtual void RpcFire (Vector3 projectilePosition, Quaternion projectileRotation) {
        GameObject projectile = (GameObject) Instantiate (projectilePrefab, projectilePosition, projectileRotation);
        // Ignore collision with instantiating player
        Physics2D.IgnoreCollision (projectile.GetComponent<Collider2D> (), player.GetComponent<Collider2D> ());
        // Set projectile owner
        projectile.GetComponent<ProjectileController> ().InstantiatorId = photonView.owner.ID;
        // Set projectile color
        projectile.GetComponent<TrailRenderer> ().material.SetColor ("_TintColor", player.GetComponent<MeshRenderer> ().material.color);

        PlayFireSoundClip ();
    }

    public virtual void FireHoming (Vector3 projectilePosition, Quaternion projectileRotation, int targetViewId) {
        photonView.RPC ("RpcFireHoming", PhotonTargets.AllViaServer, projectilePosition, projectileRotation, targetViewId);

        fireDelay = defaultFireDelay;
        ammo -= 1;
    }

    [PunRPC]
    protected virtual void RpcFireHoming (Vector3 projectilePosition, Quaternion projectileRotation, int targetViewId) {
        GameObject projectile = (GameObject) Instantiate (projectilePrefab, projectilePosition, projectileRotation);
        // Ignore collision with instantiating player
        Physics2D.IgnoreCollision (projectile.GetComponent<Collider2D> (), player.GetComponent<Collider2D> ());
        // Set homing target
        projectile.GetComponent<HomingProjectileController> ().Target = targetViewId;
        // Set projectile owner
        projectile.GetComponent<HomingProjectileController> ().InstantiatorId = photonView.owner.ID;
        // Set projectile color
        projectile.GetComponent<TrailRenderer> ().material.SetColor ("_TintColor", player.GetComponent<MeshRenderer> ().material.color);

        PlayFireSoundClip ();
    }

    protected void PlayFireSoundClip () {
        if (fireSoundClip == null) {
            return;
        }
        audioSource.PlayOneShot (fireSoundClip);
    }

    public virtual bool CanThrow () {
        if (fireDelay > 0.0f) { // Cooling down, can't throw
            return false;
        }
        if (ammo <= 0) { // No ammo, can't throw
            return false;
        }
        return true;
    }

    public virtual bool CanFire () {
        if (fireDelay > 0.0f) { // Cooling down, can't fire
            return false;
        }
        if (ammo <= 0) { // No ammo, can't fire
            return false;
        }
        return true;
    }

    public virtual bool CanReload () {
        if (stock <= 0) {
            return false;
        }
        if (ammo == defaultAmmo) { // Ammo is maxed out, can't reload
            return false;
        }
        return true;
    }

    public virtual void Reload () {
        ammo = defaultAmmo;
        stock -= 1;
    }

}
