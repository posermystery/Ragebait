using UnityEngine;

public class HiddenSpike : MonoBehaviour
{
    [Header("Settings")]
    public float triggerDistance = 1.5f; // How close player needs to be to trigger
    public float popUpSpeed = 15f;
    public float popUpHeight = 1f;

    private Transform player;
    private bool isTriggered = false;
    private Vector3 hiddenPosition;
    private Vector3 targetPosition;

    void Start()
    {
        // Store the starting (hidden) position
        hiddenPosition = transform.position;
        // The position it will shoot up to
        targetPosition = hiddenPosition + Vector3.up * popUpHeight;
        
        // Find player
        PlatformerPlayer p = FindFirstObjectByType<PlatformerPlayer>();
        if (p != null)
        {
            player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Check distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isTriggered && distanceToPlayer <= triggerDistance)
        {
            // SURPRISE!
            isTriggered = true;
        }

        if (isTriggered)
        {
            // Quickly move the spike up
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, popUpSpeed * Time.deltaTime);
        }
    }
}
