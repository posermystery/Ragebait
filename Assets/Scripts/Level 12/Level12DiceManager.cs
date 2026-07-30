using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Level12DiceManager : MonoBehaviour
{
    [Header("Dice UI Elements")]
    public Image dice1Image;
    public Image dice2Image;
    
    [Header("Dice Sprites (1 to 6)")]
    [Tooltip("Assign 6 sprites here for faces 1, 2, 3, 4, 5, 6")]
    public Sprite[] diceFaces;

    [Header("Audio")]
    public AudioClip rollSound;
    private AudioSource audioSource;

    [Header("Settings")]
    public float rollDuration = 0.5f;
    public float timeBetweenFrames = 0.05f;

    // State tracking
    private bool isRolling = false;
    private bool isDice1Held = false;
    private bool isDice2Held = false;
    private int rollCount = 0;
    private const int MAX_ROLLS = 5;

    // Current values (1 to 6)
    private int dice1Value = 1;
    private int dice2Value = 1;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        
        // Randomize initial state visually
        dice1Value = Random.Range(1, 7);
        dice2Value = Random.Range(1, 7);
        UpdateDiceVisuals();
    }

    // --- UI EVENT METHODS FOR HOLDING DICE ---
    public void OnPointerDownDice1() { isDice1Held = true; }
    public void OnPointerUpDice1() { isDice1Held = false; }
    
    public void OnPointerDownDice2() { isDice2Held = true; }
    public void OnPointerUpDice2() { isDice2Held = false; }

    // --- ROLL METHOD ---
    public void RollDice()
    {
        if (isRolling) return;

        // If both are held, you can't roll!
        if (isDice1Held && isDice2Held) return;

        StartCoroutine(RollAnimationCoroutine());
    }

    private IEnumerator RollAnimationCoroutine()
    {
        isRolling = true;
        rollCount++;
        
        if (rollSound != null) audioSource.PlayOneShot(rollSound);

        float timer = 0f;
        float spinSpeed = 1000f; // degrees per second
        
        RectTransform rect1 = dice1Image.rectTransform;
        RectTransform rect2 = dice2Image.rectTransform;

        // Animate while timer runs
        while (timer < rollDuration)
        {
            if (!isDice1Held)
            {
                dice1Image.sprite = diceFaces[Random.Range(0, 6)];
                rect1.Rotate(0, 0, spinSpeed * Time.deltaTime);
                rect1.localScale = Vector3.one * 1.2f; // Slight bounce up
            }
            if (!isDice2Held)
            {
                dice2Image.sprite = diceFaces[Random.Range(0, 6)];
                rect2.Rotate(0, 0, -spinSpeed * Time.deltaTime); // Spin opposite direction
                rect2.localScale = Vector3.one * 1.2f;
            }

            timer += Time.deltaTime;
            yield return null; // wait for next frame for smooth rotation
        }

        // Snap back to normal size and straight rotation
        if (!isDice1Held) { rect1.localRotation = Quaternion.identity; rect1.localScale = Vector3.one; }
        if (!isDice2Held) { rect2.localRotation = Quaternion.identity; rect2.localScale = Vector3.one; }

        // Generate Final Rigged Numbers
        DetermineFinalRiggedNumbers();
        UpdateDiceVisuals();

        // --- CHECK WIN CONDITION ---
        if (dice1Value + dice2Value == 12)
        {
            // WIN!
            yield return new WaitForSeconds(0.5f);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinLevel();
            }
        }
        else if (rollCount >= MAX_ROLLS)
        {
            // Loss by gambling debt
            yield return new WaitForSeconds(0.5f);
            GameManager.Instance.GameOver("You ran out of money and the casino mafia broke your legs. Should have brought more cash!");
        }

        isRolling = false;
    }

    private void DetermineFinalRiggedNumbers()
    {
        // Pick random numbers for the dice that are NOT being held
        if (!isDice1Held) dice1Value = Random.Range(1, 7);
        if (!isDice2Held) dice2Value = Random.Range(1, 7);

        // THE TROLL LOGIC: Prevent natural double 6
        // If neither dice is held, the game cheats and refuses to give a 12.
        if (!isDice1Held && !isDice2Held)
        {
            if (dice1Value == 6 && dice2Value == 6)
            {
                // Force one of them to not be a 6! (Randomly pick which one to sabotage)
                if (Random.value > 0.5f)
                    dice1Value = Random.Range(1, 6); // 1 to 5
                else
                    dice2Value = Random.Range(1, 6); // 1 to 5
            }
        }
        
        // If one IS held, we DON'T interfere. 
        // So if they hold a 6, and the other one randomly rolls a 6, they get 12 and win!
    }

    private void UpdateDiceVisuals()
    {
        if (diceFaces == null || diceFaces.Length < 6) return;

        // Array is 0-indexed, so value 1 is index 0
        dice1Image.sprite = diceFaces[dice1Value - 1];
        dice2Image.sprite = diceFaces[dice2Value - 1];
    }
}
