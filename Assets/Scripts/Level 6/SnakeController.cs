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
    private Vector2Int nextDir             = Vector2Int.right;
    private float      moveTimer           = 0f;
    private bool       isDead              = false;
    private bool       inputLocked         = true;

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
            if ((kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
                && direction != Vector2Int.down)  nextDir = Vector2Int.up;

            if ((kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)
                && direction != Vector2Int.up)    nextDir = Vector2Int.down;

            if ((kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)
                && direction != Vector2Int.right) nextDir = Vector2Int.left;

            if ((kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)
                && direction != Vector2Int.left)  nextDir = Vector2Int.right;
        }

        // Touch swipe (Android)
        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var phase = touches[i].phase.ReadValue();
                if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    swipeStart = touches[i].position.ReadValue();
                    swiping    = true;
                }
                else if (swiping && phase == UnityEngine.InputSystem.TouchPhase.Ended)
                {
                    swiping = false;
                    Vector2 delta = touches[i].position.ReadValue() - swipeStart;
                    if (delta.magnitude > 40f)
                    {
                        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                        {
                            if (delta.x > 0 && direction != Vector2Int.left)  nextDir = Vector2Int.right;
                            if (delta.x < 0 && direction != Vector2Int.right) nextDir = Vector2Int.left;
                        }
                        else
                        {
                            if (delta.y > 0 && direction != Vector2Int.down)  nextDir = Vector2Int.up;
                            if (delta.y < 0 && direction != Vector2Int.up)    nextDir = Vector2Int.down;
                        }
                    }
                }
            }
        }
    }

    // ─── One grid step ───
    private void Step()
    {
        direction = nextDir;
        Vector2Int newHead = segments[0] + direction;

        // Wall collision → wrap (Nokia style) or die — here we wrap
        newHead.x = (newHead.x + gridWidth)  % gridWidth;
        newHead.y = (newHead.y + gridHeight) % gridHeight;

        // Self collision
        for (int i = 0; i < segments.Count - 1; i++)
        {
            if (newHead == segments[i])
            {
                Die();
                return;
            }
        }

        bool ateApple = (newHead == apple.GetGridPos());

        segments.Insert(0, newHead);

        if (!ateApple)
            segments.RemoveAt(segments.Count - 1);
        else
            apple.OnEaten(this);

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
    private void Die()
    {
        isDead = true;
        string[] msgs = {
            "You bit yourself. Classic.",
            "Self-destruction speedrun any%",
            "Snake ate snake. The ouroboros wins.",
        };
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver(msgs[Random.Range(0, msgs.Length)]);
    }

    // ─── Public helpers ───

    public void MoveUp() { if (direction != Vector2Int.down) nextDir = Vector2Int.up; }
    public void MoveDown() { if (direction != Vector2Int.up) nextDir = Vector2Int.down; }
    public void MoveLeft() { if (direction != Vector2Int.right) nextDir = Vector2Int.left; }
    public void MoveRight() { if (direction != Vector2Int.left) nextDir = Vector2Int.right; }

    /// <summary>Manhattan distance from snake head to a given grid position.</summary>
    public int DistanceTo(Vector2Int pos)
        => Mathf.Abs(segments[0].x - pos.x) + Mathf.Abs(segments[0].y - pos.y);

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
