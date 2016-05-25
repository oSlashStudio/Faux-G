using UnityEngine;
using System.Collections;

/*
 * This script serializes all transform data from the owner client to other clients.
 */
public class PlayerSyncController : Photon.MonoBehaviour {

    private Vector3 lastPosition = Vector3.zero;
    private Vector3 currentPosition = Vector3.zero;
    private Quaternion lastRotation = Quaternion.identity;
    private Quaternion currentRotation = Quaternion.identity;
    private Quaternion lastWeaponRotation = Quaternion.identity;
    private Quaternion currentWeaponRotation = Quaternion.identity;

    private double lastPacketTime = 0.0;
    private double currentPacketTime = 0.0;
    private double syncTime = 0.0;

    // Cached components
    private Rigidbody2D rigidBody;
    private GameObject weapon;

    // Use this for initialization
    void Start () {
        rigidBody = GetComponent<Rigidbody2D> ();
        weapon = transform.FindChild ("Weapon").gameObject;
    }

    // Update is called once per frame
    void Update () {
        if (!photonView.isMine) {
            double syncDelay = currentPacketTime - lastPacketTime;
            syncTime += Time.deltaTime;

            transform.position = Vector3.Lerp (lastPosition, currentPosition, (float) (syncTime / syncDelay));
            transform.rotation = Quaternion.Lerp (lastRotation, currentRotation, (float) (syncTime / syncDelay));
            weapon.transform.rotation = Quaternion.Lerp (lastWeaponRotation, currentWeaponRotation, (float) (syncTime / syncDelay));
        }
    }

    void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting) {
            stream.SendNext (transform.position);
            if (rigidBody == null) {
                rigidBody = GetComponent<Rigidbody2D> ();
            }
            stream.SendNext (rigidBody.velocity);
            stream.SendNext (transform.rotation);
            if (weapon == null) {
                weapon = transform.FindChild ("Weapon").gameObject;
            }
            stream.SendNext (weapon.transform.rotation);
        } else {
            lastPosition = currentPosition;
            lastRotation = currentRotation;
            lastWeaponRotation = currentWeaponRotation;

            currentPosition = (Vector3) stream.ReceiveNext ();
            Vector2 currentVelocity = (Vector2) stream.ReceiveNext ();
            currentRotation = (Quaternion) stream.ReceiveNext ();
            currentWeaponRotation = (Quaternion) stream.ReceiveNext ();

            lastPacketTime = currentPacketTime;
            currentPacketTime = info.timestamp;
            syncTime = 0.0;
            
            currentPosition += (Vector3) currentVelocity * (float) (currentPacketTime - lastPacketTime);
        }
    }

}
