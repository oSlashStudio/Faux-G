using UnityEngine;
using System.Collections;

public class Shotgun : Weapon {

    public int numShrapnels = 4;
    public float separationAngle = 6; // The angle separating each pair of shrapnels

    public override void InitializeAmmo () {
        ammo = defaultAmmo;
        stock = defaultStock - defaultAmmo;
    }

    public override void Reload () {
        ammo += 1;
        stock -= 1;
    }

    public override void Fire (Vector3 projectilePosition, Quaternion projectileRotation, GameObject player, int instantiatorId) {
        if (numShrapnels % 2 == 1) { // Odd number of shrapnels
            for (int i = -numShrapnels / 2; i <= numShrapnels / 2; i++) {
                Quaternion instantiateRotation = Quaternion.Euler (projectileRotation.eulerAngles + new Vector3 (separationAngle * i, 0.0f, 0.0f));
                Vector3 instantiatePosition = projectilePosition + instantiateRotation * Vector3.forward * 1.0f;

                InstantiateShrapnel (instantiatePosition, instantiateRotation, player, instantiatorId);
            }
        } else { // Even number of shrapnels
            for (float i = -numShrapnels / 2 + 0.5f; i <= numShrapnels / 2 - 0.5f; i += 1.0f) {
                Quaternion instantiateRotation = Quaternion.Euler (projectileRotation.eulerAngles + new Vector3 (separationAngle * i, 0.0f, 0.0f));
                Vector3 instantiatePosition = projectilePosition + instantiateRotation * Vector3.forward * 1.0f;

                InstantiateShrapnel (instantiatePosition, instantiateRotation, player, instantiatorId);
            }
        }

        PlayFireSoundClip ();
    }

    void InstantiateShrapnel (Vector3 instantiatePosition, Quaternion instantiateRotation, GameObject player, int instantiatorId) {
        GameObject shrapnel = (GameObject) Instantiate (projectilePrefab, instantiatePosition, instantiateRotation);

        Physics2D.IgnoreCollision (shrapnel.GetComponent<Collider2D> (), player.GetComponent<Collider2D> ());

        // Set projectile owner
        shrapnel.GetComponent<ProjectileController> ().InstantiatorId = instantiatorId;

        // Set projectile color
        shrapnel.GetComponent<TrailRenderer> ().material.SetColor ("_TintColor", player.GetComponent<MeshRenderer> ().material.color);
    }

}
