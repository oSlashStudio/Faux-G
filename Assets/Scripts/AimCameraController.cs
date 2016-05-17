using UnityEngine;
using System.Collections;

public class AimCameraController : MonoBehaviour {

    public GameObject crosshair;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = crosshair.transform.position + new Vector3 (0.0f, 0.0f, -8.0f);
	}
}
