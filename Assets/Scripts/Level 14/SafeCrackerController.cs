using UnityEngine;
using UnityEngine.UI;

public class SafeCrackerController : MonoBehaviour
{
    [Header("UI Elements")]
    public Transform needlePivot;
    public Transform targetPivot;
    public Image targetZoneImage; 
    public Text statusText;
    
    [Header("Juice Effects")]
    public Transform safeDialContainer; // For the Punch effect
    public AudioSource sfxSource;       // For the Ting sound
    public AudioClip hitTingSound;      // The Ting sound clip
    
    [Header("Game Settings")]
    public float initialSpeed = 150f;
    public float speedMultiplierPerHit = 1.25f;
    
    public float initialToleranceAngle = 30f; // +/- 30 degrees initial size
    public float toleranceShrinkPerHit = 5f;

    private int currentHit = 0;
    private int maxHits = 5;
    
    private float currentSpeed;
    private float currentTolerance;
    private int spinDirection = 1; // 1 or -1
    
    private bool isGameOver = false;
    private bool levelWon = false;
    private float inputEnableTime;

    // The Final Troll Variables
    private bool hasTrolledFinalHit = false;
    private bool isCurrentlyTrolling = false;
    private float trollEndTime = 0f;
    private float preTrollSpeed = 0f;

    void Start()
    {
        currentSpeed = initialSpeed;
        currentTolerance = initialToleranceAngle;
        
        UpdateTargetVisuals();
        PlaceTargetRandomly();
        
        // Wait 0.1 seconds before accepting input to prevent accidental clicks when restarting
        inputEnableTime = Time.time + 0.1f;
    }

    void Update()
    {
        if (isGameOver || levelWon) return;

        float frameSpeed = currentSpeed;

        // The Ultimate Ragebait Troll: Final hit buzz speed!
        if (currentHit == maxHits - 1 && !hasTrolledFinalHit)
        {
            float needleAngle = NormalizeAngle(needlePivot.eulerAngles.z);
            float targetAngle = NormalizeAngle(targetPivot.eulerAngles.z);
            float angleDifference = Mathf.Abs(Mathf.DeltaAngle(needleAngle, targetAngle));

            // If needle gets within 60 degrees of the target, ACTIVATE BUZZ SPEED
            if (angleDifference < 60f && !isCurrentlyTrolling)
            {
                isCurrentlyTrolling = true;
                hasTrolledFinalHit = true; // Only do it once
                trollEndTime = Time.time + 0.25f; // Insane speed for 0.25s
                preTrollSpeed = currentSpeed;
            }
        }

        if (isCurrentlyTrolling)
        {
            if (Time.time < trollEndTime)
            {
                frameSpeed = preTrollSpeed * 5f; // 5x speed warp!
            }
            else
            {
                isCurrentlyTrolling = false;
            }
        }

        // Spin the main needle
        needlePivot.Rotate(0, 0, frameSpeed * spinDirection * Time.deltaTime);
    }

    private void PlaceTargetRandomly()
    {
        // Ensure the new target is far enough from the needle's current position so they have time to react
        float currentAngle = needlePivot.eulerAngles.z;
        float randomOffset = Random.Range(100f, 260f); // Spawns on the opposite side mostly
        targetPivot.eulerAngles = new Vector3(0, 0, currentAngle + randomOffset);
    }

    private void UpdateTargetVisuals()
    {
        // Shrink the target zone visually (Assuming it's a Filled Image)
        if (targetZoneImage != null && targetZoneImage.type == Image.Type.Filled)
        {
            // Tolerance is half the angle. Total angle is currentTolerance * 2.
            // Fill amount is Total Angle / 360.
            targetZoneImage.fillAmount = (currentTolerance * 2f) / 360f;
            
            // Offset rotation so it's centered exactly on the targetPivot
            targetZoneImage.rectTransform.localEulerAngles = new Vector3(0, 0, currentTolerance);
        }
        
        if (statusText != null)
        {
            statusText.text = "HITS: " + currentHit + " / " + maxHits;
        }
    }

    // Call this from a Full Screen Invisible Button!
    public void OnScreenTapped()
    {
        if (isGameOver || levelWon || Time.time < inputEnableTime) return;

        // Check if needle is inside target zone
        float needleAngle = NormalizeAngle(needlePivot.eulerAngles.z);
        float targetAngle = NormalizeAngle(targetPivot.eulerAngles.z);

        // Calculate shortest angular distance between needle and center of target zone
        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(needleAngle, targetAngle));

        // currentTolerance is the permitted offset from the center
        if (angleDifference <= currentTolerance)
        {
            // PERFECT HIT!
            currentHit++;
            
            // Juice: Play Sound and Punch Scale!
            if (sfxSource != null && hitTingSound != null)
            {
                sfxSource.PlayOneShot(hitTingSound);
            }
            StartCoroutine(PunchRoutine());
            
            if (currentHit >= maxHits)
            {
                // YOU WIN!
                levelWon = true;
                if (statusText != null) statusText.text = "LOCK BROKEN!";
                if (GameManager.Instance != null) GameManager.Instance.WinLevel();
            }
            else
            {
                // Increase difficulty
                currentSpeed *= speedMultiplierPerHit;
                currentTolerance -= toleranceShrinkPerHit;
                if (currentTolerance < 5f) currentTolerance = 5f; // Minimum size cap
                
                // Immediately reverse direction on hit to confuse them
                spinDirection *= -1;

                PlaceTargetRandomly();
                UpdateTargetVisuals();
            }
        }
        else
        {
            // MISSED! They tapped too early or too late
            isGameOver = true;
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver("You missed the zone by a hair! Try again.");
            }
        }
    }

    private System.Collections.IEnumerator PunchRoutine()
    {
        if (safeDialContainer == null) yield break;
        
        float timer = 0;
        Vector3 startScale = Vector3.one;
        Vector3 peakScale = new Vector3(1.15f, 1.15f, 1.15f); // 15% bigger
        
        // Pop Up
        while (timer < 0.08f)
        {
            timer += Time.deltaTime;
            safeDialContainer.localScale = Vector3.Lerp(startScale, peakScale, timer / 0.08f);
            yield return null;
        }
        
        // Settle Down
        timer = 0;
        while (timer < 0.15f)
        {
            timer += Time.deltaTime;
            safeDialContainer.localScale = Vector3.Lerp(peakScale, startScale, timer / 0.15f);
            yield return null;
        }
        safeDialContainer.localScale = startScale;
    }

    private float NormalizeAngle(float a)
    {
        a = a % 360f;
        if (a < 0) a += 360f;
        return a;
    }
}
