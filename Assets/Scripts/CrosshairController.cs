using UnityEngine;
using System.Collections;

public class CrosshairController : MonoBehaviour {

    public GameObject pivot;
    public bool debugRaycast = true;

	// Use this for initialization
	void Start () {
        Cursor.visible = false;
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 mousePosition = (Vector2) Camera.main.ScreenToWorldPoint (Input.mousePosition);
        transform.position = new Vector3 (mousePosition.x, mousePosition.y, -2.0f);

        // Debug raycast for future laser guidance implementation
        DebugRaycast (mousePosition);
    }

    void DebugRaycast (Vector2 mousePosition) {
        if (debugRaycast) {
            Vector2 pivotPosition = (Vector2) pivot.transform.position;
            RaycastHit2D raycastInfo = Physics2D.Raycast (pivotPosition, mousePosition - pivotPosition);
            Vector2 raycastHitPosition = raycastInfo.point;
            Vector2 directionVector = raycastHitPosition - pivotPosition;
            Debug.DrawRay (pivot.transform.position, directionVector, Color.green);
        }
    }

}