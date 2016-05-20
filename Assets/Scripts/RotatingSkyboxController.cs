using UnityEngine;
using System.Collections;

public class RotatingSkyboxController : MonoBehaviour {

    public float angularSpeed = -0.5f;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate (new Vector3 (0.0f, angularSpeed * Time.deltaTime, 0.0f));
	}
}
