using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

    public Sprite crosshairSprite;
    public GameObject projectilePrefab;
    public AudioClip fireSoundClip;
    public float defaultFireDelay;
    public float maxSpreadAngle;
    public float recoil;
    public float knockbackForce;
    public bool canAim;
    public bool isHoming;
    public float homingSearchRadius;

}
