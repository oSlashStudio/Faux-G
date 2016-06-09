using UnityEngine;
using System.Collections;
using System.Linq;

public class Stealth : Ability {

    public float defaultDuration;
    public float fadeTime;
    private float duration;
    private bool isActive;

    // Cached components
    private MeshRenderer[] meshRenderers;
    private SpriteRenderer[] spriteRenderers;
    private MeshRenderer[] childrenMeshRenderers;
    private SpriteRenderer[] childrenSpriteRenderers;

    protected override void Start () {
        meshRenderers = GetComponents<MeshRenderer> ().Concat (GetComponentsInChildren<MeshRenderer> ()).ToArray ();
        spriteRenderers = GetComponents<SpriteRenderer> ().Concat (GetComponentsInChildren<SpriteRenderer> ()).ToArray ();
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
        foreach (MeshRenderer meshRenderer in meshRenderers) {
            meshRenderer.material.color = new Color (
                meshRenderer.material.color.r, 
                meshRenderer.material.color.g, 
                meshRenderer.material.color.b, 
                alpha
                );
        }
        foreach (SpriteRenderer spriteRenderer in spriteRenderers) {
            spriteRenderer.material.color = new Color (
                spriteRenderer.material.color.r,
                spriteRenderer.material.color.g,
                spriteRenderer.material.color.b,
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
        photonView.RPC ("RpcActivateAbility", PhotonTargets.All);
    }

    [PunRPC]
    void RpcActivateAbility () {
        duration = defaultDuration;
        isActive = true;
    }

    void DeactivateAbility () {
        photonView.RPC ("RpcDeactivateAbility", PhotonTargets.All);
    }

    [PunRPC]
    void RpcDeactivateAbility () {
        isActive = false;
    }

}
