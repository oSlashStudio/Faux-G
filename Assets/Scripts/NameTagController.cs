using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NameTagController : NetworkBehaviour {

    public GameObject nameTag;
    private TextMesh nameTagTextMesh;
    [SyncVar]
    public string playerName = "";

    public override void OnStartServer () {
        nameTagTextMesh = nameTag.GetComponent<TextMesh> ();
        // playerName = "Player " + connectionToClient.connectionId;
    }

    public override void OnStartClient () {
        nameTagTextMesh = nameTag.GetComponent<TextMesh> ();
    }

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if (nameTagTextMesh == null) {
            return;
        }
        nameTagTextMesh.text = playerName;
	}

    public void SetPlayerName (string designatedPlayerName) {
        playerName = designatedPlayerName;
        GameManagerController.Instance.RenamePlayer (connectionToClient.connectionId, designatedPlayerName);
    }

}
