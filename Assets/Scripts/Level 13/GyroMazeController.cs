using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class GyroMazeController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 8f;
    [Tooltip("How much the ball slows down when controls are inverted (e.g. 0.6 = 40% slower)")]
    public float invertedSpeedMultiplier = 0.6f;
    public float tiltSmoothness = 2f; // Lowered from 5f for smoother acceleration
    
    [Header("Inversion Troll Settings")]
    public float flipInterval = 5f;
    private bool isInverted = false;
    
    [Header("Visuals & UI")]
    public Color normalColor = Color.white;
    public Color invertedColor = Color.red;
    public Text warningText;
    
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isDead = false;
    
    private Vector2 currentTilt = Vector2.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        
        // Ensure no gravity since it's a top-down maze
        rb.gravityScale = 0f;
        
        // Automatically grab the color you set in the Sprite Renderer!
        normalColor = sr.color;
        
        if (warningText != null)
        {
            StartCoroutine(ShowIntroText());
        }

        if (Accelerometer.current != null)
        {
            InputSystem.EnableDevice(Accelerometer.current);
        }

        // Start the 5-second inversion loop
        InvokeRepeating(nameof(FlipControls), flipInterval, flipInterval);
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 targetTilt = Vector2.zero;

        // 1. Get Mobile Tilt (X and Y axis for full maze movement)
        if (Accelerometer.current != null)
        {
            Vector3 accel = Accelerometer.current.acceleration.ReadValue();
            targetTilt = new Vector2(accel.x, accel.y);
        }
        // 2. PC Fallback (WASD or Arrows)
        else if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) targetTilt.x = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) targetTilt.x = 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) targetTilt.y = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) targetTilt.y = -1f;
        }

        // 3. Apply Inversion Troll
        if (isInverted)
        {
            targetTilt = -targetTilt;
        }

        // 4. Smooth Momentum (Feels like sliding slightly)
        currentTilt = Vector2.Lerp(currentTilt, targetTilt, Time.fixedDeltaTime * tiltSmoothness);

        // 5. Apply Movement (Slower if inverted)
        float currentSpeed = isInverted ? (speed * invertedSpeedMultiplier) : speed;
        rb.linearVelocity = currentTilt * currentSpeed;
    }

    private void FlipControls()
    {
        if (isDead) return;

        isInverted = !isInverted;
        
        // Change color to indicate the current state visually
        sr.color = isInverted ? invertedColor : normalColor;

        if (warningText != null)
        {
            StartCoroutine(FlashWarningText());
        }
    }

    private IEnumerator FlashWarningText()
    {
        warningText.text = isInverted ? "CONTROLS INVERTED!" : "CONTROLS NORMAL!";
        warningText.color = isInverted ? Color.red : Color.green;
        
        yield return new WaitForSeconds(1.5f);
        
        warningText.text = "";
    }

    private IEnumerator ShowIntroText()
    {
        warningText.text = "Tilt your device to move!";
        warningText.color = Color.yellow;
        
        yield return new WaitForSeconds(3f);
        
        // Only clear if the flip hasn't already overwritten it
        if (warningText.text == "Tilt your device to move!")
        {
            warningText.text = "";
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("DeathTrap"))
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("DeathTrap"))
        {
            Die();
        }
        else if (collision.CompareTag("GoldenOrbit") || collision.CompareTag("WinTrigger"))
        {
            isDead = true;
            rb.linearVelocity = Vector2.zero;
            if (GameManager.Instance != null) GameManager.Instance.WinLevel();
        }
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        sr.enabled = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver("You hit the wall! Stay focused, your brain needs to adapt faster.");
        }
    }
}
