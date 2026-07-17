using UnityEngine;

public class FlappyPlayer : MonoBehaviour
{
    private Rigidbody2D rb;
    public float jumpForce = 5f;
    private bool isDead = false;
    
    private int pipesPassed = 0;
    public int pipesToWin = 5; // Win after 5 safe pipes

    public AudioClip jumpSound;
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

        // Tapping / Clicking makes the bird fly up
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            rb.velocity = Vector2.up * jumpForce;
            if (jumpSound != null) audioSource.PlayOneShot(jumpSound);
        }

        // Out of bounds death (falling down or flying too high)
        if (transform.position.y > 6.5f || transform.position.y < -6.5f)
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
            Die("You didn't pass through the pipe!");
        }
        
        // THE SECRET: Player flies directly through the solid pipe to survive
        if (collision.gameObject.CompareTag("SafePipe"))
        {
            pipesPassed++;
            
            if (pipesPassed >= pipesToWin)
            {
                isDead = true;
                rb.velocity = Vector2.zero;
                rb.isKinematic = true; // Freeze player in air
                GameManager.Instance.WinLevel();
            }
        }
    }

    private void Die(string reason)
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        
        // Use the existing GameManager to handle the ragebait death
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver(reason);
        }
    }
}
