using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ShieldingController : NetworkBehaviour {

    public Transform rotationReference;
    public float angularVelocity = 10.0f;
    public float rotationSpeed = 10.0f;

    private Rigidbody2D rigidBody;

    public override void OnStartClient () {
        if (!isServer) {
            CmdSyncShieldTransform ();
        }
    }

	// Use this for initialization
	void Start () {
        rigidBody = GetComponent<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {
        rigidBody.angularVelocity = angularVelocity;
        transform.RotateAround (rotationReference.position, Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    [Command]
    void CmdSyncShieldTransform () {
        RpcSyncShieldTransform (transform.position, transform.rotation);
    }

    [ClientRpc]
    void RpcSyncShieldTransform (Vector3 position, Quaternion rotation) {
        transform.position = position;
        transform.rotation = rotation;
    }

}
