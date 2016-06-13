using UnityEngine;
using System.Collections;

public class HardenArmor : Ability {

    public float defaultDuration;
    public float armorIncrease;
    private float duration;
    private bool isActive;

    // Cached components
    private HealthController healthController;

	protected override void Start () {
        healthController = GetComponent<HealthController> ();
        base.Start ();
    }

    protected override void Update () {
        UpdateDuration ();

        base.Update ();
    }

    void UpdateDuration () {
        if (!isActive) {
            return;
        }

        duration -= Time.deltaTime;
        if (duration <= 0.0f) {
            DeactivateAbility ();
        }
    }

    protected override void ActivateAbility () {
        photonView.RPC ("RpcActivateAbility", PhotonTargets.AllViaServer);
    }

    [PunRPC]
    void RpcActivateAbility () {
        duration = defaultDuration;
        isActive = true;

        healthController.armor += armorIncrease;
    }

    void DeactivateAbility () {
        isActive = false;

        healthController.armor -= armorIncrease;
    }

}
