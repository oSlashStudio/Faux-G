using UnityEngine;
using System.Collections;
using PhotonPlayerExtension;

public class OutpostController : Photon.MonoBehaviour {

    public float influenceRate;
    public float maxInfluence;
    public float defaultTickDelay; // Delay between ticks
    public float scorePerTick; // Score given to the controlling team per tick

    private float tickDelay;

    [HideInInspector]
    public bool isControlled;
    [HideInInspector]
    public int controllingTeamId;
    [HideInInspector]
    public float currentInfluence;

    // Cached components
    private InGameNetworkManager networkManager;

	// Use this for initialization
	void Start () {
        networkManager = GameObject.FindObjectOfType<InGameNetworkManager> ();
        tickDelay = defaultTickDelay;
        isControlled = false;
        controllingTeamId = -1;
        currentInfluence = 0.0f;
	}

    void Update () {
        if (!photonView.isMine) {
            return;
        }
        if (!isControlled) {
            tickDelay = defaultTickDelay;
            return;
        }

        tickDelay -= Time.deltaTime;
        if (tickDelay <= 0.0f) {
            networkManager.AddScore (controllingTeamId, scorePerTick);
            tickDelay = defaultTickDelay;
        }
    }

    void OnTriggerStay2D (Collider2D collider) {
        if (!photonView.isMine) {
            return;
        }

        if (collider.tag == "Player") {
            PhotonPlayer player = collider.gameObject.GetComponent<PhotonView> ().owner;
            if (isControlled && controllingTeamId == player.CurrentTeamId ()) { // Case 1: currently controlling
                IncreaseInfluence (influenceRate * Time.fixedDeltaTime, player.CurrentTeamId ()); // Assert control
            } else if (!isControlled && (controllingTeamId == -1 || controllingTeamId == player.CurrentTeamId ())) { // Case 2: not controlling or is capturing
                IncreaseInfluence (influenceRate * Time.fixedDeltaTime, player.CurrentTeamId ()); // Capture
            } else { // Case 3: other possible scenarios
                ReduceInfluence (influenceRate * Time.fixedDeltaTime, player.CurrentTeamId ());
            }
        }
    }

    void IncreaseInfluence (float influenceIncrease, int teamId) {
        if (currentInfluence + influenceIncrease > maxInfluence) {
            currentInfluence = maxInfluence;
            isControlled = true;
            controllingTeamId = teamId;
        } else {
            currentInfluence += influenceIncrease;
            controllingTeamId = teamId;
        }
    }

    void ReduceInfluence (float influenceReduction, int teamId) {
        if (currentInfluence - influenceReduction < 0.0f) {
            currentInfluence = 0.0f;
            isControlled = false;
            controllingTeamId = -1;
        } else {
            currentInfluence -= influenceReduction;
        }
    }

}
