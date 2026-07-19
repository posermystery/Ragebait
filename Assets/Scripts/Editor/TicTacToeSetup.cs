using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TicTacToeSetup : EditorWindow
{
    [MenuItem("Orbitwa/Setup Troll Tic-Tac-Toe Level")]
    public static void SetupTicTacToeScene()
    {
        // 1. Create Event System if not exists
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            // Using the new Input System's UI module instead of the old StandaloneInputModule
            System.Type inputModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputModuleType != null)
            {
                eventSystem.AddComponent(inputModuleType);
            }
            else
            {
                eventSystem.AddComponent<StandaloneInputModule>(); // Fallback
            }
        }

        // 2. Create Canvas if not exists
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 3. Create Main Board Panel
        GameObject boardObj = new GameObject("TicTacToe Board");
        boardObj.transform.SetParent(canvas.transform, false);
        
        RectTransform boardRect = boardObj.AddComponent<RectTransform>();
        boardRect.sizeDelta = new Vector2(900, 900); // Increased size
        boardRect.anchoredPosition = Vector2.zero; // Center of screen

        Image boardImage = boardObj.AddComponent<Image>();
        boardImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark background for the board

        GridLayoutGroup grid = boardObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(280, 280); // Increased button size
        grid.spacing = new Vector2(15, 15);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.MiddleCenter;

        // 4. Create TicTacToe Manager
        GameObject managerObj = new GameObject("TicTacToe Manager");
        TicTacToeTroll trollScript = managerObj.AddComponent<TicTacToeTroll>();
        trollScript.buttons = new Button[9];

        // 5. Create 9 Buttons and assign them
        for (int i = 0; i < 9; i++)
        {
            // Button Object
            GameObject btnObj = new GameObject("Button_" + i);
            btnObj.transform.SetParent(boardObj.transform, false);
            
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = Color.white;
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImage; // This is crucial for clicks to register!
            trollScript.buttons[i] = btn;

            // Text inside Button
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            Text btnText = textObj.AddComponent<Text>();
            btnText.text = "";
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 120;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.black;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero; // Fill entire button
        }

        // 6. Focus on Manager object so user can add Audio Clips
        Selection.activeGameObject = managerObj;

        Debug.Log("<color=green><b>Success:</b> Tic-Tac-Toe Board Generated Successfully! Just add your GameManager prefab to the scene.</color>");
    }
}
