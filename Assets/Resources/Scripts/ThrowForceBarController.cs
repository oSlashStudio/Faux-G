using UnityEngine;
using System.Collections;

public class ThrowForceBarController : MonoBehaviour {

    private WeaponController weaponController;
    private SpriteRenderer spriteRenderer;

    // Use this for initialization
    void Start () {
        weaponController = GetComponentInParent<WeaponController> ();
        spriteRenderer = GetComponent<SpriteRenderer> ();
    }

    // Update is called once per frame
    void Update () {
        float maxThrowForce = weaponController.weapons[weaponController.currentWeapon].maxThrowForce;
        if (maxThrowForce == 0.0f) { // Divide by zero check
            maxThrowForce = 1.0f;
        }
        float throwForce = weaponController.throwForce;
        UpdateThrowForceBarScale (throwForce, maxThrowForce);
        UpdateThrowForceBarColor (throwForce, maxThrowForce);
    }

    void UpdateThrowForceBarScale (float throwForce, float maxThrowForce) {
        transform.localScale = new Vector3 (throwForce / maxThrowForce, 2 * throwForce / maxThrowForce, transform.localScale.z);
    }

    void UpdateThrowForceBarColor (float throwForce, float maxThrowForce) {
        if (throwForce / maxThrowForce <= 0.5f) {
            spriteRenderer.color = Color.Lerp (new Color (1.0f, 0.0f, 0.0f), new Color (1.0f, 1.0f, 0.0f), throwForce / maxThrowForce * 2.0f);
        } else {
            spriteRenderer.color = Color.Lerp (new Color (1.0f, 1.0f, 0.0f), new Color (0.0f, 1.0f, 0.0f), throwForce / maxThrowForce * 2.0f - 1.0f);
        }
    }

}
