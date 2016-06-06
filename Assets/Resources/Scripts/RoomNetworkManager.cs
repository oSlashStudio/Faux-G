using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using PhotonPlayerExtension;

public class RoomNetworkManager : Photon.PunBehaviour {

    private int selectedColorId = 0;
    private Color[] playerColors = new Color[] {
        new Color (1, 1, 1),  // White
        new Color (1, 0, 0), // Red
        new Color (0, 1, 0), // Green
        new Color (0, 0, 1), // Blue
        new Color (1, 1, 0), // Yellow
        new Color (1, 0, 1), // Pink
        new Color (0, 1, 1), // Cyan
        new Color (0, 0, 0) // Black
    };
    public Texture2D[] colorTextures;

    private int selectedClassId = 0;
    private string[] classNames = new string[] {
        "Mercenary",
        "Virtuoso",
        "The Maniac",
        "NASA Fanboy",
        "Spinal Holler",
        "R.O.B.O.T",
        "Gargantuan",
        "Crazy Shaman",
        "Heavy Bomber"
    };

    public Team[] teams;

    // Cached component
    private GUIStyle centeredLabel;

    /*
     * Get a rectangle relative to full HD 1920:1080 screen
     */
    Rect RelativeRect (float x, float y, float w, float h) {
        float relativeX = Screen.width * x / 1920;
        float relativeY = Screen.height * y / 1080;
        float relativeW = Screen.width * w / 1920;
        float relativeH = Screen.height * h / 1080;

        return new Rect (relativeX, relativeY, relativeW, relativeH);
    }

    float RelativeWidth (float w) {
        float relativeW = Screen.width * w / 1920;

        return relativeW;
    }

    float RelativeHeight (float h) {
        float relativeH = Screen.height * h / 1080;

        return relativeH;
    }

    void OnGUI () {
        // Initialize GUI Styles
        if (centeredLabel == null) {
            centeredLabel = new GUIStyle (GUI.skin.label);
            centeredLabel.alignment = TextAnchor.MiddleCenter;
        }

        if (!PhotonNetwork.inRoom) { // Trying to enter room
            EnteringRoomGUI ();
        } else {
            RoomGUI ();
        }
    }

    void EnteringRoomGUI () {
        GUILayout.BeginArea (RelativeRect (0, 0, 1920, 1080));
        {
            GUILayout.BeginHorizontal ();
            {
                GUILayout.FlexibleSpace (); // Left padding
                GUILayout.BeginVertical ();
                {
                    GUILayout.FlexibleSpace (); // Top padding
                    GUILayout.Label ("Trying to enter room", centeredLabel);
                    GUILayout.Label ("Please wait...", centeredLabel);
                    GUILayout.FlexibleSpace (); // Bottom padding
                }
                GUILayout.EndVertical ();
                GUILayout.FlexibleSpace (); // Right padding
            }
            GUILayout.EndHorizontal ();
        }
        GUILayout.EndArea ();
    }

    void RoomGUI () {
        GUILayout.BeginArea (RelativeRect (0, 0, 1920, 1080));
        {
            GUILayout.BeginHorizontal ();
            {
                GUILayout.FlexibleSpace (); // Left padding
                GUILayout.BeginVertical (GUILayout.Width (RelativeWidth (1280)));
                {
                    GUILayout.FlexibleSpace (); // Top padding
                    RoomInfoGUI ();
                    TeamsGUI ();
                    UnassignedGUI ();
                    ClassSelectionGUI ();
                    ControlGUI ();
                    GUILayout.FlexibleSpace (); // Bottom padding
                }
                GUILayout.EndVertical ();
                GUILayout.FlexibleSpace (); // Right padding
            }
            GUILayout.EndHorizontal ();
        }
        GUILayout.EndArea ();
    }

    void RoomInfoGUI () {
        GUILayout.BeginHorizontal (GUI.skin.box);
        GUILayout.Label (PhotonNetwork.room.name + " (" + PhotonNetwork.room.playerCount + " / " + PhotonNetwork.room.maxPlayers + ")", centeredLabel);
        GUILayout.EndHorizontal ();
    }

    void TeamsGUI () {
        GUILayout.BeginHorizontal ();
        {
            for (int i = 0; i < teams.Length; i++) {
                GUILayout.BeginVertical (GUI.skin.box, GUILayout.Width (RelativeWidth (1280.0f / teams.Length)));
                {
                    GUILayout.BeginHorizontal (GUI.skin.box);
                    {
                        GUILayout.FlexibleSpace ();
                        GUILayout.Label (teams[i].name);
                        if (PhotonNetwork.player.CurrentTeamId () != i) { // Not in this team yet
                            if (GUILayout.Button ("Join")) {
                                PhotonNetwork.player.LeaveTeam (); // Leave current team
                                PhotonNetwork.player.JoinTeam (i); // Join new team
                            }
                        } else {
                            GUI.enabled = false;
                            GUILayout.Button ("Join");
                            GUI.enabled = true;
                        }
                        GUILayout.FlexibleSpace ();
                    }
                    GUILayout.EndHorizontal ();

                    foreach (PhotonPlayer player in PhotonNetwork.playerList) {
                        if (player.CurrentTeamId () == i) { // Player is in this team
                            GUILayout.BeginHorizontal ();
                            {
                                GUILayout.FlexibleSpace ();

                                if (player.isMasterClient) {
                                    GUI.contentColor = Color.yellow;
                                    GUILayout.Label (player.name);
                                    GUI.contentColor = Color.white;
                                } else if (player.IsReady ()) { // Distinguish ready players
                                    GUI.contentColor = Color.green;
                                    GUILayout.Label (player.name);
                                    GUI.contentColor = Color.white;
                                } else {
                                    GUILayout.Label (player.name);
                                }

                                if (player.isLocal) { // Local player
                                    if (GUILayout.Button (colorTextures[selectedColorId], GUILayout.Width (RelativeWidth (200)))) {
                                        selectedColorId = (selectedColorId + 1) % playerColors.Length;
                                    }
                                } else if (PhotonNetwork.isMasterClient) {
                                    if (GUILayout.Button ("Kick")) { // Kick button
                                        KickPlayer (player);
                                    }
                                }

                                GUILayout.FlexibleSpace ();
                            }
                            GUILayout.EndHorizontal ();
                        }
                    }
                }
                GUILayout.EndVertical ();
            }
        }
        GUILayout.EndHorizontal ();
    }

    void UnassignedGUI () {
        GUILayout.BeginVertical (GUI.skin.box);
        {
            GUILayout.BeginHorizontal ();
            {
                GUILayout.Label ("Unassigned players:");
                if (PhotonNetwork.player.IsInATeam ()) { // Currently in a team
                    if (GUILayout.Button ("Join")) {
                        PhotonNetwork.player.LeaveTeam ();
                    }
                } else { // Not in a team
                    GUI.enabled = false;
                    GUILayout.Button ("Join");
                    GUI.enabled = true;
                }
                GUILayout.FlexibleSpace ();
            }
            GUILayout.EndHorizontal ();

            GUILayout.BeginVertical ();
            {
                foreach (PhotonPlayer player in PhotonNetwork.playerList) {
                    if (!player.IsInATeam ()) { // Not in a team yet
                        GUILayout.BeginHorizontal ();
                        {
                            if (player.isMasterClient) {
                                GUI.contentColor = Color.yellow;
                                GUILayout.Label (player.name);
                                GUI.contentColor = Color.white;
                            } else {
                                GUILayout.Label (player.name);
                            }

                            if (player.isLocal) { // Local player
                                if (GUILayout.Button (colorTextures[selectedColorId], GUILayout.Width (RelativeWidth (200)))) {
                                    selectedColorId = (selectedColorId + 1) % playerColors.Length;
                                }
                            } else if (PhotonNetwork.isMasterClient) {
                                if (GUILayout.Button ("Kick")) { // Kick button
                                    KickPlayer (player);
                                }
                            }
                            GUILayout.FlexibleSpace ();
                        }
                        GUILayout.EndHorizontal ();
                    }
                }
            }
            GUILayout.EndVertical ();
        }
        GUILayout.EndVertical ();
    }

    void ClassSelectionGUI () {
        GUILayout.Label ("Select Class:");
        selectedClassId = GUILayout.SelectionGrid (selectedClassId, classNames, 3);
    }

    void ControlGUI () {
        GUILayout.BeginHorizontal ();
        {
            if (GUILayout.Button ("Leave Room")) { // Leave room button
                LeaveRoom ();
            }
            if (PhotonNetwork.isMasterClient) {
                if (IsEveryoneReady ()) {
                    if (GUILayout.Button ("Start Game")) { // Start game button
                        StartGame ();
                    }
                } else {
                    GUI.enabled = false;
                    GUILayout.Button ("Start Game");
                    GUI.enabled = true;
                }
            } else {
                if (PhotonNetwork.player.IsInATeam ()) { // Currently in a team
                    if (!PhotonNetwork.player.IsReady ()) { // Not ready yet
                        if (GUILayout.Button ("Ready")) {
                            PhotonNetwork.player.Ready ();
                        }
                    } else { // Currently ready
                        if (GUILayout.Button ("Unready")) {
                            PhotonNetwork.player.Unready ();
                        }
                    }
                } else {
                    GUI.enabled = false;
                    GUILayout.Button ("Ready");
                    GUI.enabled = true;
                }
            }
        }
        GUILayout.EndHorizontal ();
    }

    void KickPlayer (PhotonPlayer kickedPlayer) {
        PhotonNetwork.CloseConnection (kickedPlayer);
    }

    bool IsEveryoneReady () {
        foreach (PhotonPlayer player in PhotonNetwork.playerList) {
            if (!player.IsInATeam ()) { // This player is not in a team
                return false;
            }
            if (player.isMasterClient) {
                continue;
            }
            if (!player.IsReady ()) { // This player is not ready yet
                return false;
            }
        }
        return true;
    }

    void LeaveRoom () {
        PhotonNetwork.LeaveRoom ();
    }

    public override void OnLeftRoom () {
        PhotonNetwork.LoadLevel (1);
    }

    void StartGame () {
        // Close access to the room when starting, this includes reconnection
        PhotonNetwork.room.open = false;
        PhotonNetwork.room.visible = false;
        // Prompts all clients to load in-game scene
        photonView.RPC ("RpcLoadLevel", PhotonTargets.All);
    }

    [PunRPC]
    void RpcLoadLevel () {
        // Set color and class
        ExitGames.Client.Photon.Hashtable playerHashTable = new ExitGames.Client.Photon.Hashtable ();
        playerHashTable["rColor"] = (byte) playerColors[selectedColorId].r;
        playerHashTable["gColor"] = (byte) playerColors[selectedColorId].g;
        playerHashTable["bColor"] = (byte) playerColors[selectedColorId].b;
        playerHashTable["class"] = (byte) selectedClassId;
        PhotonNetwork.player.SetCustomProperties (playerHashTable);
        // Temporarily pause message queue, to be resumed when the in-game scene loads
        PhotonNetwork.isMessageQueueRunning = false;
        // Load game scene
        PhotonNetwork.LoadLevel ((byte) PhotonNetwork.room.customProperties["game"]);
    }

}
