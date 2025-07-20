using System;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class DynamicObjectsTests : MonoBehaviour
{
[Header("Movement Settings")]
    [Tooltip("The speed at which the object moves.")]
    public float speed = 5.0f;

    [Header("Sphere Boundary Settings")]
    [Tooltip("The radius of the spherical boundary.")]
    public float sphereRadius = 10.0f;
    [Tooltip("The center of the spherical boundary (usually Vector3.zero for a 'close sphere').")]
    public Vector3 sphereCenter = Vector3.zero;

    [Header("Reflection Settings")]
    [Tooltip("How much randomness to add to the bounce direction (in degrees).")]
    [Range(0f, 45f)] // A reasonable range for randomness
    public float randomDeviationAngle = 15.0f; // Max deviation in degrees from perfect reflection

    public string uiID;

    private Vector3 currentDirection; // The current movement direction of the object

    void Start()
    {
        // Initialize with a random direction at the start
        currentDirection = Random.onUnitSphere;
        currentDirection.Normalize(); // Ensure it's a unit vector
    }

    public void AddListeners()
    {
        Generator.DestroyEvent.AddListener(DestroySelf);
    }

    public void DestroySelf()
    {
        Generator.ObjectDestroyedEvent.Invoke(this);
        DestroyImmediate(gameObject);
    }

    void Update()
    {
        // 1. Calculate the next position
        Vector3 nextPosition = transform.position + currentDirection * speed * Time.deltaTime;

        // 2. Check if the next position is outside the sphere boundary
        float distanceFromCenter = Vector3.Distance(sphereCenter, nextPosition);

        if (distanceFromCenter >= sphereRadius)
        {
            // The object has hit or gone outside the sphere.
            // First, correct its position to be exactly on the surface.
            Vector3 directionToCurrent = (nextPosition - sphereCenter).normalized;
            transform.position = sphereCenter + directionToCurrent * sphereRadius;

            // 3. Calculate the normal of the surface at the point of impact
            // For a sphere, the normal is simply the normalized vector from the center to the point on the surface.
            Vector3 surfaceNormal = (transform.position - sphereCenter).normalized;

            // 4. Reflect the current direction off the surface normal
            Vector3 reflectedDirection = Vector3.Reflect(currentDirection, surfaceNormal);

            // 5. Add randomness to the reflected direction
            // We create a random rotation around the reflected direction's axis or a perpendicular axis
            // A simple way is to generate a small random Euler angle perturbation
            Quaternion randomRotation = Quaternion.Euler(
                Random.Range(-randomDeviationAngle, randomDeviationAngle),
                Random.Range(-randomDeviationAngle, randomDeviationAngle),
                Random.Range(-randomDeviationAngle, randomDeviationAngle)
            );

            // Apply the random rotation to the reflected direction
            currentDirection = randomRotation * reflectedDirection;
            currentDirection.Normalize(); // Re-normalize to ensure it's still a unit vector

            // Optional: If you want a more "sticky" bounce, you might reverse the direction for a single frame
            // to ensure it fully exits the "outside" state, though the position correction usually handles this.
            // currentDirection = -currentDirection; // Not usually needed with position correction and reflection
        }
        else
        {
            // If not hitting the edge, just continue moving
            transform.position = nextPosition;
        }
    }

    // --- Visualization in the Editor ---
    void OnDrawGizmos()
    {
        // Draw the sphere boundary in the Unity Editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(sphereCenter, sphereRadius);

        // Draw the current direction of the object
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, currentDirection * 2f); // Draw a ray representing direction
    }
}
