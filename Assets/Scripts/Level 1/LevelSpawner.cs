using System.Collections.Generic;
using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject orbitPrefab;
    public Transform player;

    [Header("Spawn Settings")]
    public float spawnDistanceY = 6f; // Distance between each orbit
    public int initialSpawnCount = 5; // How many to spawn at start
    public float spawnAheadDistance = 15f; // How far ahead of the player to spawn

    [Header("Difficulty Settings")]
    public float startScale = 3f; // Initial easy, large orbits
    public float minScale = 1.2f; // Minimum size for hardest orbits
    public float scaleDecreaseRate = 0.05f; // How much scale decreases per orbit

    [Header("Level Settings")]
    public int targetOrbitCount = 8; // Number of orbits to win the level

    private float nextSpawnY = 0f;
    private int orbitsSpawned = 0;
    private List<GameObject> activeOrbits = new List<GameObject>();

    void Start()
    {
        // Spawn the first few orbits
        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnOrbit();
        }
    }

    void Update()
    {
        // If player gets close to the highest spawned orbit, spawn a new one
        if (player != null && player.position.y + spawnAheadDistance > nextSpawnY)
        {
            SpawnOrbit();
            CleanupOldOrbits();
        }
    }

    private void SpawnOrbit()
    {
        if (orbitsSpawned >= targetOrbitCount) return; // Reached the end!

        // Calculate the scale based on progression
        float currentScale = Mathf.Max(minScale, startScale - (orbitsSpawned * scaleDecreaseRate));

        // Randomize X position slightly so it's not a straight line upwards
        float randomX = Random.Range(-2f, 2f);
        
        // If it's the very first orbit, or the troll orbit (second to last), or the golden orbit, keep it centered
        // This ensures the troll happens perfectly on-screen.
        if (orbitsSpawned == 0 || orbitsSpawned >= targetOrbitCount - 2)
        {
            randomX = 0f;
            if (orbitsSpawned == 0) nextSpawnY = 0f; // Make sure first orbit is exactly at origin
        }

        Vector3 spawnPosition = new Vector3(randomX, nextSpawnY, 0f);

        // Spawn the orbit
        GameObject newOrbit = Instantiate(orbitPrefab, spawnPosition, Quaternion.identity);
        newOrbit.transform.localScale = new Vector3(currentScale, currentScale, 1f);

        // Randomly set clockwise or counter-clockwise
        Orbit orbitScript = newOrbit.GetComponent<Orbit>();
        if (orbitScript != null)
        {
            orbitScript.isClockwise = Random.value > 0.5f;
            
            // THE WHIP EFFECT
            float baseSpeed = Random.Range(150f, 200f);
            orbitScript.rotationSpeed = baseSpeed * (startScale / currentScale);
        }

        // --- RAGEBAIT: The Golden Orbit (Win Condition) ---
        if (orbitsSpawned == targetOrbitCount - 1) // 0-indexed, so 7 is the 8th orbit
        {
            newOrbit.tag = "GoldenOrbit";
            SpriteRenderer sr = newOrbit.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.yellow; // Make it look golden
            }
        }

        activeOrbits.Add(newOrbit);

        
        nextSpawnY += spawnDistanceY;
        orbitsSpawned++;
    }

    private void CleanupOldOrbits()
    {
        // Remove orbits that are too far below the player
        if (activeOrbits.Count > 0)
        {
            GameObject oldestOrbit = activeOrbits[0];
            if (oldestOrbit != null && oldestOrbit.transform.position.y < player.position.y - 10f)
            {
                // Ensure we don't delete the orbit the player is currently on!
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null && pc.currentOrbit != oldestOrbit.GetComponent<Orbit>())
                {
                    activeOrbits.RemoveAt(0);
                    Destroy(oldestOrbit);
                }
            }
        }
    }
}
