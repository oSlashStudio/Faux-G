using UnityEngine;
using System.Collections;

public class AmmoPodController : Photon.MonoBehaviour {

    public GameObject[] ammoPrefabs;
    public float defaultSpawnDelay;

    private float spawnDelay;
    private GameObject ammoPack;

	// Use this for initialization
	void Start () {
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
	}

    void SpawnAmmo () {
        int spawnedAmmoId = Random.Range (0, ammoPrefabs.Length);
        ammoPack = PhotonNetwork.InstantiateSceneObject (ammoPrefabs[spawnedAmmoId].name, 
            transform.position + transform.up, transform.rotation, 0, null);
    }

}
