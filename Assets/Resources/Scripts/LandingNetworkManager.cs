using UnityEngine;
using System.Collections;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;

public class LandingNetworkManager : Photon.PunBehaviour {

    private PlayerOnlineInfo playerOnlineInfo;

    private string inputUsername = "";
    private string inputPassword = "";

    private string errorMessage = "";

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

	}

    /*
     * Get a rectangle relative to full HD 1920:1080 screen
     */
    Rect RelativeRect (float x, float y, float w, float h) {
        float relativeX = Screen.width * x / 1920;
        float relativeY = Screen.height * y / 1080;
        float relativeW = Screen.width * w / 1920;
        float relativeH = Screen.height * h / 1080;

        return new Rect (relativeX, relativeY, relativeW, relativeH);
    }

    float RelativeWidth (float w) {
        float relativeW = Screen.width * w / 1920;

        return relativeW;
    }

    float RelativeHeight (float h) {
        float relativeH = Screen.height * h / 1080;

        return relativeH;
    }

    void OnGUI () {
        LoginKeyListener ();

        GUILayout.BeginArea (RelativeRect (560, 240, 800, 600));

        GUILayout.FlexibleSpace ();

        GUIStyle leftAlignedLabel = new GUIStyle (GUI.skin.label);
        leftAlignedLabel.alignment = TextAnchor.MiddleLeft;

        GUILayout.BeginHorizontal ();
        GUILayout.Label ("Username:", leftAlignedLabel, GUILayout.Width (RelativeWidth (300)));
        inputUsername = GUILayout.TextField (inputUsername);
        GUILayout.EndHorizontal ();

        GUILayout.BeginHorizontal ();
        GUILayout.Label ("Password:", leftAlignedLabel, GUILayout.Width (RelativeWidth (300)));
        inputPassword = GUILayout.PasswordField (inputPassword, '*');
        GUILayout.EndHorizontal ();

        GUILayout.BeginHorizontal ();
        if (GUILayout.Button ("Register")) {
            Register ();
        }
        GUILayout.FlexibleSpace ();
        if (GUILayout.Button ("Login")) {
            Login ();
        }
        GUILayout.EndHorizontal ();

        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace ();
        GUILayout.Label (errorMessage);
        GUILayout.FlexibleSpace ();
        GUILayout.EndHorizontal ();

        GUILayout.FlexibleSpace ();

        GUILayout.EndArea ();
    }

    void LoginKeyListener () {
        if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return)) {
            Login ();
        }
    }

    void Login () {
        PhotonNetwork.AuthValues = new AuthenticationValues ();
        PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Custom;

        // Setup SHA512 encoder
        SHA512 sha512 = new SHA512Managed ();
        // Encode to UTF8
        byte[] passwordBytes = Encoding.UTF8.GetBytes (inputPassword);
        // Hash password using SHA512
        byte[] passwordHashBytes = sha512.ComputeHash (passwordBytes);
        // Convert bytes to string
        string passwordHash = "";
        foreach (byte x in passwordHashBytes) {
            passwordHash += string.Format ("{0:x2}", x);
        }

        PhotonNetwork.AuthValues.AddAuthParameter ("username", inputUsername);
        PhotonNetwork.AuthValues.AddAuthParameter ("password_hash", passwordHash);

        PhotonNetwork.sendRate = 15;
        PhotonNetwork.sendRateOnSerialize = 15;
        PhotonNetwork.ConnectUsingSettings ("v0.1");
    }

    void Register () {
        Application.OpenURL ("http://oslash.studio/faux-g/register.php");
    }

    // Failed to log in
    public override void OnCustomAuthenticationFailed (string debugMessage) {
        errorMessage = debugMessage;
    }

    // Successfully logged in
    public override void OnCustomAuthenticationResponse (Dictionary<string, object> data) {
        PhotonNetwork.player.name = (string) data["username"];
        // Proceed to lobby
        PhotonNetwork.LoadLevel (1);
    }

}