using UnityEngine;
using System.Collections;

public class AmmoPodController : Photon.MonoBehaviour {

    public GameObject[] ammoPrefabs;
    public float defaultSpawnDelay;

    private float spawnDelay;
    private bool hasAmmoPack;

    // Cached components
    private Component halo;
    private GameObject ammoPack;

    // Use this for initialization
    void Start () {
        halo = GetComponent ("Halo");
        spawnDelay = defaultSpawnDelay;
	}
	
	// Update is called once per frame
	void Update () {
        if (!photonView.isMine) {
            return;
        }

        if (ammoPack == null) {
            spawnDelay -= Time.deltaTime;
            if (spawnDelay <= 0.0f) {
                SpawnAmmo ();
                spawnDelay = defaultSpawnDelay;
            }
        }

        if (!hasAmmoPack && ammoPack != null) {
            hasAmmoPack = true;
            photonView.RPC ("RpcSetHalo", PhotonTargets.All, true);
        } else if (hasAmmoPack && ammoPack == null) {
            hasAmmoPack = false;
            photonView.RPC ("RpcSetHalo", PhotonTargets.All, false);
        }
	}

    void SpawnAmmo () {
        int spawnedAmmoId = Random.Range (0, ammoPrefabs.Length);
        ammoPack = PhotonNetwork.InstantiateSceneObject (ammoPrefabs[spawnedAmmoId].name, 
            transform.position + transform.up, transform.rotation, 0, null);
    }

    [PunRPC]
    void RpcSetHalo (bool flag) {
        halo.GetType ().GetProperty ("enabled").SetValue (halo, flag, null);
    }

}
