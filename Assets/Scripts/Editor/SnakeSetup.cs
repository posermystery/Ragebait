using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SnakeSetup : EditorWindow
{
    [MenuItem("Orbitwa/Setup Snake Level (Level 6)")]
    public static void Setup()
    {
        // ── CLEAR existing scene objects (keep Main Camera) ──
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var go in roots)
        {
            if (go.GetComponent<Camera>() != null) continue;
            Object.DestroyImmediate(go);
        }

        // ── EventSystem ──
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            var t = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (t != null) es.AddComponent(t);
            else           es.AddComponent<StandaloneInputModule>();
        }

        // ── Camera ──
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.08f);
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.transform.position = new Vector3(0f, 0f, -10f);

            // Remove CameraFollow if exists, Snake level is usually static camera
            var cf = cam.GetComponent("CameraFollow");
            if (cf != null) Object.DestroyImmediate(cf);
        }

        // ── Grid Background Visuals ──
        var bgObj = new GameObject("GridBackground");
        bgObj.transform.position = Vector3.zero;
        var bgSR = bgObj.AddComponent<SpriteRenderer>();
        bgSR.sprite = GetOrCreateWhiteSprite();
        bgSR.color = new Color(0.12f, 0.12f, 0.12f);
        bgSR.sortingOrder = -10;
        bgObj.transform.localScale = new Vector3(10f, 10f, 1f); // 20 grid width * 0.5 cell size = 10f

        var borderObj = new GameObject("GridBorder");
        borderObj.transform.position = Vector3.zero;
        var borderSR = borderObj.AddComponent<SpriteRenderer>();
        borderSR.sprite = GetOrCreateWireframeSprite();
        borderSR.color = new Color(0.3f, 0.3f, 0.3f);
        borderSR.sortingOrder = -9;
        borderObj.transform.localScale = new Vector3(10.2f, 10.2f, 1f); 

        // ── Snake ──
        var snakeObj = new GameObject("SnakeController");
        var snake = snakeObj.AddComponent<SnakeController>();
        snake.gridWidth = 20;
        snake.gridHeight = 20;
        snake.cellSize = 0.5f;

        // ── Apple ──
        var appleObj = new GameObject("Apple");
        var apple = appleObj.AddComponent<AppleTroll>();
        apple.snake = snake;
        snake.apple = apple;

        // ── UI Canvas ──
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            var cObj = new GameObject("GameCanvas");
            canvas = cObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = cObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            cObj.AddComponent<GraphicRaycaster>();
        }

        // Troll Message Text
        var msgObj = new GameObject("TrollMessage");
        msgObj.transform.SetParent(canvas.transform, false);
        var msgText = msgObj.AddComponent<Text>();
        msgText.text = "Oops, you missed!";
        msgText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        msgText.fontSize = 80;
        msgText.fontStyle = FontStyle.Bold;
        msgText.alignment = TextAnchor.MiddleCenter;
        msgText.color = Color.yellow;

        var msgRect = msgObj.GetComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0.5f, 0.7f);
        msgRect.anchorMax = new Vector2(0.5f, 0.7f);
        msgRect.pivot = new Vector2(0.5f, 0.5f);
        msgRect.anchoredPosition = Vector2.zero;
        msgRect.sizeDelta = new Vector2(800, 200);

        // Add outline to text for readability
        var outline = msgObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(3, -3);

        var levelUI = canvas.gameObject.AddComponent<SnakeLevelUI>();
        levelUI.trollMessageText = msgText;
        apple.levelUI = levelUI;
        snake.levelUI = levelUI;

        // Hint Text for mobile swipe
        var hintObj = new GameObject("SwipeHint");
        hintObj.transform.SetParent(canvas.transform, false);
        var hintText = hintObj.AddComponent<Text>();
        hintText.text = "Swipe to Move";
        hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hintText.fontSize = 48;
        hintText.alignment = TextAnchor.LowerCenter;
        hintText.color = new Color(1f, 1f, 1f, 0.6f);

        var hintRect = hintObj.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 0.05f);
        hintRect.anchorMax = new Vector2(0.5f, 0.05f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.anchoredPosition = Vector2.zero;
        hintRect.sizeDelta = new Vector2(600, 80);

        // Add outline to make the text sharper and clearer against the background
        var hintOutline = hintObj.AddComponent<Outline>();
        hintOutline.effectColor = new Color(0, 0, 0, 0.5f);
        hintOutline.effectDistance = new Vector2(2, -2);

        Selection.activeGameObject = snakeObj;
        Debug.Log("<color=lime><b>[Orbitwa]</b> Snake Level (Level 6) generated! " +
                  "Assign your custom Sprites to SnakeController and AppleTroll!</color>");
    }

    private static Sprite GetOrCreateWhiteSprite()
    {
        const string dir = "Assets/Sprites";
        const string path = dir + "/WhiteSquare.png";

        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "Sprites");

        var tex = new Texture2D(4, 4);
        var px = new Color[16];
        for (int i = 0; i < 16; i++) px[i] = Color.white;
        tex.SetPixels(px); tex.Apply();

        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spritePixelsPerUnit = 4;
        imp.filterMode = FilterMode.Point;
        imp.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite GetOrCreateWireframeSprite()
    {
        const string dir = "Assets/Sprites";
        const string path = dir + "/WireSquare.png";

        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "Sprites");

        var tex = new Texture2D(32, 32);
        var px = new Color[32 * 32];
        for (int i = 0; i < 32 * 32; i++)
        {
            int x = i % 32;
            int y = i / 32;
            // Border is white, inside is clear
            if (x == 0 || x == 31 || y == 0 || y == 31)
                px[i] = Color.white;
            else
                px[i] = Color.clear;
        }
        tex.SetPixels(px); tex.Apply();

        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spritePixelsPerUnit = 32;
        imp.filterMode = FilterMode.Point;
        imp.alphaIsTransparency = true;
        imp.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
