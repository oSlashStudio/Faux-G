using UnityEngine;
using System.Collections;

public class StaminaController : Photon.MonoBehaviour {

    public GameObject calloutPrefab;
    public GameObject staminaBarPrefab;

    public float maxStamina;
    public float staminaRecoveryRate; // The amount of stamina recovered per second
    [HideInInspector]
    public float currentStamina;

    // Cached components
    private GameObject staminaBar;

    // Use this for initialization
    void Start () {
        currentStamina = maxStamina;

        if (!photonView.isMine) {
            return;
        }

        // Client specific instantiation begins here

        InstantiateStaminaBar ();
	}
	
    void InstantiateStaminaBar () {
        staminaBar = (GameObject) Instantiate (staminaBarPrefab, Vector3.zero, Quaternion.identity);
        staminaBar.transform.parent = transform;
        staminaBar.transform.localPosition = new Vector3 (0.0f, 1.4f, -1.0f);
    }

	// Update is called once per frame
	void Update () {
	    if (!photonView.isMine) {
            return;
        }

        RecoverStamina ();
	}

    /*
     * This method handles the stamina recovery of player on each frame.
     */
    void RecoverStamina () {
        float staminaRecovered = staminaRecoveryRate * Time.deltaTime;

        // Special case: stamina exceeds max stamina after increase
        if (currentStamina + staminaRecovered > maxStamina) {
            currentStamina = maxStamina;
        } else { // Normal case
            currentStamina += staminaRecovered;
        }
    }

    [PunRPC]
    public void RpcRejuvenateOwner (float rejuvenateAmount) {
        if (!photonView.isMine) { // Not owner, forward to owner
            photonView.RPC ("RpcRejuvenateOwner", photonView.owner, rejuvenateAmount);
        } else {
            Rejuvenate (rejuvenateAmount);
        }
    }

    public void Rejuvenate (float rejuvenateAmount) {
        if (!photonView.isMine) {
            return;
        }
        if (rejuvenateAmount == 0.0f) { // Ignore 0 stamina rejuvenation
            return;
        }

        // Special case: if stamina after rejuvenation exceeds max stamina
        if (currentStamina + rejuvenateAmount > maxStamina) {
            currentStamina = maxStamina;
        } else {
            currentStamina += rejuvenateAmount;
        }

        photonView.RPC ("RpcRejuvenate", PhotonTargets.All, rejuvenateAmount);
    }

    [PunRPC]
    void RpcRejuvenate (float rejuvenateAmount) {
        InstantiateRejuvenateCallout (rejuvenateAmount);
    }

    void InstantiateRejuvenateCallout (float rejuvenateAmount) {
        Vector3 calloutPosition = new Vector3 (transform.position.x, transform.position.y, -2.0f);
        Quaternion calloutRotation = (Camera.main == null) ? Quaternion.identity : Camera.main.transform.rotation;
        GameObject callout = (GameObject) Instantiate (calloutPrefab, calloutPosition, calloutRotation);
        callout.GetComponent<TextMesh> ().text = "+" + rejuvenateAmount.ToString ("0");
        callout.GetComponent<TextMesh> ().color = Color.cyan;
    }

    void OnDestroy () {
        Destroy (staminaBar);
    }

}
