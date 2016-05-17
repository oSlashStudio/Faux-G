using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ChatboxController : NetworkBehaviour {

    public float chatBoxWidth = 480.0f;
    public float chatBoxHeight = 160.0f;
    public float chatBoxHorizonalOffset = 20.0f;
    public float chatBoxVerticalOffset = -40.0f;

    [SyncVar]
    public string chatBoxString = "";

    public static ChatboxController Instance { get; private set; }

    public override void OnStartServer () {
        Instance = this;
    }

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update () {

    }

    void OnGUI () {
        GUIStyle chatBoxStyle = GUI.skin.label;
        chatBoxStyle.alignment = TextAnchor.LowerLeft;

        GUI.SetNextControlName ("ChatBox");
        GUI.Label (new Rect (chatBoxHorizonalOffset, Screen.height - chatBoxHeight + chatBoxVerticalOffset,
            chatBoxWidth, chatBoxHeight), chatBoxString, chatBoxStyle);
    }

}
