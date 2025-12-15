using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicProjectileWatcher2D : MonoBehaviour
{
    // Rotation speed in degrees per second
    public float rotationSpeed = 90f;

    // Reference to the current projectile
    private GameObject projectile;

    void Update()
    {
        // If we don't already have a projectile, try to find it
        if (projectile == null)
        {
            projectile = GameObject.FindGameObjectWithTag("Projectile");
            if (projectile != null)
            {
                Debug.Log("Projectile spawned and detected!");
            }
        }

        // If a projectile exists, check if it's moving and apply rotation
        if (projectile != null && IsMoving(projectile))
        {
            RotateObject(projectile);
        }
    }

    // Function to check if the projectile is moving
    private bool IsMoving(GameObject obj)
    {
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Check if the Rigidbody2D's velocity magnitude is above a small threshold
            return rb.velocity.magnitude > 0.01f;
        }
        else
        {
            Debug.LogWarning($"Projectile {obj.name} does not have a Rigidbody2D component!");
            return false;
        }
    }

    // Function to rotate the projectile along the Z-axis
    private void RotateObject(GameObject obj)
    {
        obj.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}