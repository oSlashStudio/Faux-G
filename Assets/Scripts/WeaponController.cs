using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class WeaponController : NetworkBehaviour {

    public GameObject weaponMuzzlePrefab;
    public GameObject rifleBulletPrefab;
    public GameObject rocketLauncherShellPrefab;
    public GameObject minigunBulletPrefab;
    public float defaultRifleFireDelay = 0.2f;
    public float defaultRocketLauncherFireDelay = 5.0f;
    public float defaultMinigunFireDelay = 0.1f;
    public float rifleMaxSpreadAngle = 5.0f;
    public float rocketLauncherMaxSpreadAngle = 0.0f;
    public float minigunMaxSpreadAngle = 10.0f;
    public float rifleRecoil = 2.0f;
    public float rocketLauncherRecoil = 5.0f;
    public float minigunRecoil = 1.0f;

    [SyncVar]
    public NetworkInstanceId playerNetId;

    private float rifleFireDelay = 0.0f;
    private float rocketLauncherFireDelay = 0.0f;
    private float minigunFireDelay = 0.0f;
    private int currentWeapon = 1; // Player starts with rifle as weapon (id 1)

    public override void OnStartClient () {
        
    }

    // Use this for initialization
    void Start () {
        GameObject player = ClientScene.FindLocalObject (playerNetId);
        transform.parent = player.transform;
        transform.localPosition = player.transform.up * 0.3f;
        player.GetComponent<PlayerController> ().weapon = gameObject;
        player.GetComponent<PlayerController> ().weaponController = gameObject.GetComponent<WeaponController> ();
    }

    // Update is called once per frame
    void Update () {
        if (!isServer) {
            return;
        }
        // Update fire delay based on time lapsed
        rifleFireDelay -= Time.deltaTime;
        rocketLauncherFireDelay -= Time.deltaTime;
        minigunFireDelay -= Time.deltaTime;
    }

    public void UpdateWeaponPosition () {
        transform.position = transform.parent.position + transform.parent.up * 0.3f;
    }

    public void UpdateWeaponDirection (Vector3 crosshairPosition) {
        transform.LookAt (new Vector3 (crosshairPosition.x,
                                       crosshairPosition.y,
                                       0.0f));
        transform.Rotate (new Vector3 (-90.0f, 0.0f, 0.0f));
    }

    [Command]
    public void CmdFire (Vector3 sourcePosition, Vector3 targetPosition) {
        switch (currentWeapon) {
            case 1:
                if (rifleFireDelay <= 0.0f) {
                    Fire (sourcePosition, targetPosition, rifleBulletPrefab, rifleMaxSpreadAngle);
                    RpcIntroduceRecoil (rifleRecoil);
                    rifleFireDelay = defaultRifleFireDelay;
                }
                break;
            case 2:
                if (rocketLauncherFireDelay <= 0.0f) {
                    Fire (sourcePosition, targetPosition, rocketLauncherShellPrefab, rocketLauncherMaxSpreadAngle);
                    RpcIntroduceRecoil (rocketLauncherRecoil);
                    rocketLauncherFireDelay = defaultRocketLauncherFireDelay;
                }
                break;
            case 3:
                if (minigunFireDelay <= 0.0f) {
                    Fire (sourcePosition, targetPosition, minigunBulletPrefab, minigunMaxSpreadAngle);
                    RpcIntroduceRecoil (minigunRecoil);
                    minigunFireDelay = defaultMinigunFireDelay;
                }
                break;
            default:
                break;
        }
    }

    void Fire (Vector3 sourcePosition, Vector3 targetPosition, GameObject projectilePrefab, float maxSpreadAngle) {
        // Bullet direction is characterized by the vector between crosshair and weapon muzzle
        Vector3 bulletDirectionVector = (targetPosition - sourcePosition).normalized;
        Quaternion bulletRotation = Quaternion.LookRotation (bulletDirectionVector);
        Vector3 bulletRotationVector = bulletRotation.eulerAngles;

        // Randomize projectile spread
        bulletRotationVector.x += Random.Range (-1.0f, 1.0f) * maxSpreadAngle;

        // Create projectile with appropriate position and rotation on the server
        GameObject projectile = (GameObject) Instantiate (projectilePrefab, sourcePosition,
            Quaternion.Euler (bulletRotationVector));
        // Create projectile on client
        NetworkServer.Spawn (projectile);
    }

    [ClientRpc]
    void RpcIntroduceRecoil (float recoil) {
        if (hasAuthority) {
            Vector2 displacement = (Random.insideUnitCircle).normalized * recoil;
            transform.parent.GetComponent<PlayerController> ().crosshair.transform.position += (Vector3) displacement;
        }
    }

    [Command]
    public void CmdChangeWeapon (int targetWeaponId) {
        if (currentWeapon != targetWeaponId) {
            currentWeapon = targetWeaponId;
        }
        RpcChangeWeapon (targetWeaponId);
    }

    [ClientRpc]
    void RpcChangeWeapon (int targetWeaponId) {
        if (currentWeapon != targetWeaponId) {
            currentWeapon = targetWeaponId;
        }
        switch (currentWeapon) {
            case 1:
                transform.localScale = new Vector3 (0.5f, 1.0f, 0.5f);
                break;
            case 2:
                transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
                break;
            case 3:
                transform.localScale = new Vector3 (0.25f, 1.0f, 0.25f);
                break;
            default:
                break;
        }
    }

}
