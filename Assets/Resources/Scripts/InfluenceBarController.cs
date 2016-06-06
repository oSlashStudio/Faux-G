using UnityEngine;
using System.Collections;

public class InfluenceBarController : MonoBehaviour {

    // Cached components
    private OutpostController outpostController;
    private SpriteRenderer spriteRenderer;
    private Light lightComponent;

    // Use this for initialization
    void Start () {
        outpostController = GetComponentInParent<OutpostController> ();
        spriteRenderer = GetComponent<SpriteRenderer> ();
        lightComponent = GetComponentInParent<Light> ();
    }

    // Update is called once per frame
    void Update () {
        float maxInfluence = outpostController.maxInfluence;
        float currentInfluence = outpostController.currentInfluence;
        bool isControlled = outpostController.isControlled;
        int controllingTeamId = outpostController.controllingTeamId;
        UpdateInfluenceBarScale (currentInfluence, maxInfluence);
        UpdateInfluenceBarColor (isControlled, controllingTeamId);
    }

    void UpdateInfluenceBarScale (float currentInfluence, float maxInfluence) {
        transform.localScale = new Vector3 (currentInfluence / maxInfluence, transform.localScale.y, transform.localScale.z);
    }

    void UpdateInfluenceBarColor (bool isControlled, int controllingTeamId) {
        if (!isControlled) {
            lightComponent.enabled = false;
        } else {
            lightComponent.enabled = true;
            if (controllingTeamId == 0) {
                lightComponent.color = Color.red;
            } else if (controllingTeamId == 1) {
                lightComponent.color = Color.cyan;
            }
        }

        if (controllingTeamId == 0) {
            spriteRenderer.color = Color.red;
        } else if (controllingTeamId == 1) {
            spriteRenderer.color = Color.cyan;
        }
    }

}
