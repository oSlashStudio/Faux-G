using UnityEngine;
using System.Collections;

public class Shotgun : Weapon {

    public int numShrapnels = 3;
    public float separationAngle = 10; // The angle separating each pair of shrapnels

    public override void Fire (Vector3 projectilePosition, Quaternion projectileRotation, GameObject player, int instantiatorId) {
        if (numShrapnels % 2 == 1) { // Odd number of shrapnels
            for (int i = -numShrapnels / 2; i <= numShrapnels / 2; i++) {
                Quaternion instantiateRotation = Quaternion.Euler (projectileRotation.eulerAngles + new Vector3 (separationAngle * i, 0.0f, 0.0f));
                Vector3 instantiatePosition = projectilePosition + instantiateRotation * Vector3.forward * 1.0f;
                InstantiateProjectile (
                    instantiatePosition,
                    instantiateRotation,
                    player,
                    instantiatorId
                    );
            }
        } else { // Even number of shrapnels
            for (float i = -numShrapnels / 2 + 0.5f; i <= numShrapnels / 2 - 0.5f; i += 1.0f) {
                Quaternion instantiateRotation = Quaternion.Euler (projectileRotation.eulerAngles + new Vector3 (separationAngle * i, 0.0f, 0.0f));
                Vector3 instantiatePosition = projectilePosition + instantiateRotation * Vector3.forward * 1.0f;
                InstantiateProjectile (
                    instantiatePosition,
                    instantiateRotation,
                    player,
                    instantiatorId
                    );
            }
        }
    }

    void InstantiateProjectile (Vector3 position, Quaternion rotation, GameObject player, int instantiatorId) {
        GameObject projectile = (GameObject) Instantiate (
                projectilePrefab,
                position,
                rotation);
        Physics2D.IgnoreCollision (projectile.GetComponent<Collider2D> (), player.GetComponent<Collider2D> ());
        // Set projectile owner
        projectile.GetComponent<ProjectileController> ().InstantiatorId = instantiatorId;
    }

}
