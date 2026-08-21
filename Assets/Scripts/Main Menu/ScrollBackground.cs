using UnityEngine;
using UnityEngine.UI;

public class ScrollBackground : MonoBehaviour
{
    [Header("UI Component")]
    public RawImage bgImage;

    [Header("Scroll Speeds")]
    [Tooltip("Positive for right, negative for left")]
    public float scrollSpeedX = 0.05f; 
    public float scrollSpeedY = 0f;

    void Update()
    {
        if (bgImage != null)
        {
            // UV Rect ki X position ko dhire dhire badhao jisse image move hoti hui lage
            Rect rect = bgImage.uvRect;
            rect.x += scrollSpeedX * Time.deltaTime;
            rect.y += scrollSpeedY * Time.deltaTime;
            bgImage.uvRect = rect;
        }
    }
}
