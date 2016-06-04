using UnityEngine;
using System.Collections;

public class HealthPackController : Photon.MonoBehaviour {

    public float rotationalSpeed;
    public float healAmount;

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
            HealthController targetHealthController = collider.gameObject.GetComponent<HealthController> ();
            targetHealthController.Heal (healAmount, targetHealthController.transform.position);
            PhotonNetwork.Destroy (gameObject);
        }
    }

}
