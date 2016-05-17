using UnityEngine;
using System.Collections;

public class CrosshairController : MonoBehaviour {

    public float moveSpeed = 10.0f;
    public float accuracy = 1.0f; // Measure of accuracy based on recoil (1.0 is highest, 100% accurate)
    public float accuracyRecoveryRate = 1.0f;
    public float maxLocalScale = 10.0f;
    public float minLocalScale = 3.0f;
    public Camera referenceCamera;

    private Rigidbody2D rigidBody;

	// Use this for initialization
	void Start () {
        Cursor.visible = false;
        rigidBody = GetComponent<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {
        if (referenceCamera == null) {
            if (Camera.main != null) {
                referenceCamera = Camera.main;
            }
        }
        IncreaseAccuracy (accuracyRecoveryRate * Time.deltaTime);
        UpdateCrosshairScale ();
        UpdateCrosshairPosition ();
    }

    void OnDestroy () {
        Cursor.visible = true;
    }

    void UpdateCrosshairScale () {
        float currentScale = maxLocalScale - (maxLocalScale - minLocalScale) * accuracy;
        transform.localScale = new Vector3 (currentScale, currentScale, 1.0f);
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
    }

    public void IncreaseAccuracy (float accuracyIncrease) {
        if (accuracy + accuracyIncrease > 1.0f) {
            accuracy = 1.0f;
        } else {
            accuracy += accuracyIncrease;
        }
    }

    public void ReduceAccuracy (float accuracyReduction) {
        if (accuracy - accuracyReduction < 0.0f) {
            accuracy = 0.0f;
        } else {
            accuracy -= accuracyReduction;
        }
    }

}