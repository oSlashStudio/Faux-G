using UnityEngine;
using System.Collections;

/*
 * This script locks an object relative position to a pivot against the currently active main camera
 */
public class LockRelativePosition : MonoBehaviour {

    public Transform pivot;
    public Vector3 mainCameraOffset; // In order: right, up, forward

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Camera.main != null) {
            transform.position = pivot.position +
                Camera.main.transform.right * mainCameraOffset.x +
                Camera.main.transform.up * mainCameraOffset.y +
                Camera.main.transform.forward * mainCameraOffset.z;
        }
	}

}
