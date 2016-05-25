using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public GameObject player;
    public float zOffset = -10.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (player == null) {
            return;
        }
        // Update position based on current player position
        transform.position = player.transform.position + new Vector3 (0.0f, 0.0f, zOffset);
	}

}
