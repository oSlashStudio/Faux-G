using System.Collections.Generic;
using UnityEngine;
using System.Collections;

[RequireComponent (typeof (PhotonView))]
public class InRoomChat : Photon.MonoBehaviour {
    public Rect GuiRect = new Rect (0, 0, 250, 300);
    public bool IsVisible = true;
    public bool AlignBottom = false;
    public List<string> messages = new List<string> ();
    private string inputLine = "";
    private Vector2 scrollPos = Vector2.zero;

    public static readonly string ChatRPC = "Chat";

    // Cached components
    GUIStyle messageScrollViewStyle;
    GUIStyle messageLabelStyle;

    public void Start () {

    }

    public void OnGUI () {
        if (this.AlignBottom) {
            this.GuiRect.y = Screen.height - this.GuiRect.height;
        }

        if (messageScrollViewStyle == null) {
            messageScrollViewStyle = GUI.skin.scrollView;
            messageScrollViewStyle.alignment = TextAnchor.LowerLeft;
        }

        if (messageLabelStyle == null) {
            messageLabelStyle = GUI.skin.label;
            messageLabelStyle.alignment = TextAnchor.MiddleLeft;
        }

        if (!this.IsVisible || PhotonNetwork.connectionStateDetailed != PeerState.Joined) {
            return;
        }

        if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return)) {
            if (!string.IsNullOrEmpty (this.inputLine)) {
                this.photonView.RPC ("Chat", PhotonTargets.All, this.inputLine);
                this.inputLine = "";
                GUI.FocusControl ("");
                return; // printing the now modified list would result in an error. to avoid this, we just skip this single frame
            } else {
                GUI.FocusControl ("ChatInput");
            }
        }

        GUI.SetNextControlName ("");
        GUILayout.BeginArea (this.GuiRect);

        scrollPos = GUILayout.BeginScrollView (scrollPos, messageScrollViewStyle);
        GUILayout.FlexibleSpace ();
        for (int i = 0; i < messages.Count; i++) {
            GUILayout.Label (messages[i], messageLabelStyle);
            scrollPos += new Vector2 (scrollPos.x, scrollPos.y + messageLabelStyle.CalcHeight (GUIContent.none, GuiRect.width));
        }
        GUILayout.EndScrollView ();

        GUILayout.BeginHorizontal ();
        GUI.SetNextControlName ("ChatInput");
        inputLine = GUILayout.TextField (inputLine);
        if (GUILayout.Button ("Send", GUILayout.ExpandWidth (false))) {
            this.photonView.RPC ("Chat", PhotonTargets.All, this.inputLine);
            this.inputLine = "";
            GUI.FocusControl ("");
        }
        GUILayout.EndHorizontal ();
        GUILayout.EndArea ();
    }

    [PunRPC]
    public void Chat (string newLine, PhotonMessageInfo mi) {
        string senderName = "anonymous";

        if (mi != null && mi.sender != null) {
            if (!string.IsNullOrEmpty (mi.sender.name)) {
                senderName = mi.sender.name;
            } else {
                senderName = "player " + mi.sender.ID;
            }
        }

        this.messages.Add (senderName + ": " + newLine);
    }

    public void AddLine (string newLine) {
        this.messages.Add (newLine);
    }
}
