using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Elements")]
    public GameObject gameOverPanel;
    public Text deathCounterText;
    public Text reasonText;
    public Text rageMessageText;

    [Header("Level Settings")]
    public int currentLevelNumber = 1; // E.g., 1 for Level1, 2 for Level2

    [Header("Dependencies")]
    public CameraController cameraController;

    [Header("Audio")]
    public AudioClip deathSound;
    public AudioClip winSound;
    private AudioSource audioSource;

    public bool isGameOver = false;
    public bool isLevelWon = false;

    private int deathCount = 0;

    private string[] rageMessages = new string[]
    {
        "My grandmother has better reflexes, and she's dead.",
        "Are you playing with your feet?",
        "Did you even try? Because that was embarrassing.",
        "Wow. You have the spatial awareness of a potato.",
        "Maybe stick to watching YouTube tutorials instead of playing.",
        "Is your screen cracked? No? Then what's your excuse?",
        "I've seen broken AI bots fail with more dignity.",
        "A blindfolded monkey could do better.",
        "You're the reason they put instructions on shampoo bottles.",
        "It's just a tapping game. How are you this bad?",
        "Just quit now. Nobody will judge you. Actually, I will.",
        "Your thumbs are legally classified as useless.",
        "Skill issue detected.",
        "I'm actually impressed you managed to fail that.",
        "Even a random number generator would have survived longer.",
        "Are you purposefully trying to make me sad?",
        "Your reaction time is measured in business days.",
        "I showed your gameplay to a professional, and they cried.",
        "Error 404: Talent not found.",
        "Please uninstall. The game is suffering.",
        "Do you want me to slow down the game? Too bad.",
        "It takes genuine effort to be this awful.",
        "If losing was an Olympic sport, you'd have gold.",
        "I can hear you breathing heavily through the screen.",
        "Are we still playing, or did you fall asleep?",
        "Your phone is ashamed of you.",
        "This is why the aliens won't talk to us.",
        "I've seen more graceful car crashes.",
        "You know you're supposed to tap, right?",
        "I bet you struggle with revolving doors too.",
        "Is this your first time using a touchscreen?",
        "I'd tell you to try again, but what's the point?",
        "A potato clock processes information faster than you.",
        "This game is rated E for Everyone, except you.",
        "I'm lowering my expectations, and you're still disappointing me.",
        "You have the hand-eye coordination of a jellyfish.",
        "Even the 'Game Over' screen is tired of seeing you.",
        "Are you lagging in real life?",
        "Maybe your thumbs are on backward?",
        "It's okay to cry. Let it all out.",
        "You could have spent this time learning a new language.",
        "I'm not mad, just deeply, deeply disappointed.",
        "You died again? Groundbreaking.",
        "At least you're consistent. Consistently terrible.",
        "I could write a book on all the mistakes you just made.",
        "You're playing like you have a vendetta against winning.",
        "My patience is thinner than your chances of winning.",
        "If I had feelings, I'd feel pity right now.",
        "Are you streaming this? Please say no.",
        "Imagine explaining this death to your ancestors.",
        "You just lost to a circle. Let that sink in.",
        "The geometry is laughing at you.",
        "Even gravity thinks you're a joke.",
        "I’d give you a hint, but I think it’d be a waste of breath.",
        "Have you considered a career in giving up?",
        "This is a safe space... for everyone except you.",
        "Your gameplay is the definition of a bruh moment.",
        "I've seen toast pop out of a toaster with better timing.",
        "You are the human equivalent of a typo.",
        "Please tell me you did that on purpose.",
        "A sloth with arthritis could have made that jump.",
        "Just close your eyes, it might actually help.",
        "You're single-handedly lowering the global average IQ.",
        "Take a deep breath and realize you're just bad.",
        "Is your brain on airplane mode?",
        "I've seen better timing in a broken clock.",
        "Even the developer didn't think anyone would die here.",
        "Congratulations, you found a new way to fail.",
        "If failure is the best teacher, you must have a PhD by now.",
        "You're playing like your screen is covered in butter.",
        "Did your WiFi drop, or is that just your brain?",
        "I'd applaud your effort, but there wasn't any.",
        "I can't even formulate an insult for whatever that was.",
        "Are you allergic to winning?",
        "Please stop. The pixels are begging you.",
        "You just made a mathematician cry.",
        "I bet you get lost on escalators.",
        "This gameplay is a crime against digital nature.",
        "Wow, you really went out of your way to die there.",
        "Does your brain have ping?",
        "You have the reaction speed of tectonic plates.",
        "I'd suggest practice, but some things can't be fixed.",
        "You're the human equivalent of buffering.",
        "Are you allergic to success?",
        "Even a Roomba has better pathfinding.",
        "You couldn't hit the broad side of a barn from the inside.",
        "I'm starting to think you enjoy the Game Over screen.",
        "Your gameplay is a valid reason for a refund.",
        "I've seen more coordination in a mosh pit.",
        "Is there a gas leak in your room?",
        "Your thumbs must be on strike.",
        "I could watch paint dry and be more entertained.",
        "You play like you're wearing oven mitts.",
        "The laws of physics are embarrassed for you.",
        "I bet you lose at tic-tac-toe against yourself.",
        "You're the reason the tutorial exists.",
        "Did you learn to play from a pigeon?",
        "I'm not saying you're bad, but the math says you're terrible.",
        "Your talent is successfully hidden.",
        "Were you trying to die, or does it just come naturally?",
        "My dog could play better, and he doesn't have thumbs.",
        "Your gaming license has been officially revoked.",
        "Imagine showing this gameplay to your future kids.",
        "I've seen plants with better reaction times.",
        "Did you drop your phone, or are you just like this?",
        "Bro, even the NPCs are feeling second-hand embarrassment.",
        "I would roast you, but your gameplay is doing it for me.",
        "Is there a gas leak in your house? You play like you're dizzy.",
        "You're built different. Built incorrectly, but different.",
        "This is a tragedy, not a playthrough.",
        "You move like a shopping cart with a broken wheel.",
        "Are you legally blind, or just ignoring the screen?",
        "I didn't know it was physically possible to be this bad.",
        "If you were a character, your stats would be in the negatives.",
        "You died so fast the game didn't even have time to load the insult.",
        "Bro is fighting demons, and the demons are winning.",
        "I'm billing you for the brain cells I lost watching that.",
        "You play like your screen is turned off.",
        "Is someone else holding the phone? Please say yes.",
        "You're the reason they put 'Do not drink' on bleach.",
        "I’ve seen better decision making in horror movies.",
        "Are you speedrunning failures?",
        "You have the survival instincts of a lemming.",
        "Bro, just put the fries in the bag.",
        "Your brain is on battery saver mode.",
        "I've seen better pathfinding in Windows 95.",
        "You're the type of guy to drown in a puddle.",
        "Even autocorrect can't fix your mistakes.",
        "Stop. Get some help.",
        "You're the gaming equivalent of a participation trophy.",
        "I bet you fail CAPTCHAs on the regular.",
        "Did you think the obstacle was a collectible?",
        "You play like you're wearing boxing gloves.",
        "Are you allergic to making progress?",
        "You’re playing 4D chess, but everyone else is playing the actual game.",
        "I hope you kept the receipt for your phone.",
        "Your gameplay is making the game engine cry.",
        "Bro's ping is higher than his IQ.",
        "This is a safe space... to laugh at your gameplay.",
        "You have the reflexes of a dial-up connection.",
        "I've seen better reflexes on a sloth on sleeping pills.",
        "You're so bad, I'm actually taking notes on what NOT to do.",
        "Bro thought he was the main character.",
        "Your skills are like a mirage. They don't exist.",
        "I’m starting a GoFundMe for your thumbs.",
        "If stupidity was a currency, you'd be a billionaire.",
        "Bro is literally a walking 'L'.",
        "Even the bugs in this code perform better than you.",
        "You're the reason we have tutorial levels that take 20 minutes."
    };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Hide Game Over panel at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Load and display death count permanently
        deathCount = PlayerPrefs.GetInt("DeathCount", 0);
        UpdateDeathUI();
    }

    private void Update()
    {
        // If the game is over (or won), wait for tap/click to restart
        if (isGameOver || isLevelWon)
        {
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                if (isLevelWon)
                {
                    // Flow shouldn't break! Go directly to the next level.
                    string nextLevelName = "Level" + (currentLevelNumber + 1);
                    
                    // Check if the next level is actually added in Unity Build Settings
                    if (Application.CanStreamedLevelBeLoaded(nextLevelName))
                    {
                        SceneManager.LoadScene(nextLevelName);
                    }
                    else
                    {
                        // If it's the last level of the game and no next level exists, go to Main Menu
                        SceneManager.LoadScene("MainMenu");
                    }
                }
                else
                {
                    RestartLevel();
                }
            }
            return;
        }
    }

    public void GameOver(string reason)
    {
        if (isGameOver || isLevelWon) return;

        isGameOver = true;

        // Trigger Camera Shake for impact
        if (cameraController != null)
        {
            cameraController.TriggerShake(0.6f, 0.4f); // 0.6s duration, 0.4 intensity
        }
        
        if (deathSound != null) audioSource.PlayOneShot(deathSound);

        // Increment and save death count
        deathCount++;
        PlayerPrefs.SetInt("DeathCount", deathCount);
        PlayerPrefs.Save();
        UpdateDeathUI();

        // Show UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            reasonText.text = "Reason: " + reason;
            
            // Add a small hint to tap to restart at the end of the insult
            rageMessageText.text = rageMessages[Random.Range(0, rageMessages.Length)] + "\n\n(Tap to restart...)";
        }
    }

    public void WinLevel()
    {
        if (isGameOver || isLevelWon) return;

        isLevelWon = true;

        // Unlock the next level (only if this is the highest level they've beaten)
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (currentLevelNumber >= unlockedLevel)
        {
            PlayerPrefs.SetInt("UnlockedLevel", currentLevelNumber + 1);
            PlayerPrefs.Save();
        }

        if (winSound != null) audioSource.PlayOneShot(winSound);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            reasonText.text = "LEVEL PASSED.";
            
            if (currentLevelNumber == 1)
            {
                rageMessageText.text = "Wow, you beat the easiest level in gaming history. Do you want a medal for basic competence?\n\n(Tap to embarrass yourself in the next level...)";
            }
            else if (currentLevelNumber == 2)
            {
                rageMessageText.text = "You finally figured out the trick? Took you long enough. Go touch grass before Level 3 breaks you.\n\n(Tap to suffer...)";
            }
            else if (currentLevelNumber == 3)
            {
                rageMessageText.text = "You figured out you just had to walk backwards? Congrats, you have basic common sense. I bet you still fell for the fake gate at least 10 times though.\n\n(Tap to suffer...)";
            }
            else
            {
                rageMessageText.text = "You beat Level " + currentLevelNumber + ". Still doesn't change the fact that you're terrible at this game.\n\n(Tap to continue...)";
            }
        }
    }

    private void UpdateDeathUI()
    {
        if (deathCounterText != null)
        {
            deathCounterText.text = "DEATHS: " + deathCount;
        }
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
