using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkManagerHUDController : NetworkBehaviour {

    public NetworkManagerHUD networkManagerHUD;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        InputToggleShowGUI ();
	}

    void InputToggleShowGUI () {
        if (Input.GetKeyDown (KeyCode.N)) {
            ToggleShowGUI ();
        }
    }

    void ToggleShowGUI () {
        networkManagerHUD.showGUI = !networkManagerHUD.showGUI;
    }
}
