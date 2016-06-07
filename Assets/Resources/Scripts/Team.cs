using UnityEngine;
using System.Collections.Generic;
using PhotonPlayerExtension;

[System.Serializable]
public class Team : System.Object {

    public int id;
    public string name;
    public int maxPlayers;
    public int colorId;

    public bool IsFull () {
        int playerCount = 0;
        foreach (PhotonPlayer player in PhotonNetwork.playerList) {
            if (player.CurrentTeamId () == id) {
                playerCount++;
            }
        }
        return playerCount == maxPlayers;
    }
    
}
