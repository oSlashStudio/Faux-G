using UnityEngine;
using System.Collections;

public class SpectateCameraController : MonoBehaviour {

    // Camera zoom related variables
    public float maxOrthographicSize = 30.0f;
    public float minOrthographicSize = 5.0f;

    // Camera movement related variables
    public float cameraMoveSpeed = 20.0f;
    private Vector3 cameraMoveDirection;
    public float dragSpeed = 2.0f;
    private Vector2 dragPivotPoint;

    // Cached components
    private Camera cameraComponent;

    // Use this for initialization
    void Start () {
        cameraComponent = GetComponent<Camera> ();
    }
	
	// Update is called once per frame
	void Update () {
	    if (cameraComponent.enabled) { // If this camera is enabled
            InputZoomCamera ();
            InputMoveCamera ();
        }
	}

    void FixedUpdate () {
        if (cameraComponent.enabled) { // If this camera is enabled
            MoveCamera ();
        }
    }

    void InputZoomCamera () {
        if (Input.GetAxis ("Mouse ScrollWheel") < 0) { // Backward scrolling
            ZoomOut ();
        } else if (Input.GetAxis ("Mouse ScrollWheel") > 0) { // Forward scrolling
            ZoomIn ();
        }
    }

    void ZoomOut () {
        cameraComponent.orthographicSize = Mathf.Min (cameraComponent.orthographicSize + 1.0f, maxOrthographicSize);
    }

    void ZoomIn () {
        cameraComponent.orthographicSize = Mathf.Max (cameraComponent.orthographicSize - 1.0f, minOrthographicSize);
    }

    void InputMoveCamera () {
        // Update movement direction based on currently pressed directional button
        cameraMoveDirection = new Vector3 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"), 0.0f).normalized;
        if (Input.GetMouseButtonDown (0) || Input.GetMouseButtonDown (1) || Input.GetMouseButtonDown (2)) {
            StartDrag ();
        } else if (Input.GetMouseButton (0) || Input.GetMouseButton (1) || Input.GetMouseButton (2)) {
            Drag ();
        }
    }

    void MoveCamera () {
        transform.Translate (cameraMoveDirection * cameraMoveSpeed * Time.fixedDeltaTime);
    }

    void StartDrag () {
        dragPivotPoint = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
    }

    void Drag () {
        Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
        Vector3 directionVector = cameraComponent.ScreenToViewportPoint (new Vector3 (
            mousePosition.x - dragPivotPoint.x,
            mousePosition.y - dragPivotPoint.y,
            0.0f
            ));

        transform.Translate (directionVector * dragSpeed, Space.World);
    }

}
