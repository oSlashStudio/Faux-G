using UnityEngine;
using System.Collections;

public class Rifle : Weapon {

    public float burstFireDelay = 0.1f;

    public float toggledFireDelay = 0.6f; // The fire delay when toggled, will be swapped with default fire delay on toggle
    public float toggledMaxSpreadAngle = 20.0f; // The max spread angle when toggled, will be swapped with max spread angle on toggle
    public float toggledRecoil = 0.6f; // The recoil when toggled, will be swapped with recoil on toggle

    private bool toggled;

    public override void Toggle () {
        toggled = !toggled;
        // Swap fire delays
        Swap (ref defaultFireDelay, ref toggledFireDelay);
        // Swap spread angles
        Swap (ref maxSpreadAngle, ref toggledMaxSpreadAngle);
        // Swap recoils
        Swap (ref recoil, ref toggledRecoil);
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
        yield return new WaitForSeconds (burstFireDelay);
        base.Fire (projectilePosition, projectileRotation, player, instantiatorId);
        yield return new WaitForSeconds (burstFireDelay);
        base.Fire (projectilePosition, projectileRotation, player, instantiatorId);
    }

    public override bool CanFire () {
        if (toggled) {
            if (fireDelay > 0.0f) { // Cooling down, can't fire
                return false;
            }
            if (ammo < 3) { // Not enough ammo, can't fire
                return false;
            }
            return true;
        } else {
            return base.CanFire ();
        }
    }

    void Swap<T> (ref T lhs, ref T rhs) {
        T temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

}
