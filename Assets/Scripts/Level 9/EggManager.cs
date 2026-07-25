using UnityEngine;
using UnityEngine.UI;

public class EggManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform cameraTransform;
    public Text scoreText;
    
    [Header("Camera Settings")]
    public float yOffset = 2.5f; // Position camera above egg so egg stays in lower half of screen
    
    private float highestTargetY = 0f;
    private float highestY = 0f;
    private int score = 0;

    void Start()
    {
        if (cameraTransform == null)
        {
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogError("[EggManager] No Camera assigned or tagged as MainCamera!");
                return;
            }
        }

        if (player != null)
        {
            highestY = player.position.y;
            highestTargetY = player.position.y + yOffset;
            cameraTransform.position = new Vector3(cameraTransform.position.x, highestTargetY, cameraTransform.position.z);
        }
    }

    void LateUpdate()
    {
        if (player != null && cameraTransform != null)
        {
            float targetY = player.position.y + yOffset;
            
            // Camera only moves upwards (never downwards, so falling means death)
            if (targetY > highestTargetY)
            {
                highestTargetY = targetY;
            }

            // Direct lock in LateUpdate completely prevents camera lag when jumping fast
            cameraTransform.position = new Vector3(cameraTransform.position.x, highestTargetY, cameraTransform.position.z);
            
            // Calculate score (each basket is spaced roughly 4 units apart)
            if (player.position.y > highestY)
            {
                highestY = player.position.y;
                int newScore = Mathf.FloorToInt((highestY - (-3.5f)) / 4f); 
                if (newScore > score)
                {
                    score = newScore;
                    if (scoreText != null)
                        scoreText.text = "Baskets: " + score;
                }
            }
        }
    }
}
