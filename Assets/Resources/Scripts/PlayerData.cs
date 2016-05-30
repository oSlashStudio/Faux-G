using UnityEngine;
using System.Collections;

public class PlayerData : MonoBehaviour {

    public string playerName;
    public int kill;
    public int death;
    public float damage;
    public float heal;

    public PlayerData (string name) {
        this.playerName = name;
        kill = 0;
        death = 0;
        damage = 0.0f;
        heal = 0.0f;
    }

    public void AddKill () {
        kill++;
    }

    public void AddDeath () {
        death++;
    }

    public void AddDamage (float damage) {
        this.damage += damage;
    }

    public void AddHeal (float heal) {
        this.heal += heal;
    }

}
