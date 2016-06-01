using UnityEngine;
using System.Collections;

public class PlayerData {

    public string playerName;
    public int kill;
    public int death;
    public float damage;
    public float heal;
    public int killStreak;
    public int deathStreak;

    public PlayerData (string name) {
        this.playerName = name;
        kill = 0;
        death = 0;
        damage = 0.0f;
        heal = 0.0f;
        killStreak = 0;
        deathStreak = 0;
    }

    public void AddKill () {
        kill++;
        killStreak++;
        deathStreak = 0;
    }

    public void AddDeath () {
        death++;
        deathStreak++;
        killStreak = 0;
    }

    public void AddDamage (float damage) {
        this.damage += damage;
    }

    public void AddHeal (float heal) {
        this.heal += heal;
    }

}
