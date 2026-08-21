using UnityEngine;
using TMPro;

public class SplashTextAnimator : MonoBehaviour
{
    public TextMeshProUGUI splashText;
    
    [Header("Animation Settings")]
    public float bounceSpeed = 4f;       // Kitni tezi se dhak-dhak karega
    public float bounceAmount = 0.08f;   // Kitna bada-chota hoga
    
    [Header("Minecraft Style Quotes")]
    public string[] funnyQuotes = {
        "Prepare to rage!",
        "Don't smash your phone!",
        "Skill issue detected...",
        "100% Unfair!",
        "Keyboard sold separately!",
        "Is this too hard?",
        "Git Gud!",
        "Why are you doing this?",
        "Tears guaranteed!"
    };

    private Vector3 originalScale;

    void Start()
    {
        // Agar slot khali hai toh khud text component dhund lega
        if (splashText == null) 
            splashText = GetComponent<TextMeshProUGUI>();
        
        // Random message uthana
        if (splashText != null && funnyQuotes.Length > 0)
        {
            int randomIndex = Random.Range(0, funnyQuotes.Length);
            splashText.text = funnyQuotes[randomIndex];
        }
        
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Sine wave ka use karke continuous bouncing effect
        float pulse = Mathf.Sin(Time.time * bounceSpeed) * bounceAmount;
        transform.localScale = originalScale + new Vector3(pulse, pulse, 0);
    }
}
