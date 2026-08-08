using UnityEngine;
using UnityEngine.UI;

public class WaterLeakController : MonoBehaviour
{
    [Header("UI & Visuals")]
    public Image waterImage;                 // The blue UI image acting as water
    public ParticleSystem leakBubbles;       // Bubbles coming out of the hole

    [Header("Game Rules")]
    public float drainSpeed = 0.05f;         // How fast the water drains per second (0 to 1)
    public float requiredChargeTime = 2.0f;  // Must be plugged in for 2 seconds to win

    private bool isGameOver = false;
    private bool levelWon = false;
    private float initialBubbleLifetime = 1f;
    private float currentChargeTime = 0f;

    void Start()
    {
        // Make sure the water is full at the start
        if (waterImage != null)
        {
            waterImage.fillAmount = 1.0f;
        }

        // Make sure bubbles are playing
        if (leakBubbles != null)
        {
            initialBubbleLifetime = leakBubbles.main.startLifetimeMultiplier;
            if (!leakBubbles.isPlaying)
            {
                leakBubbles.Play();
            }
        }
    }

    void Update()
    {
        if (isGameOver || levelWon) return;

        // Check if phone (or laptop) is plugged into a charger
        bool isPluggedIn = (SystemInfo.batteryStatus == BatteryStatus.Charging || SystemInfo.batteryStatus == BatteryStatus.Full);

#if UNITY_EDITOR
        // HACK for PC Testing: Also allow 'C' key just in case you don't have your laptop charger handy
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.cKey.isPressed)
        {
            isPluggedIn = true;
        }
#endif

        if (isPluggedIn)
        {
            // Immediately stop bubbles when plugged in
            if (leakBubbles != null && leakBubbles.isPlaying) 
            {
                leakBubbles.Stop();
            }

            currentChargeTime += Time.deltaTime;
            if (currentChargeTime >= requiredChargeTime)
            {
                WinGame();
            }
        }
        else
        {
            currentChargeTime = 0f; // Reset if unplugged before 2 seconds

            // Resume bubbles if they were stopped
            if (leakBubbles != null && !leakBubbles.isPlaying)
            {
                leakBubbles.Play();
            }

            // Drain the water slowly!
            if (waterImage != null)
            {
                waterImage.fillAmount -= drainSpeed * Time.deltaTime;

                // Dynamically reduce bubble lifetime so they pop before hitting the air
                if (leakBubbles != null)
                {
                    var main = leakBubbles.main;
                    main.startLifetimeMultiplier = initialBubbleLifetime * waterImage.fillAmount;
                }

                if (waterImage.fillAmount <= 0)
                {
                    LoseGame();
                }
            }
        }
    }

    private void WinGame()
    {
        levelWon = true;

        // Stop the leak!
        if (leakBubbles != null)
        {
            leakBubbles.Stop(); // Bubbles will stop generating
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinLevel();
        }
    }

    private void LoseGame()
    {
        isGameOver = true;

        if (leakBubbles != null)
        {
            leakBubbles.Stop(); // Stop bubbles on Game Over too
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver("The water completely leaked out! You completely failed to stop the leak.");
        }
    }
}
