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

    [Header("State")]
    public bool isOrbiting = false;
    public Orbit currentOrbit;

    [Header("Audio")]
    public AudioClip jumpSound;
    [Range(0f, 1f)] public float jumpVolume = 0.1f; // Jump sound volume control
    private AudioSource audioSource;

    private float currentAngle = 0f;
    private float currentRadius = 0f;
    private float totalRevolutions = 0f;
    private float airTime = 0f; // Tracks time spent in mid-air
    private Rigidbody2D rb;
    private Vector2 jumpDir;

    // Track whether the current orbit is the last one before golden
    private bool isOnLastOrbit = false;

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

        // --- Out of Bounds Check (Timer Based) ---
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
        
        // Add drag to simulate planetary gravity slowing the meteor down
        rb.linearDamping = spaceDrag;
        
        // Calculate tangent direction
        float tangentOffset = currentOrbit.isClockwise ? -90f : 90f;
        float tangentAngleRad = (currentAngle + tangentOffset) * Mathf.Deg2Rad;

        jumpDir = new Vector2(
            Mathf.Cos(tangentAngleRad),
            Mathf.Sin(tangentAngleRad)
        ).normalized;

        // --- THE TROLL: If on last orbit, reverse the jump direction 180 degrees! ---
        if (isOnLastOrbit)
        {
            jumpDir = -jumpDir; // 180 degree flip!
        }

        // Apply velocity to the Rigidbody
        rb.linearVelocity = jumpDir * jumpSpeed;
        
        if (jumpSound != null) audioSource.PlayOneShot(jumpSound, jumpVolume);
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

        // Check if this orbit is the last one before golden (next orbit in spawner would be golden)
        // We detect this by checking if any sibling/nearby orbit has the GoldenOrbit tag
        isOnLastOrbit = IsNextOrbitGolden();
    }

    private bool IsNextOrbitGolden()
    {
        // Find the LevelSpawner to check if this is the second-to-last orbit
        LevelSpawner spawner = FindAnyObjectByType<LevelSpawner>();
        if (spawner == null) return false;

        // The golden orbit is the last one (targetOrbitCount - 1, 0-indexed)
        // So the "last normal orbit" is targetOrbitCount - 2
        // We check by counting how many orbits exist with "Orbit" tag below the golden one
        
        // Simple approach: Find the golden orbit and check if this orbit is the closest one to it
        GameObject[] allOrbits = GameObject.FindGameObjectsWithTag("Orbit");
        GameObject goldenOrbit = GameObject.FindGameObjectWithTag("GoldenOrbit");
        
        if (goldenOrbit == null) return false;

        // Find which normal orbit is closest to the golden orbit (that's the last one)
        float closestDist = float.MaxValue;
        Orbit closestOrbit = null;

        foreach (GameObject orbitObj in allOrbits)
        {
            float dist = Vector2.Distance(orbitObj.transform.position, goldenOrbit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestOrbit = orbitObj.GetComponent<Orbit>();
            }
        }

        return closestOrbit == currentOrbit;
    }
}
