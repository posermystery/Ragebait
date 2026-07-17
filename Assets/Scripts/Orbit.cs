using UnityEngine;

public class Orbit : MonoBehaviour
{
    [Header("Orbit Settings")]
    public float rotationSpeed = 180f; // Degrees per second
    public bool isClockwise = true;

    // Dynamically get the radius based on the collider so it always matches the visual size
    public float radius 
    { 
        get 
        { 
            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col != null)
                return col.bounds.extents.x; 
            return 2f; 
        } 
    }

    // To visualize the orbit in the Scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
