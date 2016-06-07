using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TeamData : System.Object {

    public int id;
    public string name;
    public Color color;
    public List<PhotonPlayer> members;
    public float score;

    public TeamData (int teamId, string teamName, Color teamColor) {
        id = teamId;
        name = teamName;
        color = teamColor;
        members = new List<PhotonPlayer> ();
    }

    public void AssignPlayer (PhotonPlayer player) {
        members.Add (player);
    }

}
