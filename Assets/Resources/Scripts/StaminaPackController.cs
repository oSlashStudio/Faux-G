using UnityEngine;
using System.Collections;

public class StaminaPackController : Photon.MonoBehaviour {

    public float rotationalSpeed;
    public float rejuvenateAmount;

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update () {
        transform.Rotate (transform.up, rotationalSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D (Collider2D collider) {
        if (!photonView.isMine) {
            return;
        }

        if (collider.tag == "Player") {
            StaminaController targetStaminaController = collider.gameObject.GetComponent<StaminaController> ();
            targetStaminaController.RpcRejuvenateOwner (rejuvenateAmount);
            PhotonNetwork.Destroy (gameObject);
        }
    }

}
