using UnityEngine;
using System.Collections;

public class FollowCameraRotation : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Camera.main != null) {
            transform.rotation = Camera.main.transform.rotation;
        } else {
            transform.rotation = Quaternion.identity;
        }
    }

}
