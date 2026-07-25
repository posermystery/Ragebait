using UnityEngine;
using UnityEngine.InputSystem;

public class EggJumpPlayer : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 12f;
    
    [Header("Trail Settings (Customize Color in Inspector!)")]
    public Color trailStartColor = Color.white;
    public Color trailEndColor = new Color(1f, 0.8f, 0.2f); // Golden Yellow by default
    public float trailDuration = 0.3f;
    public float trailWidth = 0.5f;

    [Header("Audio")]
    public AudioClip jumpSound;
    private AudioSource audioSource;

    private Rigidbody2D rb;
    private bool isGravityReversed = false;
    private float initialGravity = 1.5f;
    private bool isGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = initialGravity;
        rb.freezeRotation = true; // No rotation for the ball/egg
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        // Setup visual trail automatically with your custom inspector colors!
        SetupTrail();

        // Enable Accelerometer in New Input System for mobile phone flip detection
        if (Accelerometer.current != null)
        {
            InputSystem.EnableDevice(Accelerometer.current);
        }
    }

    private void SetupTrail()
    {
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
        }

        trail.time = trailDuration; // Configurable duration
        trail.startWidth = trailWidth; // Configurable width
        trail.endWidth = 0.05f;  // Sharp tapered tail at the end
        trail.minVertexDistance = 0.05f;
        trail.autodestruct = false;
        trail.emitting = true;

        // Use standard 2D sprite shader so it renders bright and clean
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            trail.material = new Material(shader);
        }

        // Render behind the egg sprite!
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            trail.sortingLayerName = sr.sortingLayerName;
            trail.sortingOrder = sr.sortingOrder - 1;
        }

        // Create gradient using your custom Start and End colors from Inspector!
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(trailStartColor, 0.0f), new GradientColorKey(trailEndColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(trailStartColor.a != 0 ? trailStartColor.a : 0.8f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        trail.colorGradient = gradient;
    }

    void Update()
    {
        // 1. Check for Mobile Flip (Upside Down) OR 'F' / 'Up Arrow' key in Unity Editor to win!
        bool flipTriggered = false;
        if (Accelerometer.current != null && Accelerometer.current.acceleration.ReadValue().y > 0.5f)
        {
            flipTriggered = true;
        }
        if (Keyboard.current != null && (Keyboard.current.fKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame))
        {
            flipTriggered = true;
        }

        if (flipTriggered && !isGravityReversed)
        {
            ReverseGravity();
        }

        // 2. Win condition when falling upwards into the sky
        if (isGravityReversed && transform.position.y > 30f)
        {
            Win();
        }
        
        // 3. Lose condition if falling below camera view
        Camera cam = Camera.main;
        if (cam != null && !isGravityReversed)
        {
            // As soon as egg reaches the bottom edge of the screen, die instantly!
            float bottomOfScreen = cam.transform.position.y - cam.orthographicSize + 0.2f;
            if (transform.position.y < bottomOfScreen)
            {
                Lose();
                return;
            }
        }

        // 4. Jump Input (Tap on screen, Mouse click, or Spacebar via New Input System)
        bool jumpPressed = false;
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) jumpPressed = true;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) jumpPressed = true;

        if (jumpPressed && !isGravityReversed)
        {
            Jump();
        }
    }

    private void Jump()
    {
        // Prevent jumping when clicking UI buttons
        if (GameManager.IsPointerOverMenuButton()) return;

        // PREVENT MID-AIR JUMPING!
        // Player can only jump if in contact with a basket/surface (isGrounded) OR attached as child to a basket!
        if (!isGrounded && transform.parent == null) return;

        isGrounded = false;
        // Unparent from basket so we jump freely
        transform.SetParent(null);

        // Reset vertical velocity before jumping
        rb.linearVelocity = new Vector2(0f, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        if (jumpSound != null)
            audioSource.PlayOneShot(jumpSound);
    }

    private void ReverseGravity()
    {
        isGravityReversed = true;
        isGrounded = false;
        transform.SetParent(null);
        rb.gravityScale = -2.5f; // Fly up into the sky!
        Debug.Log("<b>[EggJumpPlayer]</b> Gravity Reversed! Egg flying to the sky!");
    }

    private void Win()
    {
        gameObject.SetActive(false);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinLevel();
        }
        else
        {
            Debug.Log("LEVEL WON! (GameManager not found)");
        }
    }

    private void Lose()
    {
        gameObject.SetActive(false);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver("You fell into the void! Better luck next time!");
        }
        else
        {
            Debug.Log("GAME OVER! (GameManager not found)");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Instantly die if touching ANY object/basket near or below the bottom edge of the screen!
        Camera cam = Camera.main;
        if (cam != null && !isGravityReversed)
        {
            float bottomOfScreen = cam.transform.position.y - cam.orthographicSize + 0.2f;
            if (transform.position.y < bottomOfScreen)
            {
                Lose();
                return;
            }
        }

        // Touching a basket or platform
        isGrounded = true;

        // When landing on a basket from above, stick to it so we don't slide off!
        if (collision.gameObject.name.StartsWith("Basket") && rb.linearVelocity.y <= 0.1f && !isGravityReversed)
        {
            transform.SetParent(collision.transform);
            rb.linearVelocity = Vector2.zero;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // Maintain grounded state while touching a surface
        isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Left the surface / in mid-air
        isGrounded = false;
    }

    private void OnDestroy()
    {
        // Reset gravity for future scenes
        Physics2D.gravity = new Vector2(0, -9.81f);
    }
}
