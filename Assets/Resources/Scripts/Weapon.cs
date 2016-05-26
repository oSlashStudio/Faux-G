using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

    public Sprite crosshairSprite;
    public GameObject projectilePrefab;
    public AudioClip fireSoundClip;

    // Throw related variables
    public bool isThrowable;
    public float maxThrowForce;
    public float throwForceIncreaseRate;

    // Fire related variables
    public float defaultFireDelay;
    public float maxSpreadAngle;
    public float recoil;
    public float knockbackForce;
    public bool canAim;
    public bool isHoming;
    public float homingSearchRadius;

}
