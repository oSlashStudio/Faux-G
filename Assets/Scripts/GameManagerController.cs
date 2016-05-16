using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameManagerController : NetworkBehaviour {

    public GameObject scoreboardPrefab;

    public override void OnStartServer () {
        GameObject scoreboard = (GameObject) Instantiate (scoreboardPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn (scoreboard);
    }

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
