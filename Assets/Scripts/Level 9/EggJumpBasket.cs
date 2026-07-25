using UnityEngine;

public class EggJumpBasket : MonoBehaviour
{
    [Header("Movement Settings")]
    public bool canMove = true;
    public float moveSpeed = 0.3f;
    public float moveRange = 1.5f; // How far left/right it travels
    
    [Header("Troll Settings (For 6th Golden/Green Basket)")]
    public bool isGoldenTroll = false;
    public float dodgeDistance = 6f; // Slide distance when troll triggers
    public float dodgeSpeed = 3f; // Super smooth, visible teasing slide!
    
    [Header("Audio")]
    public AudioClip trollSound;
    private AudioSource audioSource;

    private float startX;
    private float currentOffset = 0f;
    private float moveDirection = 1f;
    private bool hasDodged = false;
    private float targetX;
    private Transform player;

    void Start()
    {
        startX = transform.position.x;
        targetX = startX;
        
        // Alternate initial direction based on Y coordinate so they don't move in sync
        moveDirection = (Mathf.RoundToInt(transform.position.y) % 8 == 0) ? 1f : -1f;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (isGoldenTroll)
        {
            if (player == null) FindPlayer();

            // ULTIMATE APEX TROLL CONDITION:
            // When player flies up above the basket AND reaches the jump apex (velocity ~ 0, about to drop down), THAT is when it dodges!
            if (!hasDodged && player != null)
            {
                Rigidbody2D prb = player.GetComponent<Rigidbody2D>();
                if (prb != null)
                {
                    // Check if player is above basket level AND their upward velocity has slowed to <= 0.5f (apex of jump)
                    if (player.position.y >= transform.position.y - 0.1f && prb.linearVelocity.y <= 0.5f && Mathf.Abs(player.position.x - transform.position.x) < 3.5f)
                    {
                        Dodge();
                    }
                }
            }
            
            if (hasDodged)
            {
                // Smoothly and teasingly slide away to troll position
                float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * dodgeSpeed);
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            }
            else if (canMove)
            {
                LinearMovement();
            }
        }
        else if (canMove)
        {
            LinearMovement();
        }
    }

    private void LinearMovement()
    {
        if (moveSpeed <= 0f) return;

        // Strict constant speed linear movement (no ease-in / ease-out)
        currentOffset += moveDirection * moveSpeed * Time.deltaTime;
        
        // Reverse direction at edges
        if (currentOffset > moveRange)
        {
            currentOffset = moveRange;
            moveDirection = -1f;
        }
        else if (currentOffset < -moveRange)
        {
            currentOffset = -moveRange;
            moveDirection = 1f;
        }

        transform.position = new Vector3(startX + currentOffset, transform.position.y, transform.position.z);
    }

    private void Dodge()
    {
        hasDodged = true;
        
        // Dodge opposite to where the player is horizontally so player can't land on it
        float dir = (player != null && player.position.x > transform.position.x) ? -1f : 1f;
        if (player != null && Mathf.Abs(player.position.x - transform.position.x) < 0.2f) 
            dir = (Random.value > 0.5f) ? 1f : -1f;

        targetX = startX + (dir * dodgeDistance);

        if (trollSound != null)
            audioSource.PlayOneShot(trollSound);
        
        Debug.Log("<b>[Troll Basket]</b> Dodged at jump apex! Player must flip phone upside down (or press F on PC) to win!");
    }
}
