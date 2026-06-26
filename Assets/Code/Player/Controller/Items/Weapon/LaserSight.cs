using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSight : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float maxDistance = 100f;

    private void Update()
    {
        // Cast a ray forward from the empty game object's position
        Ray ray = new Ray(transform.position, transform.up);
        RaycastHit hit;

        // Check for collisions with objects in the scene
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // Set the Line Renderer's starting and ending positions
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            Vector3 endpoint = transform.position + transform.up * maxDistance;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, endpoint);
        }
    }
}

