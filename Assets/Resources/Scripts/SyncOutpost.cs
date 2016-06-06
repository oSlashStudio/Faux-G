using UnityEngine;
using System.Collections;

public class SyncOutpost : Photon.MonoBehaviour {

    // Cached components
    private OutpostController outpostController;

    void Awake () {
        outpostController = GetComponent<OutpostController> ();
    }

	void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting) {
            stream.SendNext (outpostController.currentInfluence);
            stream.SendNext (outpostController.isControlled);
            stream.SendNext (outpostController.controllingTeamId);
        } else {
            outpostController.currentInfluence = (float) stream.ReceiveNext ();
            outpostController.isControlled = (bool) stream.ReceiveNext ();
            outpostController.controllingTeamId = (int) stream.ReceiveNext ();
        }
    }

}
