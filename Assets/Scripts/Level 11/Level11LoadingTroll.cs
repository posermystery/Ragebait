using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class Level11LoadingTroll : MonoBehaviour
{
    [Header("UI Elements")]
    public Text loadingText;       // Text showing "Loading Level 11... X%"
    public Slider loadingBar;      // Optional progress bar
    public GameObject baitTipText; // Text saying "Tap screen to speed up loading!"

    [Header("Timing Settings")]
    public float fastFillSpeed = 40f; // Percent per second when not stuck

    private bool isLoadingComplete = false;
    private bool isGameOver = false;

    void Start()
    {
        // Hide the bait tip initially, we can show it when it gets stuck at 99%
        if (baitTipText != null) baitTipText.SetActive(false);

        StartCoroutine(LoadingSequence());
    }

    void Update()
    {
        if (isGameOver || isLoadingComplete) return;

        // Check if the user is tapping, clicking, or pressing keys impatiently
        bool userTapped = false;
        
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            // If they click the "MENU" button (handled by GameManager), we shouldn't kill them for trying to pause/quit.
            // But since GameManager's Menu button is usually a UI button, the click will hit both.
            // Let's rely on the raw pointer down, but exclude the top right corner where the menu button usually is!
            Vector2 pos = Pointer.current.position.ReadValue();
            if (pos.x > Screen.width - 200 && pos.y > Screen.height - 150)
            {
                // Likely clicking the MENU button, ignore
            }
            else
            {
                userTapped = true;
            }
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            userTapped = true;
        }

        if (userTapped)
        {
            FailLevelImpatient();
        }
    }

    private IEnumerator LoadingSequence()
    {
        float currentProgress = 0f;

        // --- Phase 1: 0% to 60% ---
        while (currentProgress < 60f)
        {
            currentProgress += fastFillSpeed * Time.deltaTime;
            UpdateUI(currentProgress);
            yield return null;
        }
        UpdateUI(60f);
        
        // Stuck at 60% for 3 seconds
        yield return new WaitForSeconds(3f);

        // --- Phase 2: 60% to 95% ---
        while (currentProgress < 95f)
        {
            currentProgress += fastFillSpeed * Time.deltaTime;
            UpdateUI(currentProgress);
            yield return null;
        }
        UpdateUI(95f);

        // Stuck at 95% for 3 seconds
        yield return new WaitForSeconds(3f);

        // --- Phase 3: 95% to 99% ---
        while (currentProgress < 99f)
        {
            // Slow down dramatically for the last few percentages to build tension
            currentProgress += (fastFillSpeed / 4f) * Time.deltaTime;
            UpdateUI(currentProgress);
            yield return null;
        }
        UpdateUI(99f);

        // Show the bait text to trick them into tapping!
        if (baitTipText != null) baitTipText.SetActive(true);

        // Stuck at 99% for 5 grueling seconds
        yield return new WaitForSeconds(5f);

        // --- Phase 4: 100% SUCCESS ---
        UpdateUI(100f);
        isLoadingComplete = true;

        if (baitTipText != null) baitTipText.SetActive(false);

        // Win the level!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinLevel();
        }
    }

    private void UpdateUI(float percent)
    {
        int p = Mathf.FloorToInt(percent);
        if (loadingText != null)
        {
            loadingText.text = "Loading Level 11... " + p + "%";
        }
        if (loadingBar != null)
        {
            loadingBar.value = p / 100f;
        }
    }

    private void FailLevelImpatient()
    {
        isGameOver = true;
        StopAllCoroutines();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver("Impatient much? You couldn't even wait for a loading screen. Your attention span is shorter than a goldfish.");
        }
    }
}
