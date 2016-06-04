using UnityEngine;
using System.Collections;

public class HomingMissileLauncher : Weapon {

    public override void InitializeAmmo () {
        ammo = defaultAmmo;
        stock = defaultStock - defaultAmmo;
    }

    public override void Reload () {
        ammo += 1;
        stock -= 1;
    }

}
