using UnityEngine;
using System.Collections;

public class CrosshairController : MonoBehaviour {

    public GameObject pivot;
    public float crosshairSpeed = 10.0f;
    public bool debugRaycast = true;

    private Rigidbody2D rigidBody;

	// Use this for initialization
	void Start () {
        Cursor.visible = false;
        rigidBody = GetComponent<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 mousePosition = (Vector2) Camera.main.ScreenToWorldPoint (Input.mousePosition);
        // Direction vector
        Vector2 moveDirectionVector = mousePosition - (Vector2) transform.position;
        // Speed towards target position is linearly related to distance
        rigidBody.MovePosition ((Vector2) transform.position + moveDirectionVector * crosshairSpeed * Time.deltaTime);

        if (pivot == null) {
            debugRaycast = false;
        }

        // Debug raycast for future laser guidance implementation
        DebugRaycast ();
    }

    void DebugRaycast () {
        if (debugRaycast) {
            Vector2 pivotPosition = (Vector2) pivot.transform.position;
            RaycastHit2D raycastInfo = Physics2D.Raycast (pivotPosition, (Vector2) transform.position - pivotPosition);
            Vector2 raycastHitPosition = raycastInfo.point;
            Vector2 directionVector = raycastHitPosition - pivotPosition;
            Debug.DrawRay (pivot.transform.position, directionVector, Color.green);
        }
    }

}