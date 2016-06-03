using UnityEngine;
using System.Collections;

public class AimCameraController : MonoBehaviour {

    public GameObject crosshair;
    public GameObject mainCamera;

    private Camera aimCameraComponent;

	// Use this for initialization
	void Start () {
        aimCameraComponent = GetComponent<Camera> ();
	}
	
	// Update is called once per frame
	void Update () {
        if (!aimCameraComponent.enabled) { // When not enabled, ignore updates
            return;
        }
        transform.position = crosshair.transform.position + new Vector3 (0.0f, 0.0f, -8.0f);
        transform.rotation = mainCamera.transform.rotation;
	}
}
