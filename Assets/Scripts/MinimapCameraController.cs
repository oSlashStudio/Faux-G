using UnityEngine;
using System.Collections;

public class MinimapCameraController : MonoBehaviour {

    Camera cameraComponent;
    public int minimapMode = 0; // Initial minimap mode is 0 (maximum size)

    private Rect[] minimapRect = {
        new Rect (0.55f, 0.55f, 0.4f, 0.4f), 
        new Rect (0.65f, 0.65f, 0.3f, 0.3f), 
        new Rect (0.75f, 0.75f, 0.2f, 0.2f), 
        new Rect (0.85f, 0.85f, 0.1f, 0.1f), 
        new Rect (0.95f, 0.95f, 0.0f, 0.0f)
    };

	// Use this for initialization
	void Start () {
        cameraComponent = GetComponent<Camera> ();
	}
	
	// Update is called once per frame
	void Update () {
        InputToggleMinimap ();
	}

    void InputToggleMinimap () {
        if (Input.GetKeyDown (KeyCode.M)) {
            ToggleMinimap ();
        }
    }

    void ToggleMinimap () {
        // Cycle minimap mode
        minimapMode = (minimapMode + 1) % minimapRect.Length;
        // Set camera rect based on current minimap mode
        cameraComponent.rect = minimapRect[minimapMode];
    }

}
