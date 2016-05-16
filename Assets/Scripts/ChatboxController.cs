using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ChatboxController : NetworkBehaviour {

    public float chatBoxWidth = 480.0f;
    public float chatBoxHeight = 160.0f;
    public float chatBoxHorizonalOffset = 20.0f;
    public float chatBoxVerticalOffset = -40.0f;
    public float chatFieldWidth = 480.0f;
    public float chatFieldHeight = 20.0f;
    public float chatFieldHorizontalOffset = 20.0f;
    public float chatFieldVerticalOffset = -20.0f;

    [SyncVar]
    private string chatBoxString = "";

    private bool isChatFieldActive = false;
    private string chatFieldString = "";

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update () {

    }

    void OnGUI () {
        GUIStyle chatBoxStyle = GUI.skin.label;
        chatBoxStyle.alignment = TextAnchor.UpperLeft;

        GUI.SetNextControlName ("ChatBox");
        GUI.Label (new Rect (chatBoxHorizonalOffset, Screen.height - chatBoxHeight + chatBoxVerticalOffset,
            chatBoxWidth, chatBoxHeight), chatBoxString, chatBoxStyle);

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
        chatBoxString = chatMessage + "\n" + chatBoxString;
    }

}
