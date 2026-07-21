using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class DinoRunner : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 10f;
    [Range(0.05f, 0.5f)]
    public float jumpCooldown = 0.12f;

    [Header("Win Condition")]
    [Tooltip("Y position above which the player wins (above camera top)")]
    public float winHeight = 6f;

    [Header("UI")]
    public Text scoreText;

    [Header("Audio")]
    public AudioClip jumpSound;
    [Range(0f, 1f)] public float jumpVolume = 0.3f;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private float lastJumpTime = -1f;
    private float score = 0f;
    private bool isDead = false;
    private bool inputLocked = true;

    private const string deathMessage = "You didn't jump high enough.";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        StartCoroutine(UnlockInputDelay());
    }

    private IEnumerator UnlockInputDelay()
    {
        yield return new WaitForSeconds(0.3f);
        inputLocked = false;
    }

    void Update()
    {
        if (isDead || inputLocked) return;

        // Visual polish: Rotate continuously to simulate rolling forward
        rb.freezeRotation = false;
        rb.angularVelocity = -400f; 

        // Score counter
        score += Time.deltaTime * 10f;
        if (scoreText != null)
        {
            // Troll: score goes crazy when player discovers infinite jump
            if (transform.position.y > 2f)
                scoreText.text = "?!?!?";
            else
                scoreText.text = Mathf.FloorToInt(score).ToString("D5");
        }

        // Jump input — tap, click, or spacebar
        bool jumpPressed = false;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            jumpPressed = true;

        if (jumpPressed && Time.time - lastJumpTime > jumpCooldown)
        {
            // THE TROLL: INFINITE JUMP — no ground check at all!
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Reset Y for consistent jump
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            lastJumpTime = Time.time;

            if (jumpSound != null) audioSource.PlayOneShot(jumpSound, jumpVolume);
        }

        // WIN CONDITION: Player flew above the screen!
        if (transform.position.y > winHeight)
        {
            isDead = true;
            if (GameManager.Instance != null)
                GameManager.Instance.WinLevel();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            isDead = true;
            gameObject.SetActive(false);

            if (GameManager.Instance != null)
                GameManager.Instance.GameOver(deathMessage);
        }
    }
}
