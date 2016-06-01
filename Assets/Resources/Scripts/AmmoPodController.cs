using UnityEngine;
using System.Collections;

public class AmmoPodController : Photon.MonoBehaviour {

    public GameObject[] ammoPrefabs;
    public float defaultSpawnDelay;

    private float spawnDelay;
    private bool hasAmmoPack = true;

    // Cached components
    private Light halo;
    private GameObject ammoPack;

    // Use this for initialization
    void Start () {
        halo = GetComponent<Light> ();
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

        if (hasAmmoPack && ammoPack == null) {
            hasAmmoPack = false;
            photonView.RPC ("RpcSetHalo", PhotonTargets.All, false, 0f, 0f, 0f);
        }
	}

    void SpawnAmmo () {
        int spawnedAmmoId = Random.Range (0, ammoPrefabs.Length);
        ammoPack = PhotonNetwork.InstantiateSceneObject (ammoPrefabs[spawnedAmmoId].name, 
            transform.position + transform.up, transform.rotation, 0, null);

        hasAmmoPack = true;

        Color haloColor;
        switch (spawnedAmmoId) {
            case 0:
                haloColor = new Color (255, 0, 0);
                break;
            case 1:
                haloColor = new Color (0, 255, 255);
                break;
            case 2:
                haloColor = new Color (255, 255, 0);
                break;
            default:
                haloColor = new Color (255, 255, 255);
                break;
        }
        photonView.RPC ("RpcSetHalo", PhotonTargets.All, true, haloColor.r, haloColor.g, haloColor.b);
    }

    [PunRPC]
    void RpcSetHalo (bool flag, float r, float g, float b) {
        halo.enabled = flag;
        if (halo.enabled) {
            halo.color = new Color (r, g, b);
        }
    }

}
