using UnityEngine;
using System.Collections;

using Prototype.NetworkLobby;
using UnityEngine.Networking;

/*
 * This class acts as a "hook" between the lobbyPlayer object and the gamePlayer object, all informations contained within lobbyPlayer 
 * can be imported onto gamePlayer object through the method OnLobbyServerSceneLoadedForPlayer ().
 */
public class LobbyPlayerHook : LobbyHook {

    public override void OnLobbyServerSceneLoadedForPlayer (NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer) {
        gamePlayer.GetComponent<NameTagController> ().playerName = lobbyPlayer.GetComponent<LobbyPlayer> ().playerName;
        gamePlayer.GetComponent<PlayerController> ().playerColor = lobbyPlayer.GetComponent<LobbyPlayer> ().playerColor;
    }

}
