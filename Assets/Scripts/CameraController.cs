using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;
    public float yOffset = 3f; // Positive value means camera is higher than the player

    [Header("Follow Settings")]
    public float smoothTime = 0.3f;
    
    private Vector3 velocity = Vector3.zero;
    private Vector3 currentVirtualPos; // To track smooth position independent of shake

    [Header("Shake Settings")]
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

    void Start()
    {
        currentVirtualPos = transform.position;
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // Calculate where the camera should smoothly go
            Vector3 targetPosition = new Vector3(transform.position.x, player.position.y + yOffset, transform.position.z);
            
            // Apply SmoothDamp to our VIRTUAL position so the shake doesn't mess up the tracking logic
            currentVirtualPos = Vector3.SmoothDamp(currentVirtualPos, targetPosition, ref velocity, smoothTime);

            Vector3 finalPos = currentVirtualPos;

            // Apply Shake on top of the smooth tracking
            if (shakeDuration > 0)
            {
                finalPos += (Vector3)Random.insideUnitCircle * shakeMagnitude;
                shakeDuration -= Time.deltaTime;
            }

            transform.position = finalPos;
        }
    }

    public void TriggerShake(float duration = 0.5f, float magnitude = 0.5f)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}
