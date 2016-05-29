using UnityEngine;
using System.Collections.Generic;

public class ThrowableController : MonoBehaviour {

    public GameObject explosionPrefab;
    
    public float throwableLifetime;
    public bool isExplodingOnCollision;

    // Clustering related variables
    public bool isClustered;
    public GameObject clusterPrefab;
    public int numClusters;
    public float clusteringForce; // The force applied to each cluster after clustering event

    private float haloBlinkDelay;
    private bool isHaloEnabled;

    // Owner information variables
    private bool isPlayerInstantiated = false;
    private int instantiatorId;

    public int InstantiatorId {
        get {
            return instantiatorId;
        }
        set {
            isPlayerInstantiated = true;
            instantiatorId = value;
        }
    }

    // Cached components
    private Component halo;

    // Use this for initialization
    void Start () {
        halo = GetComponent ("Halo");
    }
	
	// Update is called once per frame
	void Update () {
        throwableLifetime -= Time.deltaTime;
        if (throwableLifetime <= 0.0f) {
            GameObject explosion = (GameObject) Instantiate (explosionPrefab, transform.position, Quaternion.identity);

            if (isPlayerInstantiated) {
                explosion.GetComponent<ExplosionController> ().InstantiatorId = instantiatorId;
            }

            if (isClustered) {
                InstantiateClusters ();
            }

            Destroy (gameObject);
        }

        haloBlinkDelay -= Time.deltaTime;
        if (haloBlinkDelay <= 0.0f) {
            ToggleHalo ();
            haloBlinkDelay = throwableLifetime / 6.0f;
        }
	}

    void ToggleHalo () {
        isHaloEnabled = !isHaloEnabled;
        halo.GetType ().GetProperty ("enabled").SetValue (halo, isHaloEnabled, null);
    }

    void OnCollisionEnter2D (Collision2D collision) {
        if (!isExplodingOnCollision) { // Not exploding on collision, ignore collision event
            return;
        }

        GameObject explosion = (GameObject) Instantiate (explosionPrefab, transform.position, Quaternion.identity);

        if (isPlayerInstantiated) {
            explosion.GetComponent<ExplosionController> ().InstantiatorId = instantiatorId;
        }

        if (isClustered) {
            InstantiateClusters ();
        }

        Destroy (gameObject);
    }

    void InstantiateClusters () {
        Random.seed = Mathf.RoundToInt ((float) PhotonNetwork.time); // This syncs randomization throughout the network
        List<GameObject> instantiatedClusters = new List<GameObject> ();

        if (numClusters % 2 == 1) { // Odd number of clusters
            for (int i = -numClusters / 2; i <= numClusters / 2; i++) {
                Quaternion instantiateRotation = Quaternion.Euler (new Vector3 (0.0f, 0.0f, Random.Range (0.0f, 360.0f)));
                Vector3 instantiatePosition = transform.position;
                InstantiateCluster (instantiatePosition, instantiateRotation, ref instantiatedClusters);
            }
        } else { // Even number of clusters
            for (float i = -numClusters / 2 + 0.5f; i <= numClusters / 2 - 0.5f; i += 1.0f) {
                Quaternion instantiateRotation = Quaternion.Euler (new Vector3 (0.0f, 0.0f, Random.Range (0.0f, 360.0f)));
                Vector3 instantiatePosition = transform.position;
                InstantiateCluster (instantiatePosition, instantiateRotation, ref instantiatedClusters);
            }
        }
    }

    void InstantiateCluster (Vector3 instantiatePosition, Quaternion instantiateRotation, ref List<GameObject> instantiatedClusters) {
        GameObject cluster = (GameObject) Instantiate (clusterPrefab, instantiatePosition, instantiateRotation);

        if (isPlayerInstantiated) {
            cluster.GetComponent<ThrowableController> ().InstantiatorId = instantiatorId;
        }

        // Ignore collision with clustering object
        Physics2D.IgnoreCollision (cluster.GetComponent<Collider2D> (), gameObject.GetComponent<Collider2D> ());

        // Ignore collisions with instantiated clusters
        foreach (GameObject instantiatedCluster in instantiatedClusters) {
            Physics2D.IgnoreCollision (cluster.GetComponent<Collider2D> (), instantiatedCluster.GetComponent<Collider2D> ());
        }
        instantiatedClusters.Add (cluster); // Add as instantiated cluster

        cluster.GetComponent<Rigidbody2D> ().AddForce (instantiateRotation * Vector3.right * clusteringForce);
    }

}
