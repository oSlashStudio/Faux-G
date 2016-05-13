using UnityEngine;
using System.Collections;

public class CrosshairController : MonoBehaviour {

    public float moveSpeed = 10.0f;

    private Rigidbody2D rigidBody;

	// Use this for initialization
	void Start () {
        Cursor.visible = false;
        rigidBody = GetComponent<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {

    }

    void OnDestroy () {
        Cursor.visible = true;
    }

    public void MoveTowards (Vector2 targetPosition) {
        rigidBody.MovePosition (targetPosition);
    }

}