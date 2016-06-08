using UnityEngine;
using System.Collections;

public class Rifle : Weapon {

    public float burstFireDelay = 0.1f;

    public float toggledFireDelay = 0.6f; // The fire delay when toggled, will be swapped with default fire delay on toggle
    public float toggledMaxSpreadAngle = 20.0f; // The max spread angle when toggled, will be swapped with max spread angle on toggle
    public float toggledRecoil = 0.6f; // The recoil when toggled, will be swapped with recoil on toggle

    private bool toggled;
    private int checkedAmmo;

    public override void Toggle () {
        toggled = !toggled;
        // Swap fire delays
        Swap (ref defaultFireDelay, ref toggledFireDelay);
        // Swap spread angles
        Swap (ref maxSpreadAngle, ref toggledMaxSpreadAngle);
        // Swap recoils
        Swap (ref recoil, ref toggledRecoil);
    }

    public override bool CanFire () {
        if (!toggled) {
            return base.CanFire ();
        } else {
            checkedAmmo = ammo; // Save the current # of ammo for determining # of bursted bullets
            return base.CanFire ();
        }
    }

    public override void Fire (Vector3 projectilePosition, Quaternion projectileRotation, GameObject player, int instantiatorId) {
        if (!toggled) { // Semi-automatic mode
            base.Fire (projectilePosition, projectileRotation, player, instantiatorId);
        } else { // Burst fire mode
            StartCoroutine (BurstFire (projectilePosition, projectileRotation, player, instantiatorId));
        }
    }

    IEnumerator BurstFire (Vector3 projectilePosition, Quaternion projectileRotation, GameObject player, int instantiatorId) {
        base.Fire (projectilePosition, projectileRotation, player, instantiatorId);
        if (checkedAmmo < 2) {
            yield break;
        }
        yield return new WaitForSeconds (burstFireDelay);
        base.Fire (projectilePosition, projectileRotation, player, instantiatorId);
        if (checkedAmmo < 3) {
            yield break;
        }
        yield return new WaitForSeconds (burstFireDelay);
        base.Fire (projectilePosition, projectileRotation, player, instantiatorId);
    }

    public override void ResetFire () {
        fireDelay = defaultFireDelay;
        if (toggled) {
            ammo = Mathf.Max (ammo - 3, 0);
        } else {
            ammo -= 1;
        }
    }

    void Swap<T> (ref T lhs, ref T rhs) {
        T temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

}
