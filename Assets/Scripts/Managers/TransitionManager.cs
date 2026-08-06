using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Header("UI Elements")]
    public RectTransform leftDoor;
    public RectTransform rightDoor;
    public Text levelText;
    
    [Header("Animation Settings")]
    public float transitionSpeed = 1.5f;
    public float holdDuration = 2.0f; // Time to show the text

    [Header("Level Subtitles")]
    [Tooltip("Index 0 = Level 1, Index 1 = Level 2, etc. Leave blank for no subtitle.")]
    public string[] levelNames;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Force the canvas to be on top of EVERYTHING
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 9999;
            }
            
            if(levelText != null) levelText.gameObject.SetActive(false);
            
            // Ensure doors are fully open on awake and have perfect anchors!
            if (leftDoor != null && rightDoor != null)
            {
                // Force Left Door to anchor at the center, pivot on its right edge
                leftDoor.anchorMin = new Vector2(0.5f, 0);
                leftDoor.anchorMax = new Vector2(0.5f, 1);
                leftDoor.pivot = new Vector2(1, 0.5f);
                leftDoor.sizeDelta = new Vector2(2000, 0); // Massive width to ensure coverage

                // Force Right Door to anchor at the center, pivot on its left edge
                rightDoor.anchorMin = new Vector2(0.5f, 0);
                rightDoor.anchorMax = new Vector2(0.5f, 1);
                rightDoor.pivot = new Vector2(0, 0.5f);
                rightDoor.sizeDelta = new Vector2(2000, 0);

                float moveDistance = leftDoor.rect.width;
                leftDoor.anchoredPosition = new Vector2(-moveDistance, 0);
                rightDoor.anchoredPosition = new Vector2(moveDistance, 0);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName, string textToDisplay)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneName, textToDisplay, false));
    }
    
    public void LoadScene(int sceneIndex, string textToDisplay)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneIndex.ToString(), textToDisplay, true));
    }

    public void LoadLevelWithNumber(string sceneIdentifier, int levelNumber, bool isIndex = false)
    {
        string text = "LEVEL " + levelNumber;
        
        // Add custom subtitle if it exists in the array
        if (levelNames != null && levelNumber >= 1 && levelNumber <= levelNames.Length)
        {
            string customName = levelNames[levelNumber - 1];
            if (!string.IsNullOrEmpty(customName))
            {
                // Adds a newline and slightly smaller text for the subtitle
                text += "\n<size=50>" + customName + "</size>"; 
            }
        }
        
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneIdentifier, text, isIndex));
    }

    private IEnumerator TransitionRoutine(string sceneIdentifier, string textToDisplay, bool isIndex)
    {
        isTransitioning = true;
        
        // 1. Pause the game completely so gameplay stops (Gyro won't kill player)
        Time.timeScale = 0f;

        float moveDistance = leftDoor.rect.width;
        
        // 2. Close doors
        float t = 0;
        while (t < 1f)
        {
            // Using unscaledDeltaTime because timeScale is 0!
            t += Time.unscaledDeltaTime * transitionSpeed;
            float easedT = Mathf.SmoothStep(0, 1, t); // Smooth easing
            
            leftDoor.anchoredPosition = new Vector2(Mathf.Lerp(-moveDistance, 0, easedT), 0);
            rightDoor.anchoredPosition = new Vector2(Mathf.Lerp(moveDistance, 0, easedT), 0);
            yield return null;
        }

        // Doors fully closed, now ensure they are perfectly centered
        leftDoor.anchoredPosition = Vector2.zero;
        rightDoor.anchoredPosition = Vector2.zero;

        // 3. Show the text ONLY when doors are fully closed
        if (levelText != null)
        {
            levelText.text = textToDisplay;
            levelText.gameObject.SetActive(true);
        }

        // 4. Asynchronously load the scene in the background
        AsyncOperation asyncLoad;
        if (isIndex)
        {
            int index = int.Parse(sceneIdentifier);
            asyncLoad = SceneManager.LoadSceneAsync(index);
        }
        else
        {
            asyncLoad = SceneManager.LoadSceneAsync(sceneIdentifier);
        }

        asyncLoad.allowSceneActivation = false;

        // 5. Hold the screen so the player can read the text
        float holdTimer = 0;
        while (holdTimer < holdDuration)
        {
            holdTimer += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // 6. Activate the new scene
        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Extremely important: Wait a couple of frames for the scene to fully initialize!
        // Otherwise Time.unscaledDeltaTime will be huge (e.g. 1 second) because of the load,
        // and the door opening animation will skip entirely.
        yield return null;
        yield return null;

        // 7. Hide text before opening doors
        if (levelText != null)
        {
            levelText.gameObject.SetActive(false);
        }

        // 8. Open doors
        t = 0;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * transitionSpeed;
            float easedT = Mathf.SmoothStep(0, 1, t);
            
            leftDoor.anchoredPosition = new Vector2(Mathf.Lerp(0, -moveDistance, easedT), 0);
            rightDoor.anchoredPosition = new Vector2(Mathf.Lerp(0, moveDistance, easedT), 0);
            yield return null;
        }
        
        // Ensure they are fully pushed out
        leftDoor.anchoredPosition = new Vector2(-moveDistance, 0);
        rightDoor.anchoredPosition = new Vector2(moveDistance, 0);

        // 9. Resume game completely
        Time.timeScale = 1f;
        isTransitioning = false;
    }
}
