using UnityEngine;
using System.Collections;

public class NameTagController : Photon.MonoBehaviour {

    // Cached components
    private TextMesh textMesh;

	// Use this for initialization
	void Start () {
        textMesh = GetComponent<TextMesh> ();

        textMesh.text = photonView.owner.name;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
