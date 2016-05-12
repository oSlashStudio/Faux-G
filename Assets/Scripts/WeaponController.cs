using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour {

	public GameObject weaponMuzzlePrefab;
	public GameObject rifleBulletPrefab;
	public GameObject rocketLauncherShellPrefab;
    public GameObject minigunBulletPrefab;
    public GameObject crosshairPrefab;
	public float defaultRifleFireDelay = 0.2f;
	public float defaultRocketLauncherFireDelay = 5.0f;
    public float defaultMinigunFireDelay = 0.1f;
    public float rifleMaxSpreadAngle = 5.0f;
    public float rocketLauncherMaxSpreadAngle = 1.0f;
    public float minigunMaxSpreadAngle = 10.0f;

    private float rifleFireDelay = 0.0f;
	private float rocketLauncherFireDelay = 0.0f;
    private float minigunFireDelay = 0.0f;
    private GameObject crosshair;

	private int currentWeapon = 1; // Player starts with rifle as weapon (id 1)

	// Use this for initialization
	void Start () {
        crosshair = (GameObject) Instantiate (crosshairPrefab, transform.position, Quaternion.identity);
        crosshair.GetComponent<CrosshairController> ().pivot = weaponMuzzlePrefab;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 crosshairPosition = crosshair.transform.position;
		//mousePosition = Quaternion.Euler (0.0f, 0.0f, 0.0f) * mousePosition;
		transform.LookAt (new Vector3 (crosshairPosition.x,
                                       crosshairPosition.y, 
		                               0.0f));
		transform.Rotate (new Vector3 (-90.0f, 0.0f, 0.0f));

		// Update fire delay based on time lapsed
		rifleFireDelay -= Time.deltaTime;
		rocketLauncherFireDelay -= Time.deltaTime;
        minigunFireDelay -= Time.deltaTime;

		InputFire ();
		InputChangeWeapon ();
	}

	void InputFire () {
		if (Input.GetMouseButton (0)) { // Fire current weapon
			switch (currentWeapon) {
				case 1:
					if (rifleFireDelay <= 0.0f) {
						Fire ();
						rifleFireDelay = defaultRifleFireDelay;
					}
					break;
				case 2:
					if (rocketLauncherFireDelay <= 0.0f) {
						Fire ();
						rocketLauncherFireDelay = defaultRocketLauncherFireDelay;
					}
					break;
                case 3:
                    if (minigunFireDelay <= 0.0f) {
                        Fire ();
                        minigunFireDelay = defaultMinigunFireDelay;
                    }
                    break;
				default:
					break;
			}
		}
	}
	
	void Fire () {
		// Bullet direction is characterized by the vector between crosshair and weapon muzzle
		Vector3 bulletDirectionVector = (crosshair.transform.position - 
		                                 weaponMuzzlePrefab.transform.position).normalized;
        Quaternion bulletRotation = Quaternion.LookRotation (bulletDirectionVector);
        Vector3 bulletRotationVector = bulletRotation.eulerAngles;
        // Determine the projectile prefab based on current weapon id
        GameObject currentWeaponProjectilePrefab;
		switch (currentWeapon) {
			case 1:
				currentWeaponProjectilePrefab = rifleBulletPrefab;
                bulletRotationVector.x += Random.Range (-1.0f, 1.0f) * rifleMaxSpreadAngle;
                break;
			case 2:
				currentWeaponProjectilePrefab = rocketLauncherShellPrefab;
                bulletRotationVector.x += Random.Range (-1.0f, 1.0f) * rocketLauncherMaxSpreadAngle;
                break;
            case 3:
                currentWeaponProjectilePrefab = minigunBulletPrefab;
                bulletRotationVector.x += Random.Range (-1.0f, 1.0f) * minigunMaxSpreadAngle;
                break;
			default:
				currentWeaponProjectilePrefab = rifleBulletPrefab;
				break;
		}
		// Create projectile with appropriate position and rotation
		Instantiate (currentWeaponProjectilePrefab, weaponMuzzlePrefab.transform.position, 
		             Quaternion.Euler (bulletRotationVector));
	}

	void InputChangeWeapon () {
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			if (currentWeapon != 1) {
				transform.localScale = new Vector3 (0.5f, 1.0f, 0.5f);
				currentWeapon = 1;
			}
		} else if (Input.GetKeyDown (KeyCode.Alpha2)) {
			if (currentWeapon != 2) {
				transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
				currentWeapon = 2;
			}
		} else if (Input.GetKeyDown (KeyCode.Alpha3)) {
            if (currentWeapon != 3) {
                transform.localScale = new Vector3 (0.25f, 1.0f, 0.25f);
                currentWeapon = 3;
            }
        }
	}

}
