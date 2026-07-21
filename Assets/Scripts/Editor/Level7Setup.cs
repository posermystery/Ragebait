using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Level7Setup : EditorWindow
{
    [MenuItem("Orbitwa/Setup Trick Circle Level (Level 7)")]
    public static void Setup()
    {
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var go in roots)
        {
            if (go.GetComponent<Camera>() != null) continue;
            Object.DestroyImmediate(go);
        }

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            var t = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (t != null) es.AddComponent(t);
            else           es.AddComponent<StandaloneInputModule>();
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            var cf = cam.GetComponent("CameraFollow");
            if (cf != null) Object.DestroyImmediate(cf);
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            var cObj = new GameObject("GameCanvas");
            canvas = cObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = cObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            cObj.AddComponent<GraphicRaycaster>();
        }

        // Logic manager
        var managerObj = new GameObject("TrickCircleManager");
        var logic = managerObj.AddComponent<TrickCircleLevel>();

        // Question container
        var qParent = new GameObject("QuestionTextContainer");
        qParent.transform.SetParent(canvas.transform, false);
        var qRect = qParent.AddComponent<RectTransform>();
        qRect.anchorMin = new Vector2(0f, 0.7f);
        qRect.anchorMax = new Vector2(1f, 0.9f);
        qRect.offsetMin = Vector2.zero;
        qRect.offsetMax = Vector2.zero;

        var hlg = qParent.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.spacing = 0f;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        int fSize = 130; // Bigger text to make the 'i' more visible

        var p1 = new GameObject("Text_1");
        p1.transform.SetParent(qParent.transform, false);
        var t1 = p1.AddComponent<Text>();
        t1.text = "choose the smallest c";
        t1.font = font;
        t1.fontSize = fSize;
        t1.color = Color.white;
        t1.alignment = TextAnchor.MiddleCenter;
        t1.horizontalOverflow = HorizontalWrapMode.Overflow;
        var cs1 = p1.AddComponent<ContentSizeFitter>();
        cs1.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        var p2 = new GameObject("Text_i");
        p2.transform.SetParent(qParent.transform, false);
        var t2 = p2.AddComponent<Text>();
        t2.text = "i";
        t2.font = font;
        t2.fontSize = fSize;
        t2.color = Color.white;
        t2.alignment = TextAnchor.MiddleCenter;
        t2.horizontalOverflow = HorizontalWrapMode.Overflow;
        var cs2 = p2.AddComponent<ContentSizeFitter>();
        cs2.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        var iBtn = p2.AddComponent<Button>();

        logic.correctButton = iBtn;

        var p3 = new GameObject("Text_3");
        p3.transform.SetParent(qParent.transform, false);
        var t3 = p3.AddComponent<Text>();
        t3.text = "rcle";
        t3.font = font;
        t3.fontSize = fSize;
        t3.color = Color.white;
        t3.alignment = TextAnchor.MiddleCenter;
        t3.horizontalOverflow = HorizontalWrapMode.Overflow;
        var cs3 = p3.AddComponent<ContentSizeFitter>();
        cs3.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Decoy circles
        var circleParent = new GameObject("DecoyCircles");
        circleParent.transform.SetParent(canvas.transform, false);
        var cpRect = circleParent.AddComponent<RectTransform>();
        cpRect.anchorMin = new Vector2(0.1f, 0.2f);
        cpRect.anchorMax = new Vector2(0.9f, 0.6f);
        cpRect.offsetMin = Vector2.zero;
        cpRect.offsetMax = Vector2.zero;

        var grid = circleParent.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(250, 250);
        grid.spacing = new Vector2(80, 80);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;

        Color[] colors = { new Color(0.9f, 0.2f, 0.2f), new Color(0.2f, 0.8f, 0.2f), new Color(0.2f, 0.4f, 0.9f), new Color(0.9f, 0.8f, 0.1f) };
        Sprite circleSprite = GetOrCreateCircleSprite();
        
        logic.decoyButtons = new Button[4];

        for (int i = 0; i < 4; i++)
        {
            var cObj = new GameObject("DecoyCircle_" + i);
            cObj.transform.SetParent(circleParent.transform, false);
            var img = cObj.AddComponent<Image>();
            img.sprite = circleSprite;
            img.color = colors[i];
            
            var btn = cObj.AddComponent<Button>();
            logic.decoyButtons[i] = btn;
            
            float scale = Random.Range(0.8f, 1.2f);
            cObj.transform.localScale = new Vector3(scale, scale, 1f);
        }
        
        Selection.activeGameObject = managerObj;
        Debug.Log("<color=lime><b>[Orbitwa]</b> Trick Circle Level (Level 7) generated!</color>");
    }

    private static Sprite GetOrCreateCircleSprite()
    {
        const string dir = "Assets/Sprites";
        const string path = dir + "/Circle.png";

        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "Sprites");

        int size = 256;
        var tex = new Texture2D(size, size);
        Color[] px = new Color[size * size];
        Vector2 center = new Vector2(size/2f, size/2f);
        float radius = size/2f - 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                    px[y * size + x] = new Color(1, 1, 1, alpha);
                }
                else
                {
                    px[y * size + x] = Color.clear;
                }
            }
        }
        tex.SetPixels(px);
        tex.Apply();

        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.alphaIsTransparency = true;
        imp.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
