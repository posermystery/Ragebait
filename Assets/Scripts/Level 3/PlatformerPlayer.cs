using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerPlayer : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    
    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    
    [Header("Audio")]
    public AudioClip jumpSound;
    [Range(0f, 1f)] public float jumpVolume = 0.1f;
    
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private bool isDead = false;
    private bool isGrounded = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (isDead) return;

        // Check if grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        float horizontalInput = 0f;
        bool jumpPressed = false;

        // --- PC CONTROLS ---
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput = 1f;
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame) jumpPressed = true;
        }

        // --- MOBILE TOUCH CONTROLS (Invisible Zones) ---
        if (Touchscreen.current != null)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            foreach (var touch in Touchscreen.current.touches)
            {
                var phase = touch.phase.ReadValue();
                if (phase == UnityEngine.InputSystem.TouchPhase.Began || 
                    phase == UnityEngine.InputSystem.TouchPhase.Moved || 
                    phase == UnityEngine.InputSystem.TouchPhase.Stationary)
                {
                    Vector2 pos = touch.position.ReadValue();
                    
                    // Top 50% = Jump
                    if (pos.y > screenHeight * 0.5f)
                    {
                        if (phase == UnityEngine.InputSystem.TouchPhase.Began) jumpPressed = true;
                    }
                    // Bottom Left = Move Left
                    else if (pos.x < screenWidth * 0.5f)
                    {
                        horizontalInput = -1f;
                    }
                    // Bottom Right = Move Right
                    else if (pos.x >= screenWidth * 0.5f)
                    {
                        horizontalInput = 1f;
                    }
                }
            }
        }

        // Move Player
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Jump Player
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (jumpSound != null) audioSource.PlayOneShot(jumpSound, jumpVolume);
        }
        
        // Flip Sprite
        if (horizontalInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1, 1);
        }

        // Fall out of world death
        if (transform.position.y < -10f)
        {
            Die("You fell into the abyss.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("DeathTrap"))
        {
            Die("Wow, you stepped on a spike. Very original.");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("DeathTrap"))
        {
            Die("Wow, you stepped on a spike. Very original.");
        }
    }

    public void Die(string reason)
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true; // freeze
        gameObject.GetComponent<SpriteRenderer>().enabled = false; // Hide player

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver(reason);
        }
    }
}
