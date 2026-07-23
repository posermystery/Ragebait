using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Classic Nokia-style snake controller.
/// Grid-based movement, supports keyboard + touch swipe for mobile.
/// </summary>
public class SnakeController : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth  = 20;
    public int gridHeight = 20;
    public float cellSize = 0.5f;

    [Header("Speed")]
    [Tooltip("Seconds between each move step")]
    public float moveInterval = 0.18f;

    [Header("Sprites — Assign in Inspector")]
    public Sprite headSprite; // drag your head sprite here
    public Sprite bodySprite; // drag your body sprite here
    public Color  snakeColor  = new Color(0.2f, 0.85f, 0.3f);

    [Header("References")]
    public AppleTroll   apple;
    public SnakeLevelUI levelUI;

    // ─── Internal ───
    private List<Vector2Int>     segments  = new List<Vector2Int>();
    private List<GameObject>     segGOs    = new List<GameObject>();
    private Vector2Int direction           = Vector2Int.right;
    private Queue<Vector2Int> inputQueue   = new Queue<Vector2Int>();
    private float      moveTimer           = 0f;
    private bool       isDead              = false;
    private bool       inputLocked         = true;

    // Ragebait Extra Apple feature
    private bool       wasAdjacentToApple  = false;
    private List<Vector2Int> extraApples   = new List<Vector2Int>();
    private List<GameObject> extraAppleGOs = new List<GameObject>();

    // Touch swipe
    private Vector2 swipeStart;
    private bool    swiping = false;

    // Shared white sprite
    private Sprite whiteSprite;

    public int SegmentCount => segments.Count;

    // ─── Unity ───
    void Start()
    {
        // Hook up D-Pad buttons at runtime (Editor listeners don't persist)
        var upBtn = GameObject.Find("UpBtn")?.GetComponent<UnityEngine.UI.Button>();
        if (upBtn != null) upBtn.onClick.AddListener(MoveUp);

        var downBtn = GameObject.Find("DownBtn")?.GetComponent<UnityEngine.UI.Button>();
        if (downBtn != null) downBtn.onClick.AddListener(MoveDown);

        var leftBtn = GameObject.Find("LeftBtn")?.GetComponent<UnityEngine.UI.Button>();
        if (leftBtn != null) leftBtn.onClick.AddListener(MoveLeft);

        var rightBtn = GameObject.Find("RightBtn")?.GetComponent<UnityEngine.UI.Button>();
        if (rightBtn != null) rightBtn.onClick.AddListener(MoveRight);

        whiteSprite = MakeWhiteSprite();

        // Start with 4 segments (head + 3 body parts)
        Vector2Int head = new Vector2Int(gridWidth / 2, gridHeight / 2);
        segments.Add(head);
        segments.Add(new Vector2Int(head.x - 1, head.y));
        segments.Add(new Vector2Int(head.x - 2, head.y));
        segments.Add(new Vector2Int(head.x - 3, head.y));

        RebuildVisuals();
        StartCoroutine(UnlockAfterDelay(0.5f));
    }

    void Update()
    {
        if (isDead || inputLocked) return;

        ReadInput();

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            Step();
        }
    }

    // ─── Input ───
    private void ReadInput()
    {
        // Keyboard
        if (Keyboard.current != null)
        {
            var kb = Keyboard.current;
            if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame) EnqueueInput(Vector2Int.up);
            if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame) EnqueueInput(Vector2Int.down);
            if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame) EnqueueInput(Vector2Int.left);
            if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame) EnqueueInput(Vector2Int.right);
        }

        // Touch swipe (Android)
        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                var phase = touch.phase.ReadValue();
                
                if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    // Ignore swipes that start on UI elements (like the D-Pad)
                    if (UnityEngine.EventSystems.EventSystem.current != null && 
                        UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.touchId.ReadValue()))
                    {
                        swiping = false;
                        continue;
                    }

                    swipeStart = touch.position.ReadValue();
                    swiping    = true;
                }
                else if (swiping && phase == UnityEngine.InputSystem.TouchPhase.Ended)
                {
                    swiping = false;
                    Vector2 delta = touch.position.ReadValue() - swipeStart;
                    if (delta.magnitude > 50f)
                    {
                        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                        {
                            if (delta.x > 0) EnqueueInput(Vector2Int.right);
                            else EnqueueInput(Vector2Int.left);
                        }
                        else
                        {
                            if (delta.y > 0) EnqueueInput(Vector2Int.up);
                            else EnqueueInput(Vector2Int.down);
                        }
                    }
                }
            }
        }
    }

    // ─── One grid step ───
    private void Step()
    {
        if (inputQueue.Count > 0)
        {
            direction = inputQueue.Dequeue();
        }
        
        Vector2Int newHead = segments[0] + direction;

        // Wall collision → wrap (Nokia style) or die — here we wrap
        newHead.x = (newHead.x + gridWidth)  % gridWidth;
        newHead.y = (newHead.y + gridHeight) % gridHeight;

        for (int i = 0; i < segments.Count - 1; i++)
        {
            if (newHead == segments[i])
            {
                Die("You bit yourself. Classic.");
                return;
            }
        }

        bool ateMainApple = (newHead == apple.GetGridPos());
        
        // Did we eat an extra apple?
        bool ateExtraApple = extraApples.Contains(newHead);
        if (ateExtraApple)
        {
            int idx = extraApples.IndexOf(newHead);
            extraApples.RemoveAt(idx);
            
            GameObject go = extraAppleGOs[idx];
            extraAppleGOs.RemoveAt(idx);
            Destroy(go);

            // Check if we just cleared the last extra apple and already ate 5 main apples!
            if (apple != null && apple.applesEaten >= 5 && extraApples.Count == 0)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.WinLevel();
                }
            }
        }

        Vector2Int oldTail = segments[segments.Count - 1];

        // --- RAGEBAIT EXTRA APPLE MECHANIC ---
        // If we were adjacent to the MAIN apple but didn't eat it this step, we missed it!
        // We poop an EXTRA apple that makes the snake grow but doesn't count towards winning!
        if (wasAdjacentToApple && !ateMainApple)
        {
            extraApples.Add(oldTail);
            
            var go = new GameObject("ExtraApple");
            go.transform.position = GridToWorld(oldTail);
            go.transform.localScale = Vector3.one * (cellSize * 0.88f);
            
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3; // Behind snake
            sr.sprite = apple.appleSprite != null ? apple.appleSprite : whiteSprite; 
            sr.color = apple.appleSprite == null ? Color.red : Color.white;
            
            extraAppleGOs.Add(go);
        }

        segments.Insert(0, newHead);

        bool grewThisStep = ateMainApple || ateExtraApple;

        if (!grewThisStep)
            segments.RemoveAt(segments.Count - 1);
        
        if (ateMainApple)
            apple.OnEaten(this);

        // Update adjacency for NEXT step (only relative to the MAIN apple)
        wasAdjacentToApple = (DistanceTo(apple.GetGridPos()) == 1);

        RebuildVisuals();
    }

    // ─── Rebuild sprite objects for every segment ───
    private void RebuildVisuals()
    {
        // Destroy old
        foreach (var go in segGOs)
            if (go != null) Destroy(go);
        segGOs.Clear();

        float scale = cellSize * 0.88f;

        for (int i = 0; i < segments.Count; i++)
        {
            var go = new GameObject(i == 0 ? "SnakeHead" : "SnakeSeg_" + i);
            go.transform.position   = GridToWorld(segments[i]);
            go.transform.localScale = Vector3.one * scale;

            var sr          = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
            sr.color        = snakeColor;

            // Head vs body sprite
            Sprite sp = (i == 0) ? headSprite : bodySprite;
            sr.sprite = (sp != null) ? sp : whiteSprite;

            segGOs.Add(go);
        }
    }

    // ─── Death ───
    private void Die(string msg = null)
    {
        isDead = true;
        string[] msgs = {
            "You bit yourself. Classic.",
            "Self-destruction speedrun any%",
            "Snake ate snake. The ouroboros wins.",
        };
        
        string finalMsg = msg != null ? msg : msgs[Random.Range(0, msgs.Length)];
        
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver(finalMsg);
    }

    public void MoveUp() { EnqueueInput(Vector2Int.up); }
    public void MoveDown() { EnqueueInput(Vector2Int.down); }
    public void MoveLeft() { EnqueueInput(Vector2Int.left); }
    public void MoveRight() { EnqueueInput(Vector2Int.right); }

    private void EnqueueInput(Vector2Int dir)
    {
        // Prevent reversing back into the body
        Vector2Int lastDir = inputQueue.Count > 0 ? inputQueue.ToArray()[inputQueue.Count - 1] : direction;
        
        if (dir != -lastDir && dir != lastDir)
        {
            // Limit queue size to 2 to prevent spam queuing
            if (inputQueue.Count < 2) 
            {
                inputQueue.Enqueue(dir);
            }
        }
    }

    /// <summary>Manhattan distance from snake head to a given grid position.</summary>
    public int DistanceTo(Vector2Int pos)
        => Mathf.Abs(segments[0].x - pos.x) + Mathf.Abs(segments[0].y - pos.y);

    public int GetExtraAppleCount() => extraApples.Count;

    public List<Vector2Int> GetOccupiedCells() => segments;

    public Vector3 GridToWorld(Vector2Int gp)
    {
        float ox = -(gridWidth  * cellSize) * 0.5f + cellSize * 0.5f;
        float oy = -(gridHeight * cellSize) * 0.5f + cellSize * 0.5f;
        return new Vector3(ox + gp.x * cellSize, oy + gp.y * cellSize, 0f);
    }

    // ─── Helpers ───
    private IEnumerator UnlockAfterDelay(float t)
    {
        yield return new WaitForSeconds(t);
        inputLocked = false;
    }

    private static Sprite MakeWhiteSprite()
    {
        var tex = new Texture2D(4, 4);
        Color[] px = new Color[16];
        for (int i = 0; i < 16; i++) px[i] = Color.white;
        tex.SetPixels(px); tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4f);
    }
}
