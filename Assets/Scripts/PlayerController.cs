using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpSpeed = 25f; // Increased initial speed
    public float spaceDrag = 1.5f; // How quickly it slows down in space (Planetary feel)

    [Header("Orbit Entry Settings")]
    public float spiralSpeed = 5f; // How fast it corrects its orbit to the perfect edge

    [Header("Visual FX")]
    public GameObject deathExplosionPrefab;

    [Header("Ragebait Settings")]
    public float jitterAmount = 0.15f; // How much the ball shakes to ruin timing
    public float reversalCheckInterval = 1f; // How often to check for a sudden reversal
    public float reversalChance = 0.15f; // 15% chance to suddenly reverse direction

    [Header("State")]
    public bool isOrbiting = false;
    public Orbit currentOrbit;

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip reverseSound;
    private AudioSource audioSource;

    private float currentAngle = 0f;
    private float currentRadius = 0f;
    private float nextReversalCheck = 0f;
    private float totalRevolutions = 0f;
    private float airTime = 0f; // Tracks time spent in mid-air
    private bool willReverseMidAir = false;
    private float midAirReverseTimer = 0f;
    private Rigidbody2D rb;
    private Vector2 jumpDir;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        // If we start attached to an orbit
        if (currentOrbit != null)
        {
            AttachToOrbit(currentOrbit);
            currentRadius = currentOrbit.radius; // Perfect start
        }
    }

    void Update()
    {
        if (isOrbiting && currentOrbit != null)
        {
            // Smoothly transition the radius from entry point to the perfect orbit edge
            currentRadius = Mathf.Lerp(currentRadius, currentOrbit.radius, Time.deltaTime * spiralSpeed);

            // Calculate new angle based on orbit speed and direction
            float directionMult = currentOrbit.isClockwise ? -1f : 1f;
            currentAngle += currentOrbit.rotationSpeed * directionMult * Time.deltaTime;
            
            // Keep angle between 0 and 360
            currentAngle %= 360f;

            // --- RAGEBAIT: Sudden Reversal ---
            if (Time.time >= nextReversalCheck)
            {
                nextReversalCheck = Time.time + reversalCheckInterval;
                if (Random.value < reversalChance)
                {
                    currentOrbit.isClockwise = !currentOrbit.isClockwise; // Surprise!
                }
            }

            // Calculate new position using trigonometry and our dynamic currentRadius
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector2 newPos = new Vector2(
                Mathf.Cos(rad) * currentRadius,
                Mathf.Sin(rad) * currentRadius
            );

            // --- RAGEBAIT: Anti-Camping Mechanic ---
            // If they wait too long (3 revolutions), they die!
            totalRevolutions += (currentOrbit.rotationSpeed * Time.deltaTime) / 360f;
            if (totalRevolutions >= 3f)
            {
                Die("Took too long! Orbit Exploded.");
            }

            // Move the player to the orbit position (Vector2 drops the Z, so we explicitly set Z to -1)
            // By forcing Z to -1 via code, the ball will physically ALWAYS be closer to the camera than the orbits (which are at Z=0)
            Vector3 calculatedPos = (Vector2)currentOrbit.transform.position + newPos;
            calculatedPos.z = -1f;
            transform.position = calculatedPos;

            // Check for tap/click to jump using the new Input System
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                Jump();
            }
        }
        else if (!isOrbiting)
        {
            // --- RAGEBAIT: Mid-Air Rubberband Reversal! ---
            if (willReverseMidAir)
            {
                midAirReverseTimer -= Time.deltaTime;
                if (midAirReverseTimer <= 0f)
                {
                    willReverseMidAir = false; // Only do it once
                    jumpDir = -jumpDir; // Reverse direction
                    rb.linearVelocity = jumpDir * jumpSpeed; // Shoot backwards with full speed!
                    
                    if (reverseSound != null) audioSource.PlayOneShot(reverseSound);

                    // Give a tiny bit of extra airtime so they don't unfairly timeout instantly
                    airTime -= 0.5f; 
                }
            }
        }

        // --- RAGEBAIT: Out of Bounds Check (Timer Based) ---
        // Instead of checking distance or speed, we just use a strict timer!
        // If you are flying in space for more than 1.5 seconds, you die.
        if (!isOrbiting)
        {
            airTime += Time.deltaTime;
            if (airTime > 1.5f)
            {
                Die("Lost in space! Jump faster.");
            }
        }
    }

    private void Die(string reason)
    {
        // Spawn the explosion particle effect
        if (deathExplosionPrefab != null)
        {
            Instantiate(deathExplosionPrefab, transform.position, Quaternion.identity);
        }

        // Hide the player so it looks like it was destroyed
        gameObject.SetActive(false);

        // Tell the GameManager to trigger the UI and Ragebait logic
        GameManager.Instance.GameOver(reason);
    }

    private void Jump()
    {
        isOrbiting = false;
        airTime = 0f; // Reset mid-air timer
        
        // --- RAGEBAIT: 10% chance to rubberband backwards in mid-air! ---
        willReverseMidAir = (Random.value < 0.10f);
        if (willReverseMidAir)
        {
            // Will snap backward randomly between 0.15s and 0.4s after jumping
            midAirReverseTimer = Random.Range(0.15f, 0.4f);
        }
        
        // Add drag to simulate planetary gravity slowing the meteor down
        rb.linearDamping = spaceDrag;
        
        // Calculate tangent direction
        float tangentOffset = currentOrbit.isClockwise ? -90f : 90f;
        float tangentAngleRad = (currentAngle + tangentOffset) * Mathf.Deg2Rad;

        jumpDir = new Vector2(
            Mathf.Cos(tangentAngleRad),
            Mathf.Sin(tangentAngleRad)
        ).normalized;

        // Apply velocity to the Rigidbody
        rb.linearVelocity = jumpDir * jumpSpeed;
        
        if (jumpSound != null) audioSource.PlayOneShot(jumpSound);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isOrbiting && collision.CompareTag("Orbit"))
        {
            Orbit orbit = collision.GetComponent<Orbit>();
            if (orbit != null)
            {
                AttachToOrbit(orbit);
            }
        }
        else if (collision.CompareTag("GoldenOrbit"))
        {
            GameManager.Instance.WinLevel();
        }
    }

    private void AttachToOrbit(Orbit orbit)
    {
        currentOrbit = orbit;
        isOrbiting = true;
        
        Vector2 directionFromCenter = transform.position - orbit.transform.position;

        // Determine natural rotation direction based on entry velocity!
        // This makes the meteor preserve its momentum instead of instantly reversing direction.
        // 2D Cross product of directionFromCenter and velocity.
        if (rb.linearVelocity.magnitude > 0.1f) // Only calculate if moving
        {
            float crossProduct = (directionFromCenter.x * rb.linearVelocity.y) - (directionFromCenter.y * rb.linearVelocity.x);
            // If crossProduct > 0 -> Counter-Clockwise
            // If crossProduct < 0 -> Clockwise
            orbit.isClockwise = (crossProduct < 0);
        }

        // Stop movement and reset drag
        rb.linearVelocity = Vector2.zero;
        rb.linearDamping = 0f;
        totalRevolutions = 0f; // Reset revolution counter for the new orbit

        // Calculate exact distance from center when hit (for the meteor spiral effect)
        currentRadius = directionFromCenter.magnitude;

        // Calculate the angle we hit the orbit at
        currentAngle = Mathf.Atan2(directionFromCenter.y, directionFromCenter.x) * Mathf.Rad2Deg;
    }
}
