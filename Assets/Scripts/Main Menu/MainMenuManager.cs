using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

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
    public Text settingsTauntText;
    [TextArea] public string tauntMessage = "Adjust Difficulty? Hah! What did you think this was, a fair game? Deal with it or quit.";

    [Header("Game Info")]
    public Text gameInfoText;
    [TextArea] public string customGameInfo = "Created by [Your Name]. A game designed to test your patience and sanity.";

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
    public GameObject levelButtonPrefab; // The button template to spawn
    public Transform levelGridParent;    // The Content object of the Scroll View
    public int totalLevels = 30;         // Total levels in your game
    public Sprite lockSprite;            // Assign your custom lock sprite here
    [Range(0.1f, 1.5f)]
    public float lockSpriteScale = 0.85f; // Scale of the lock sprite relative to button

    void Start()
    {
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
            totalDeathCountText.text = "TOTAL DEATHS: " + deaths;
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
            cg.blocksRaycasts = false; // Disable clicks
            cg.interactable = false;
        }

        yield return new WaitForSeconds(0.3f);

        if (cg != null)
        {
            cg.blocksRaycasts = true; // Re-enable clicks
            cg.interactable = true;
        }
    }

    private void GenerateLevelGrid()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 1; i <= totalLevels; i++)
        {
            GameObject newBtnObj = Instantiate(levelButtonPrefab, levelGridParent);
            Text btnText = newBtnObj.GetComponentInChildren<Text>();
            
            if (btnText != null) btnText.text = i.ToString();

            int levelNum = i; // Capture variable for closure
            Button btn = newBtnObj.GetComponent<Button>();
            if (btn != null)
            {
                if (i <= unlockedLevel)
                {
                    btn.interactable = true;
                    // Assign click event for level load and sound
                    btn.onClick.AddListener(() => LoadLevel(levelNum));
                    btn.onClick.AddListener(PlayClickSound);
                }
                else
                {
                    btn.interactable = false; 
                    if (btnText != null) btnText.text = ""; // Hide the number text
                    
                    // Add lock sprite image on top
                    if (lockSprite != null)
                    {
                        GameObject lockImgObj = new GameObject("LockIcon");
                        lockImgObj.transform.SetParent(newBtnObj.transform, false);
                        Image lockImg = lockImgObj.AddComponent<Image>();
                        lockImg.sprite = lockSprite;
                        lockImg.preserveAspect = true;
                        
                        RectTransform rt = lockImgObj.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(0.5f, 0.5f);
                        rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.anchoredPosition = Vector2.zero;
                        
                        // Set size relative to the button using the scale variable
                        RectTransform btnRect = newBtnObj.GetComponent<RectTransform>();
                        if (btnRect != null)
                        {
                            rt.sizeDelta = new Vector2(btnRect.sizeDelta.x * lockSpriteScale, btnRect.sizeDelta.y * lockSpriteScale);
                        }
                        else
                        {
                            rt.sizeDelta = new Vector2(80, 80);
                        }
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
            Debug.Log("No more levels! Restarting from Level 1.");
            PlayerPrefs.SetInt("UnlockedLevel", 1); // Progression reset karna ho toh ye line chalegi
            PlayerPrefs.Save();
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
        if (gameInfoText != null) gameInfoText.text = customGameInfo;
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
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Level " + levelNumber + " is not built yet!");
        }
    }
}
