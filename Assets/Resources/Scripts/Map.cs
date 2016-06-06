using UnityEngine;
using System.Collections;

[System.Serializable]
public class Map : System.Object {

    public string name;
    public byte roomSceneId;
    public byte gameSceneId;

    public override string ToString () {
        return name;
    }

}
