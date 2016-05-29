using UnityEngine;
using System.Collections;

/*
 * This script serializes all transform data from the owner client to other clients.
 */
public class WeaponSyncController : Photon.MonoBehaviour {
    
    private Quaternion lastWeaponRotation = Quaternion.identity;
    private Quaternion currentWeaponRotation = Quaternion.identity;

    private double lastPacketTime = 0.0;
    private double currentPacketTime = 0.0;
    private double syncTime = 0.0;

    // Cached components
    private GameObject weapon;

    // Use this for initialization
    void Start () {
        weapon = transform.FindChild ("Weapon").gameObject;
    }

    // Update is called once per frame
    void Update () {
        if (!photonView.isMine) {
            double syncDelay = currentPacketTime - lastPacketTime;
            syncTime += Time.deltaTime;
            
            weapon.transform.rotation = Quaternion.Lerp (lastWeaponRotation, currentWeaponRotation, (float) (syncTime / syncDelay));
        }
    }

    void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting) {
            if (weapon == null) {
                weapon = transform.FindChild ("Weapon").gameObject;
            }
            stream.SendNext (weapon.transform.rotation);
        } else {
            lastWeaponRotation = currentWeaponRotation;

            currentWeaponRotation = (Quaternion) stream.ReceiveNext ();

            lastPacketTime = currentPacketTime;
            currentPacketTime = info.timestamp;
            syncTime = 0.0;
        }
    }

}
