using UnityEngine;

public class FlappyPlayer : MonoBehaviour
{
    private Rigidbody2D rb;
    public float jumpForce = 5f;
    private bool isDead = false;
    
    private int pipesPassed = 0;
    public int pipesToWin = 5; // Win after 5 safe pipes

    public AudioClip jumpSound;
    [Range(0f, 1f)] public float jumpVolume = 0.1f; // Default volume is low so it doesn't hurt ears
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (isDead) return;

        // Tapping / Clicking makes the bird fly up (New Input System compatible)
        if (UnityEngine.InputSystem.Pointer.current != null && UnityEngine.InputSystem.Pointer.current.press.wasPressedThisFrame)
        {
            rb.linearVelocity = Vector2.up * jumpForce;
            if (jumpSound != null) audioSource.PlayOneShot(jumpSound, jumpVolume);
        }

        // --- NEW: Flappy Bird Rotation Logic ---
        // Upar ja raha hai toh upar dekhega (30 degrees), neeche gir raha hai toh neeche dekhega (-90 degrees)
        float angle = 0f;
        if (rb.linearVelocity.y > 0) 
        {
            angle = Mathf.Lerp(0, 30, rb.linearVelocity.y / jumpForce);
        }
        else 
        {
            // Neeche girte waqt thoda smooth rotate karega
            angle = Mathf.Lerp(0, -60, -rb.linearVelocity.y / jumpForce);
        }
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // --- NEW: Out of bounds death (ANY direction) ---
        // Y-axis (Upar/Neeche) and X-axis (Left/Right)
        if (transform.position.y > 6.5f || transform.position.y < -6.5f || 
            transform.position.x > 4.5f || transform.position.x < -4.5f)
        {
            Die("You flew out of the universe!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        // THE EVIL TRAP: Player thinks gap is safe, but it kills them
        if (collision.gameObject.CompareTag("DeathGap"))
        {
            Die(""); // Blank string bhejenge toh GameManager apna default random gaali dega
        }
        
        // THE SECRET: Player flies directly through the solid pipe to survive
        if (collision.gameObject.CompareTag("SafePipe"))
        {
            pipesPassed++;
            
            if (pipesPassed >= pipesToWin)
            {
                isDead = true;
                rb.linearVelocity = Vector2.zero;
                rb.isKinematic = true; // Freeze player in air
                GameManager.Instance.WinLevel();
            }
        }
    }

    private void Die(string reason)
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        
        // Use the existing GameManager to handle the ragebait death
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver(reason);
        }
    }
}
