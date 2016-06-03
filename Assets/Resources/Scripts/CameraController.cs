using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public GameObject player;
    public float zOffset = -10.0f;

    public bool isPositionDamped;
    public float positionDamping;
    public bool isRotationDamped;
    public float rotationDamping;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (player == null) {
            return;
        }
        // Update position based on current player position
        if (isPositionDamped) {
            targetPosition = player.transform.position + new Vector3 (0, 0, zOffset);
            transform.position = Vector3.Lerp (transform.position, targetPosition, positionDamping * Time.deltaTime);
        } else {
            transform.position = player.transform.position + new Vector3 (0, 0, zOffset);
        }

        if (isRotationDamped) {
            targetRotation = player.transform.rotation;
            transform.rotation = Quaternion.Slerp (transform.rotation, targetRotation, rotationDamping * Time.deltaTime);
        } else {
            transform.rotation = player.transform.rotation;
        }
	}

}
