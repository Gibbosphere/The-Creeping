using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorScript : MonoBehaviour
{
    public Renderer floorRenderer;
    public float tileSize = 1f; // Adjust this as needed

    void Start()
    {
        if (floorRenderer == null)
        {
            floorRenderer = GetComponent<Renderer>();
        }

        if (floorRenderer != null)
        {
            // Calculate the tiling based on the object’s scale
            Vector3 objectScale = transform.localScale;
            floorRenderer.material.mainTextureScale = new Vector2(objectScale.x / tileSize, objectScale.z / tileSize);
        }
    }
}
