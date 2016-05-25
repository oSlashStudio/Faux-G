using UnityEngine;
using System.Collections;

public class CalloutController : MonoBehaviour {

    public float maxLifeDuration = 2.0f;
    public float floatSpeed = 1.0f;

    private float lifeDuration;

    // Cached components
    private Rigidbody2D rigidBody;
    private TextMesh textMesh;

    public string Text {
        get {
            return textMesh.text;
        }
        set {
            textMesh.text = value;
        }
    }

    // Use this for initialization
    void Start () {
        rigidBody = GetComponent<Rigidbody2D> ();
        textMesh = GetComponent<TextMesh> ();

        lifeDuration = maxLifeDuration;
        rigidBody.velocity = transform.up * floatSpeed;
	}
	
	// Update is called once per frame
	void Update () {
        lifeDuration -= Time.deltaTime;
        if (lifeDuration <= 0.0f) {
            Destroy (gameObject);
        }

        UpdateAlpha ();
	}

    void UpdateAlpha () {
        textMesh.color = new Color (textMesh.color.r, textMesh.color.g, textMesh.color.b, lifeDuration / maxLifeDuration);
    }

}
