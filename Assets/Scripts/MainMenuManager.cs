using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Text totalDeathCountText;
    
    [Header("Dynamic Grid Settings")]
    public GameObject levelButtonPrefab; // The button template to spawn
    public Transform levelGridParent;    // The Content object of the Scroll View
    public int totalLevels = 30;         // Total levels in your game

    void Start()
    {
        // 1. Display total deaths
        int deaths = PlayerPrefs.GetInt("DeathCount", 0);
        if (totalDeathCountText != null)
        {
            totalDeathCountText.text = "TOTAL DEATHS: " + deaths;
        }

        // 2. Generate the Level Grid
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 1; i <= totalLevels; i++)
        {
            // Spawn a new button from the prefab
            GameObject newBtnObj = Instantiate(levelButtonPrefab, levelGridParent);
            Button btn = newBtnObj.GetComponent<Button>();
            
            // Set the number on the button's text (assuming the Text is a child)
            Text btnText = newBtnObj.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.text = i.ToString();
            }

            // Check if this level should be unlocked or locked
            if (i <= unlockedLevel)
            {
                btn.interactable = true; // Unlocked
                
                // Add the click event via code!
                int levelIndexToLoad = i; // Store local copy for the button's memory
                btn.onClick.AddListener(() => LoadLevel(levelIndexToLoad));
            }
            else
            {
                btn.interactable = false; // Locked
                if (btnText != null) btnText.text = "🔒"; // Optional: show lock icon
            }
        }
    }

    public void LoadLevel(int levelNumber)
    {
        // Check if level actually exists before crashing
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
