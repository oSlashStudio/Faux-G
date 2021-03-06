using UnityEngine;
using System.Collections;

public class AmmoPackController : Photon.MonoBehaviour {

    public float rotationalSpeed;
    public int targetWeapon;
    public int addStock;

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
            WeaponController targetWeaponController = collider.gameObject.GetComponentInChildren<WeaponController> ();
            targetWeaponController.AddStock (targetWeapon, addStock); // Add stock(s) to target weapon
            PhotonNetwork.Destroy (gameObject);
        }
    }

}
