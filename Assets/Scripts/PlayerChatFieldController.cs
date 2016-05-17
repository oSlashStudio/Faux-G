using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerChatFieldController : NetworkBehaviour {

    public float chatFieldWidth = 480.0f;
    public float chatFieldHeight = 20.0f;
    public float chatFieldHorizontalOffset = 20.0f;
    public float chatFieldVerticalOffset = -20.0f;

    private bool isChatFieldActive = false;
    private string chatFieldString = "";

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update () {

    }

    void OnGUI () {
        if (!isLocalPlayer) {
            return;
        }

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return) {
            if (!isChatFieldActive) {
                isChatFieldActive = true;
            } else {
                CmdSendChatMessage (chatFieldString);
                chatFieldString = "";
                isChatFieldActive = false;
            }
        }

        if (isChatFieldActive) {
            GUI.SetNextControlName ("ChatField");
            chatFieldString = GUI.TextField (new Rect (chatFieldHorizontalOffset, Screen.height - chatFieldHeight + chatFieldVerticalOffset,
                chatFieldWidth, chatFieldHeight), chatFieldString, 32);
            GUI.FocusControl ("ChatField");
        }
    }

    [Command]
    void CmdSendChatMessage (string chatMessage) {
        ChatboxController.Instance.chatBoxString = ChatboxController.Instance.chatBoxString + "\n" + chatMessage;
    }

}