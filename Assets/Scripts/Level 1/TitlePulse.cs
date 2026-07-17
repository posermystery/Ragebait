using UnityEngine;
using UnityEngine.UI;

public class TitlePulse : MonoBehaviour
{
    private Outline outline;
    
    [Header("Pulse Settings")]
    public Color normalColor = Color.black; // Jo color abhi hai
    public Color flashColor = Color.red;    // Jo chamkega
    public float flashSpeed = 5f;           // Chamakne ki speed

    void Start()
    {
        // Outline component dhoondho
        outline = GetComponent<Outline>();
        
        // Agar Outline nahi laga hua toh khud laga do
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(2, -2);
        }
    }

    void Update()
    {
        // PingPong function 0 se 1 aur 1 se 0 ke beech me ghoomta hai lagatar
        float lerpValue = Mathf.PingPong(Time.time * flashSpeed, 1f);
        
        // Outline ke color ko do colors ke beech me mix (fade) karo
        outline.effectColor = Color.Lerp(normalColor, flashColor, lerpValue);
    }
}
