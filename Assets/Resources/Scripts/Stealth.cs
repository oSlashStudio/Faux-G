using UnityEngine;
using System.Collections;
using System.Linq;

public class Stealth : Ability {

    public float defaultDuration;
    public float fadeTime;
    private float duration;
    private bool isActive;

    // Cached components
    private Renderer[] renderers;

    protected override void Start () {
        renderers = GetComponents<Renderer> ().Concat (GetComponentsInChildren<Renderer> ()).ToArray ();
    }

    protected override void Update () {
        UpdateDuration ();
        UpdateRendererAlpha ();

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

    void UpdateRendererAlpha () {
        float alpha = Mathf.Clamp (stealthFunction (duration), 0, 1);
        foreach (Renderer renderer in renderers) {
            renderer.material.color = new Color (
                renderer.material.color.r,
                renderer.material.color.g,
                renderer.material.color.b, 
                alpha
                );
        }
    }

    float stealthFunction (float x) {
        x = Mathf.Clamp (x, 0, defaultDuration);
        if (x < fadeTime) {
            return 1 - x / fadeTime;
        } else if (defaultDuration - x < fadeTime) {
            return 1 - (defaultDuration - x) / fadeTime;
        } else {
            return 0;
        }
    }

    protected override void ActivateAbility () {
        photonView.RPC ("RpcActivateAbility", PhotonTargets.AllViaServer);
    }

    [PunRPC]
    void RpcActivateAbility () {
        duration = defaultDuration;
        isActive = true;
    }

    void DeactivateAbility () {
        isActive = false;
    }

}
