using UnityEngine;
using System.Collections;

public class FogLighter : Photon.MonoBehaviour {

    public int visionRadius;

    // Cached components
    private GameObject fogOfWar;
    private Color32[] defaultTexturePixels;
    private Color[] colorArray;

	// Use this for initialization
	void Start () {
        if (!photonView.isMine) { // Not local player, don't reveal fog
            enabled = false;
            return;
        }

        fogOfWar = GameObject.FindGameObjectWithTag ("Fog of War");
        if (fogOfWar == null) {
            enabled = false;
            return;
        }

        Texture2D initialTexture = (Texture2D) fogOfWar.GetComponent<Renderer> ().material.mainTexture;
        defaultTexturePixels = initialTexture.GetPixels32 ();

        InitializeColorArray ();
        StartCoroutine (PeriodicIlluminate ());
    }
    
    void InitializeColorArray () {
        int diameter = visionRadius * 2;
        colorArray = new Color[diameter * diameter + 1];
        for (int y = 0; y < diameter; y++) {
            for (int x = 0; x < diameter; x++) {
                float alpha = squaredDistance (x, y, visionRadius, visionRadius) / (visionRadius * visionRadius);
                colorArray[y * diameter + x] = new Color (0, 0, 0, alpha);
            }
        }
    }

    IEnumerator PeriodicIlluminate () {
        while (true) {
            Illuminate ();
            yield return new WaitForSeconds (0.1f);
        }
    }

    void Illuminate () {
        RaycastHit hitInfo;
        Physics.Raycast (transform.position, -Vector3.forward, out hitInfo);
        if (hitInfo.transform == null) {
            return;
        }
        Renderer renderer = hitInfo.transform.GetComponentInParent<Renderer> ();
        MeshCollider meshCollider = hitInfo.transform.GetComponentInParent<MeshCollider> ();
        if (renderer == null || renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null || meshCollider == null) {
            return;
        }
        Texture2D texture = (Texture2D) renderer.material.mainTexture;
        // Reset to default texture before revealing parts of fog
        texture.SetPixels32 (defaultTexturePixels);

        Vector2 centerPixel = hitInfo.textureCoord;
        centerPixel.x = (1 - centerPixel.x) * texture.width;
        centerPixel.y *= texture.height;

        int startY = (int) centerPixel.y - visionRadius;
        int startX = (int) centerPixel.x - visionRadius;

        Color[] newTextureColorArray = texture.GetPixels (startX, startY, visionRadius * 2, visionRadius * 2);
        for (int i = 0; i < newTextureColorArray.Length; i++) {
            newTextureColorArray[i] *= colorArray[i];
        }
        texture.SetPixels (startX, startY, visionRadius * 2, visionRadius * 2, newTextureColorArray);

        texture.Apply ();
    }

    float squaredDistance (float x1, float y1, float x2, float y2) {
        return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
    }

}
