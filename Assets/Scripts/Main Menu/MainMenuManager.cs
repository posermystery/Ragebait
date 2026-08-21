using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

[System.Serializable]
public class LevelData
{
    public string levelTitle;
    public Sprite levelThumbnail;
}

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject settingsPanel;
    public GameObject gameInfoPanel;

    [Header("Main Menu UI Elements")]
    public Text totalDeathCountText;
    
    [Header("Settings / Taunt")]
    public TextMeshProUGUI settingsTauntText;
    [TextArea] public string tauntMessage = "Adjust Difficulty? Hah! What did you think this was, a fair game? Deal with it or quit.";

    [Header("Game Info")]
    public TextMeshProUGUI gameInfoText;

    [Header("Audio (Drag n Drop SFX here)")]
    public AudioClip clickSound;
    private AudioSource audioSource;

    [Header("Main Buttons (Drag n Drop here!)")]
    public Button startButton;
    public Button levelSelectButton;
    public Button settingsButton;
    public Button gameInfoButton;
    public Button exitButton;

    [Header("Back Buttons (Drag n Drop here!)")]
    public Button settingsBackButton;
    public Button levelSelectBackButton;
    public Button gameInfoBackButton;

    [Header("Dynamic Grid Settings")]
    public GameObject levelButtonPrefab; // The Level Card prefab
    public Transform levelGridParent;    
    public int totalLevels = 30;         
    
    [Header("Level Card Data")]
    public LevelData[] levelDataList;    // Assign level names and thumbnails here in Inspector
    public Sprite lockSprite;            
    public Sprite completedStarSprite;   
    [Range(0.1f, 1.5f)]
    public float lockSpriteScale = 0.85f;

    void Start()
    {
        // 🚀 MOBILE OPTIMIZATION 🚀
        // Unlock FPS to 60 for buttery smooth gameplay (Unity defaults to 30 on mobile)
        Application.targetFrameRate = 60;
        
        // Prevent screen from dimming/sleeping while they are raging
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Setup AudioSource automatically
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // 1. Show Main Menu by default, hide others
        BackToMainMenu();

        // Automatically connect all buttons via code so you don't have to!
        if (startButton != null) { startButton.onClick.AddListener(ClickStartGame); startButton.onClick.AddListener(PlayClickSound); }
        if (levelSelectButton != null) { levelSelectButton.onClick.AddListener(ClickLevelSelect); levelSelectButton.onClick.AddListener(PlayClickSound); }
        if (settingsButton != null) { settingsButton.onClick.AddListener(ClickSettings); settingsButton.onClick.AddListener(PlayClickSound); }
        if (gameInfoButton != null) { gameInfoButton.onClick.AddListener(ClickGameInfo); gameInfoButton.onClick.AddListener(PlayClickSound); }
        if (exitButton != null) { exitButton.onClick.AddListener(ClickExit); exitButton.onClick.AddListener(PlayClickSound); }

        // Connect Back Buttons
        if (settingsBackButton != null) { settingsBackButton.onClick.AddListener(BackToMainMenu); settingsBackButton.onClick.AddListener(PlayClickSound); }
        if (levelSelectBackButton != null) { levelSelectBackButton.onClick.AddListener(BackToMainMenu); levelSelectBackButton.onClick.AddListener(PlayClickSound); }
        if (gameInfoBackButton != null) { gameInfoBackButton.onClick.AddListener(BackToMainMenu); gameInfoBackButton.onClick.AddListener(PlayClickSound); }

        // 2. Display total deaths
        int deaths = PlayerPrefs.GetInt("DeathCount", 0);
        if (totalDeathCountText != null)
        {
            totalDeathCountText.text = deaths + " deaths by the way";
        }

        // 3. Generate the Level Grid (Hidden until Level Select is clicked)
        GenerateLevelGrid();

        // 4. Block input briefly so "tap to restart" from previous scene doesn't bleed through
        StartCoroutine(BlockInputBriefly());
    }

    private IEnumerator BlockInputBriefly()
    {
        // Add a CanvasGroup to block all raycasts temporarily
        CanvasGroup cg = null;
        if (mainMenuPanel != null)
        {
            cg = mainMenuPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = mainMenuPanel.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false; // Disable clicks but don't change visual interactable state
        }

        // Use Realtime so it doesn't break if timeScale is 0
        yield return new WaitForSecondsRealtime(0.1f);

        if (cg != null)
        {
            cg.blocksRaycasts = true; // Re-enable clicks
        }
    }

    private void GenerateLevelGrid()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        
        // Check if player has finished the game/all levels. We can use a special PlayerPref for this if needed, 
        // but for now we'll assume a level is "Completed" if its index is strictly less than unlockedLevel.

        for (int i = 1; i <= totalLevels; i++)
        {
            GameObject newBtnObj = Instantiate(levelButtonPrefab, levelGridParent);
            int levelNum = i;
            Button btn = newBtnObj.GetComponent<Button>();

            // Try to find the specific UI parts inside the Prefab by name
            Transform thumbTransform = newBtnObj.transform.Find("Thumbnail");
            Transform titleTransform = newBtnObj.transform.Find("TitleText");
            Transform statusTransform = newBtnObj.transform.Find("StatusText");
            Transform iconTransform = newBtnObj.transform.Find("StatusIcon");

            Image thumbImg = thumbTransform != null ? thumbTransform.GetComponent<Image>() : null;
            TextMeshProUGUI titleText = titleTransform != null ? titleTransform.GetComponent<TextMeshProUGUI>() : null;
            TextMeshProUGUI statusText = statusTransform != null ? statusTransform.GetComponent<TextMeshProUGUI>() : null;
            Image statusIcon = iconTransform != null ? iconTransform.GetComponent<Image>() : null;

            // Apply Level Data (if we entered it in the inspector)
            string defaultTitle = (i < 10 ? "0" + i : i.ToString()) + ": UNKNOWN";
            if (levelDataList != null && i - 1 < levelDataList.Length)
            {
                if (titleText != null) titleText.text = (i < 10 ? "0" + i : i.ToString()) + ": " + levelDataList[i - 1].levelTitle;
                if (thumbImg != null && levelDataList[i - 1].levelThumbnail != null) thumbImg.sprite = levelDataList[i - 1].levelThumbnail;
            }
            else
            {
                if (titleText != null) titleText.text = defaultTitle;
            }

            if (btn != null)
            {
                if (i <= unlockedLevel) // UNLOCKED (or previously completed)
                {
                    btn.interactable = true;
                    btn.onClick.AddListener(() => LoadLevel(levelNum));
                    btn.onClick.AddListener(PlayClickSound);

                    if (statusText != null) { statusText.text = "Unlocked"; statusText.color = Color.white; }
                    if (statusIcon != null) { statusIcon.gameObject.SetActive(false); } // No icon, just text
                }
                else // LOCKED
                {
                    btn.interactable = false;
                    
                    if (statusText != null) { statusText.text = "Locked"; statusText.color = Color.red; }
                    
                    // Dim the thumbnail for locked levels
                    if (thumbImg != null) thumbImg.color = new Color(0.3f, 0.3f, 0.3f, 1f); 

                    if (statusIcon != null && lockSprite != null) 
                    { 
                        statusIcon.gameObject.SetActive(true); 
                        statusIcon.sprite = lockSprite; 
                    }
                }
            }
        }
    }

    // --- BUTTON FUNCTIONS ---

    public void ClickStartGame()
    {
        // Loads the highest unlocked level
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        string sceneName = "Level" + unlockedLevel;

        // Agar scene build me nahi hai (yani aage koi level nahi bacha), toh Level 1 se shuru karo
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            LoadLevel(unlockedLevel);
        }
        else
        {
            Debug.Log("No more levels! Loading Level 1 without resetting unlocked progress.");
            LoadLevel(1);
        }
    }

    public void ClickLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    public void ClickSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        if (settingsTauntText != null) settingsTauntText.text = tauntMessage;
    }

    public void ClickGameInfo()
    {
        mainMenuPanel.SetActive(false);
        gameInfoPanel.SetActive(true);
        if (gameInfoText != null) 
        {
            gameInfoText.text = "Hi, I'm Sanskar, the developer of this game.\n\nThis project started out as my college assignment while I was learning Unity. After spending countless hours getting enraged over fixing endless bugs in my code, I decided it was only fair to pass that frustration on to someone else.\n\nSo, I built this game with one simple goal: to ragebait all of you. Enjoy the suffering!";
        }
    }

    public void ClickExit()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }

    private void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public void BackToMainMenu()
    {
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameInfoPanel != null) gameInfoPanel.SetActive(false);
        
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    private void LoadLevel(int levelNumber)
    {
        string sceneName = "Level" + levelNumber;
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            if (TransitionManager.Instance != null) TransitionManager.Instance.LoadLevelWithNumber(sceneName, levelNumber, false);
            else SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Level " + levelNumber + " is not built yet!");
        }
    }
}
