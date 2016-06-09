using UnityEngine;
using System.Collections;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Net;

public class LandingNetworkManager : Photon.PunBehaviour {

    private PlayerOnlineInfo playerOnlineInfo;

    private string inputUsername = "";
    private string inputPassword = "";

    private string displayMessage = "";

    private int selectedRegionId = 0;
    private string[] regions = new string[] {
        "Asia",
        "Australia",
        "Europe",
        "Japan",
        "US"
    };
    private string[] regionsAndPings = new string[] {
        "Asia",
        "Australia",
        "Europe",
        "Japan",
        "US"
    };
    private CloudRegionCode[] regionCode = new CloudRegionCode[] {
        CloudRegionCode.asia,
        CloudRegionCode.au,
        CloudRegionCode.eu,
        CloudRegionCode.jp,
        CloudRegionCode.us
    };
    private string[] regionHost = new string[] {
        "app-asia.exitgamescloud.com",
        "app-au.exitgamescloud.com",
        "app-eu.exitgamescloud.com",
        "app-jp.exitgamescloud.com",
        "app-us.exitgamescloud.com"
    };

    // Use this for initialization
    void Start () {

    }

    IEnumerator UpdatePing () {
        Ping[] pings = new Ping[5];
        for (int i = 0; i < regionHost.Length; i++) {
            pings[i] = new Ping (GetIpAddress (regionHost[i]).ToString ());
        }
        yield return new WaitForSeconds (1.0f); // Wait for 1s
        for (int i = 0; i < regionHost.Length; i++) {
            if (pings[i].time == -1) {
                regionsAndPings[i] = regions[i] + " (Timeout)";
            } else {
                regionsAndPings[i] = regions[i] + " (" + pings[i].time + " ms)";
            }
        }
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

        GUILayout.BeginArea (RelativeRect (0, 0, 1920, 1080));

        GUILayout.BeginHorizontal ();
        {
            GUILayout.FlexibleSpace (); // Left padding

            GUILayout.BeginVertical (GUILayout.Width (RelativeWidth (800)));
            {
                GUILayout.FlexibleSpace (); // Top padding

                GUIStyle leftAlignedLabel = new GUIStyle (GUI.skin.label);
                leftAlignedLabel.alignment = TextAnchor.MiddleLeft;

                GUILayout.BeginHorizontal ();
                {
                    GUILayout.Label ("Username:", leftAlignedLabel);
                    GUI.SetNextControlName ("inputUsername");
                    inputUsername = GUILayout.TextField (inputUsername, 32, GUILayout.Width (RelativeWidth (400)));
                    if (inputUsername == "") {
                        GUI.FocusControl ("inputUsername"); // Focus while empty
                    }
                }
                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();
                {
                    GUILayout.Label ("Password:", leftAlignedLabel);
                    inputPassword = GUILayout.PasswordField (inputPassword, '*', 32, GUILayout.Width (RelativeWidth (400)));
                }
                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();
                {
                    selectedRegionId = GUILayout.SelectionGrid (selectedRegionId, regionsAndPings, 3);
                }
                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();
                {
                    if (GUILayout.Button ("Check Ping")) {
                        StartCoroutine (UpdatePing ());
                    }
                    GUILayout.FlexibleSpace ();
                    if (GUILayout.Button ("Register")) {
                        Register ();
                    }
                    if (GUILayout.Button ("Login")) {
                        Login ();
                    }
                }
                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();
                {
                    GUILayout.FlexibleSpace ();
                    GUILayout.Label (displayMessage);
                    GUILayout.FlexibleSpace ();
                }
                GUILayout.EndHorizontal ();

                GUILayout.FlexibleSpace (); // Bottom padding
            }
            GUILayout.EndVertical ();

            GUILayout.FlexibleSpace (); // Right padding
        }
        GUILayout.EndHorizontal ();

        GUILayout.EndArea ();
    }

    void LoginKeyListener () {
        if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return)) {
            Login ();
        }
    }

    void Login () {
        displayMessage = "Logging in...";

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
        PhotonNetwork.ConnectToRegion (regionCode[selectedRegionId], "v0.1");
    }

    void Register () {
        Application.OpenURL ("http://oslash.studio/faux-g/register.php");
    }

    // Failed to log in
    public override void OnCustomAuthenticationFailed (string debugMessage) {
        displayMessage = debugMessage;
    }

    // Successfully logged in
    public override void OnCustomAuthenticationResponse (Dictionary<string, object> data) {
        displayMessage = "Logged in successfully";
        PhotonNetwork.player.name = (string) data["username"];

        ExitGames.Client.Photon.Hashtable sessionKeyHashtable = new ExitGames.Client.Photon.Hashtable ();
        sessionKeyHashtable["sessionKey"] = (string) data["session_key"];
        PhotonNetwork.player.SetCustomProperties (sessionKeyHashtable);

        // Proceed to lobby
        PhotonNetwork.LoadLevel (1);
    }

    IPAddress GetIpAddress (string hostName) {
        IPHostEntry host;
        host = Dns.GetHostEntry (hostName);

        if (host.AddressList.Length == 0) {
            return null;
        }
        return host.AddressList[0];
    }

}