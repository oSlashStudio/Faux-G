using UnityEngine;
using System.Collections;

public class Glock : Weapon {
    
    public float burstFireDelay = 0.1f;

    public float toggledFireDelay = 1.0f; // The fire delay when toggled, will be swapped with default fire delay on toggle
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
        if (ammo == 0) {
            yield break;
        }
        yield return new WaitForSeconds (burstFireDelay);
        base.Fire (projectilePosition, projectileRotation, player, instantiatorId);
        if (ammo == 0) {
            yield break;
        }
        yield return new WaitForSeconds (burstFireDelay);
        base.Fire (projectilePosition, projectileRotation, player, instantiatorId);
    }

    void Swap<T> (ref T lhs, ref T rhs) {
        T temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

}
