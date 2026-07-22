using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Level8Setup : EditorWindow
{
    [MenuItem("Orbitwa/Setup Terms Level (Level 8)")]
    public static void Setup()
    {
        // -- CLEAR existing scene objects (keep Main Camera) --
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var go in roots)
        {
            if (go.GetComponent<Camera>() != null) continue;
            Object.DestroyImmediate(go);
        }

        // -- EventSystem --
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            var t = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (t != null) es.AddComponent(t);
            else           es.AddComponent<StandaloneInputModule>();
        }

        // -- Camera --
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        // -- UI Canvas --
        var cObj = new GameObject("GameCanvas");
        Canvas canvas = cObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = cObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        cObj.AddComponent<GraphicRaycaster>();
        
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // -- MAIN PANEL --
        var mainPanel = new GameObject("MainPanel");
        mainPanel.transform.SetParent(canvas.transform, false);
        var mainRect = mainPanel.AddComponent<RectTransform>();
        mainRect.anchorMin = Vector2.zero;
        mainRect.anchorMax = Vector2.one;
        mainRect.sizeDelta = Vector2.zero;
        
        var bgImg = mainPanel.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.12f); // Dark background

        // Title
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(mainPanel.transform, false);
        var titleText = titleObj.AddComponent<Text>();
        titleText.text = "TERMS & CONDITIONS";
        titleText.font = font;
        titleText.fontSize = 80;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.8f);
        titleRect.anchorMax = new Vector2(0.5f, 0.8f);
        titleRect.sizeDelta = new Vector2(1000, 200);

        // Buttons Container
        var btnContainer = new GameObject("ButtonContainer");
        btnContainer.transform.SetParent(mainPanel.transform, false);
        var btnRect = btnContainer.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.2f);
        btnRect.anchorMax = new Vector2(0.5f, 0.2f);
        btnRect.sizeDelta = new Vector2(1000, 200);
        
        var layoutGroup = btnContainer.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = 50;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;

        // Left Red Button
        var leftBtn = CreateButton(btnContainer.transform, "LeftBtn_Decline", "DECLINE", new Color(0.8f, 0.2f, 0.2f), font);
        // Center Blue Button
        var centerBtn = CreateButton(btnContainer.transform, "CenterBtn_ReadMore", "READ MORE", new Color(0.2f, 0.5f, 0.8f), font);
        // Right Green Button
        var rightBtn = CreateButton(btnContainer.transform, "RightBtn_Accept", "ACCEPT", new Color(0.2f, 0.8f, 0.2f), font);

        // -- TERMS PANEL --
        var termsPanel = new GameObject("TermsPanel");
        termsPanel.transform.SetParent(canvas.transform, false);
        var tPanelRect = termsPanel.AddComponent<RectTransform>();
        tPanelRect.anchorMin = Vector2.zero;
        tPanelRect.anchorMax = Vector2.one;
        tPanelRect.sizeDelta = Vector2.zero;
        var tBgImg = termsPanel.AddComponent<Image>();
        tBgImg.color = new Color(0.9f, 0.9f, 0.9f); // Light background for document

        // Scroll View Setup (Simplified)
        var scrollRectObj = new GameObject("ScrollArea");
        scrollRectObj.transform.SetParent(termsPanel.transform, false);
        var srRect = scrollRectObj.AddComponent<RectTransform>();
        srRect.anchorMin = new Vector2(0.1f, 0.3f);
        srRect.anchorMax = new Vector2(0.9f, 0.9f);
        srRect.sizeDelta = Vector2.zero;
        var srImg = scrollRectObj.AddComponent<Image>();
        srImg.color = Color.white;
        
        var viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollRectObj.transform, false);
        var vpRect = viewportObj.AddComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.sizeDelta = Vector2.zero;
        viewportObj.AddComponent<Image>();
        viewportObj.AddComponent<Mask>().showMaskGraphic = false;

        var contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        var contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 5000); // Increased height to fit all 20 clauses
        
        var scroll = scrollRectObj.AddComponent<ScrollRect>();
        scroll.content = contentRect;
        scroll.viewport = vpRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        // The Long Sarcastic Text
        var longTextObj = new GameObject("LongText");
        longTextObj.transform.SetParent(contentObj.transform, false);
        var longText = longTextObj.AddComponent<Text>();
        longText.text = "TERMS OF SUFFERING\n\n1. By accepting these terms, you agree to forfeit your sanity completely and absolutely.\n\n2. The developer reserves the right to mock you relentlessly in the event of any and all failures.\n\n3. You shall not complain about unfair mechanics on any social media platform.\n\n4. If you fail, it is a skill issue. Legally and morally.\n\n5. You must read this entire document, even though we both know you are just skimming.\n\n6. By continuing, you agree that Pineapple belongs on pizza. Non-negotiable.\n\n7. You hereby grant us permission to use your tears to salt our popcorn during movie nights.\n\n8. If you break your phone in rage, we are not liable. Your anger issues are your own personal demons to fight.\n\n9. Every time you die in this game, a kitten gets slightly annoyed somewhere in the world.\n\n10. You confirm you are not a robot, although your repetitive failures suggest otherwise.\n\n11. We reserve the right to change these terms whenever we feel like it, specifically to make you suffer more.\n\n12. In accordance with Subsection 4, Paragraph B: Any attempt to cheat, glitch, or bypass mechanics will result in immediate judgment by the code.\n\n13. We do not collect your personal data, because frankly, your gameplay data is embarrassing enough.\n\n14. Are you actually still reading this? This is why you don't have a girlfriend.\n\n15. We are legally obligated to inform you of the following mechanical change: Upon returning from this agreement, the functionality of the Accept and Decline buttons will be entirely reversed.\n\n16. Any damage caused to your device by throwing it against a wall is strictly your responsibility and will not be reimbursed.\n\n17. Seriously, go outside. Touch some grass. Get some vitamin D.\n\n18. The left button was red, the right button was green. Did you memorize that? Good.\n\n19. By checking the box below, you swear under penalty of perjury that you read all of this. We both know you're a liar.\n\n20. If you survived reading all this, you deserve a medal. But you won't get one. Just pain.";
        longText.font = font;
        longText.fontSize = 45;
        longText.color = Color.black;
        var ltRect = longTextObj.GetComponent<RectTransform>();
        ltRect.anchorMin = Vector2.zero;
        ltRect.anchorMax = Vector2.one;
        ltRect.sizeDelta = new Vector2(-40, -40);

        // Toggle Checkbox
        var toggleObj = new GameObject("AcceptToggle");
        toggleObj.transform.SetParent(termsPanel.transform, false);
        var toggleRect = toggleObj.AddComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0.5f, 0.2f);
        toggleRect.anchorMax = new Vector2(0.5f, 0.2f);
        toggleRect.sizeDelta = new Vector2(800, 100);
        var toggle = toggleObj.AddComponent<Toggle>();

        var bgToggleObj = new GameObject("Background");
        bgToggleObj.transform.SetParent(toggleObj.transform, false);
        var bgToggleRect = bgToggleObj.AddComponent<RectTransform>();
        bgToggleRect.anchorMin = new Vector2(0, 0.5f);
        bgToggleRect.anchorMax = new Vector2(0, 0.5f);
        bgToggleRect.sizeDelta = new Vector2(80, 80);
        bgToggleRect.anchoredPosition = new Vector2(40, 0);
        var toggleBgImg = bgToggleObj.AddComponent<Image>();
        toggleBgImg.color = Color.white;

        var checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(bgToggleObj.transform, false);
        var checkRect = checkObj.AddComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.5f, 0.5f);
        checkRect.anchorMax = new Vector2(0.5f, 0.5f);
        checkRect.sizeDelta = new Vector2(50, 50);
        var checkImg = checkObj.AddComponent<Image>();
        checkImg.color = Color.black; // Simple black square as checkmark
        
        toggle.targetGraphic = toggleBgImg;
        toggle.graphic = checkImg;
        toggle.isOn = false;

        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(toggleObj.transform, false);
        var labelTxt = labelObj.AddComponent<Text>();
        labelTxt.text = "I have read and accept all these ridiculous terms.";
        labelTxt.font = font;
        labelTxt.fontSize = 40;
        labelTxt.color = Color.black;
        labelTxt.alignment = TextAnchor.MiddleLeft;
        var labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = new Vector2(100, 0);
        labelRect.offsetMax = new Vector2(0, 0);

        // Back Button
        var backBtn = CreateButton(termsPanel.transform, "BackBtn", "BACK", Color.gray, font);
        var backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.5f, 0.1f);
        backRect.anchorMax = new Vector2(0.5f, 0.1f);
        backRect.sizeDelta = new Vector2(400, 150);

        termsPanel.SetActive(false);

        // -- MANAGER SCRIPT --
        var managerObj = new GameObject("Level8Manager");
        var termsScript = managerObj.AddComponent<TermsLevel>();
        termsScript.mainPanel = mainPanel;
        termsScript.termsPanel = termsPanel;
        termsScript.leftButton = leftBtn;
        termsScript.centerButton = centerBtn;
        termsScript.rightButton = rightBtn;
        termsScript.acceptToggle = toggle;
        termsScript.backButton = backBtn;

        Selection.activeGameObject = managerObj;
        Debug.Log("<color=cyan><b>[Orbitwa]</b> Terms Level (Level 8) generated!</color>");
    }

    private static Button CreateButton(Transform parent, string name, string text, Color color, Font font)
    {
        var btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        
        var img = btnObj.AddComponent<Image>();
        img.color = color;
        
        var btn = btnObj.AddComponent<Button>();

        var txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        var txt = txtObj.AddComponent<Text>();
        txt.text = text;
        txt.font = font;
        txt.fontSize = 50;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        
        var txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;

        return btn;
    }
}

