using UnityEngine;
using System.Collections;

public class CrosshairController : MonoBehaviour {

    public float moveSpeed = 10.0f;
    public float accuracy = 1.0f; // Measure of accuracy based on recoil (1.0 is highest, 100% accurate)
    public float accuracyRecoveryRate = 0.5f;
    public float maxLocalScale = 6.0f;
    public float minLocalScale = 3.0f;

    // Cached components
    public Camera referenceCamera;
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Start () {
        referenceCamera = Camera.main;
        rigidBody = GetComponent<Rigidbody2D> ();
        spriteRenderer = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
        Cursor.visible = false;

        IncreaseAccuracy (accuracyRecoveryRate * Time.deltaTime);
        UpdateCrosshairScale ();
        UpdateCrosshairColor ();
        UpdateCrosshairPosition ();
        if (referenceCamera != null) {
            transform.rotation = referenceCamera.transform.rotation;
        }
    }

    void OnDestroy () {
        Cursor.visible = true;
    }

    void UpdateCrosshairScale () {
        float currentScale = maxLocalScale - (maxLocalScale - minLocalScale) * accuracy;
        transform.localScale = new Vector3 (currentScale, currentScale, 1.0f);
    }

    void UpdateCrosshairColor () {
        if (accuracy <= 0.5f) { // From 0 to 0.5, lerps from red to yellow
            spriteRenderer.color = Color.Lerp (Color.red, Color.yellow, accuracy * 2);
        } else { // From 0.5 to 1.0, lerps from yellow to green
            spriteRenderer.color = Color.Lerp (Color.yellow, Color.green, accuracy * 2 - 1.0f);
        }
    }

    void UpdateCrosshairPosition () {
        if (referenceCamera == null) {
            return;
        }
        Vector2 mousePosition = referenceCamera.ScreenToWorldPoint (Input.mousePosition);
        // Calculate direction vector
        Vector2 moveDirectionVector = mousePosition - (Vector2) transform.position;
        // Calculate target position vector
        Vector2 targetPosition = (Vector2) transform.position + moveDirectionVector * moveSpeed * Time.deltaTime;
        // Move crosshair towards target position
        MoveTowards (targetPosition);
    }

    public void MoveTowards (Vector2 targetPosition) {
        rigidBody.MovePosition (targetPosition);
        // Normalize crosshair z-position to -2.0f
        transform.position = new Vector3 (transform.position.x, transform.position.y, -2.0f);
    }

    public void IncreaseAccuracy (float accuracyIncrease) {
        // Special case: accuracy is more than 1 after increase
        if (accuracy + accuracyIncrease > 1.0f) {
            accuracy = 1.0f;
        } else {
            accuracy += accuracyIncrease;
        }
    }

    public void ReduceAccuracy (float accuracyReduction) {
        // Special case: accuracy is less than zero after reduction
        if (accuracy - accuracyReduction < 0.0f) {
            accuracy = 0.0f;
        } else {
            accuracy -= accuracyReduction;
        }
    }

}