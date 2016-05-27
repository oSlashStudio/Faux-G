using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

    public Sprite crosshairSprite;
    public GameObject projectilePrefab;
    public AudioClip fireSoundClip;

    // Throw related variables
    public bool isThrowable;
    public float maxThrowForce;
    public float throwForceIncreaseRate;

    // Fire related variables
    public float defaultFireDelay;
    public float maxSpreadAngle;
    public float recoil;
    public float knockbackForce;
    public bool canAim;
    public bool isHoming;
    public float homingSearchRadius;

    public virtual void Throw (Vector3 throwPosition, Vector2 throwDirectionalForce, GameObject player, int instantiatorViewId) {
        GameObject throwableObject = (GameObject) Instantiate (projectilePrefab, throwPosition, Quaternion.identity);

        Physics2D.IgnoreCollision (throwableObject.GetComponent<Collider2D> (), player.GetComponent<Collider2D> ());

        // Set throwable object owner
        throwableObject.GetComponent<ThrowableController> ().InstantiatorViewId = instantiatorViewId;

        throwableObject.GetComponent<Rigidbody2D> ().AddForce (throwDirectionalForce);
    }

    public virtual void Fire (Vector3 projectilePosition, Quaternion projectileRotation, GameObject player, int instantiatorViewId) {
        GameObject projectile = (GameObject) Instantiate (projectilePrefab, projectilePosition, projectileRotation);

        Physics2D.IgnoreCollision (projectile.GetComponent<Collider2D> (), player.GetComponent<Collider2D> ());

        // Set projectile owner
        projectile.GetComponent<ProjectileController> ().InstantiatorViewId = instantiatorViewId;
    }

    public virtual void FireHoming (Vector3 projectilePosition, Quaternion projectileRotation, GameObject player, int instantiatorViewId, int targetViewId) {
        GameObject projectile = (GameObject) Instantiate (projectilePrefab, projectilePosition, projectileRotation);

        Physics2D.IgnoreCollision (projectile.GetComponent<Collider2D> (), player.GetComponent<Collider2D> ());

        // Set target
        projectile.GetComponent<HomingProjectileController> ().Target = targetViewId;

        // Set projectile owner
        projectile.GetComponent<HomingProjectileController> ().InstantiatorViewId = instantiatorViewId;
    }

}
