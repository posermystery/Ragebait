using UnityEngine;

public class Pipe : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float deadZone = -10f; // Screen left boundary

    void Update()
    {
        // Move the pipe left constantly
        transform.position = transform.position + (Vector3.left * moveSpeed) * Time.deltaTime;

        // Destroy pipe if it goes too far left (off-screen)
        if (transform.position.x < deadZone)
        {
            Destroy(gameObject);
        }
    }
}
