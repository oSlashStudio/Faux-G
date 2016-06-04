using UnityEngine;
using System.Collections;

public class StaminaPackController : Photon.MonoBehaviour {

    public float rotationalSpeed;
    public float staminaAmount;

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
            PlayerController targetPlayerController = collider.gameObject.GetComponent<PlayerController> ();
            targetPlayerController.currentStamina = Mathf.Min (targetPlayerController.currentStamina + staminaAmount, targetPlayerController.maxStamina);
            PhotonNetwork.Destroy (gameObject);
        }
    }

}
