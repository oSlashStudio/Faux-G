using UnityEngine;
using System.Collections;

public class SpectateCameraController : MonoBehaviour {

    // Camera movement related variables
    public float cameraMoveSpeed = 20.0f;
    private Vector3 cameraMoveDirection;

    // Cached components
    private InGameNetworkManager networkManager;

    // Use this for initialization
    void Start () {
        networkManager = GameObject.FindObjectOfType<InGameNetworkManager> ();
    }
	
	// Update is called once per frame
	void Update () {
	    if (networkManager.IsDead) { // If client's player is respawning
            InputMoveCamera ();
        }
	}

    void FixedUpdate () {
        if (networkManager.IsDead) { // If client's player is respawning
            MoveCamera ();
        }
    }

    void InputMoveCamera () {
        // Update movement direction based on currently pressed directional button
        cameraMoveDirection = new Vector3 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"), 0.0f).normalized;
    }

    void MoveCamera () {
        transform.Translate (cameraMoveDirection * cameraMoveSpeed * Time.fixedDeltaTime);
    }

}
