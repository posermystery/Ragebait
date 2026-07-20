using UnityEngine;
using System.Collections.Generic;

public class AppleTroll : MonoBehaviour
{
    [Header("Sprites — Assign in Inspector")]
    public Sprite appleSprite;

    [Header("Audio")]
    public AudioClip eatSound;
    public AudioClip teleportSound;
    [Range(0f, 1f)] public float volume = 0.5f;

    [Header("References")]
    public SnakeController snake;
    public SnakeLevelUI levelUI;

    private Vector2Int gridPos;
    private int applesEaten = 0;
    private int teleportsDone = 0;
    private SpriteRenderer sr;
    private AudioSource audioSource;
    private Sprite whiteSprite;

    // Troll messages
    private string[] teleportMessages = {
        "Oops, you missed!",
        "Too slow! Try to catch me.",
        "Sike! One more time."
    };

    void Start()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 4;
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        whiteSprite = MakeWhiteSprite();
        sr.sprite = (appleSprite != null) ? appleSprite : whiteSprite;
        sr.color = (appleSprite == null) ? Color.red : Color.white;

        Reposition();
    }

    void Update()
    {
        if (snake == null) return;

        // Troll logic: On the 5th apple (applesEaten == 4), if the snake gets close (<= 3 grid cells), teleport it!
        if (applesEaten == 4 && teleportsDone < 3)
        {
            if (snake.DistanceTo(gridPos) <= 3)
            {
                TeleportTroll();
            }
        }
    }

    public Vector2Int GetGridPos() => gridPos;

    public void OnEaten(SnakeController snakeRef)
    {
        applesEaten++;

        if (eatSound != null)
            audioSource.PlayOneShot(eatSound, volume);

        if (applesEaten >= 5)
        {
            // Win condition!
            gameObject.SetActive(false);
            if (GameManager.Instance != null)
                GameManager.Instance.WinLevel();
            return;
        }

        Reposition();
    }

    private void TeleportTroll()
    {
        if (teleportSound != null)
            audioSource.PlayOneShot(teleportSound, volume);
            
        if (levelUI != null && teleportsDone < teleportMessages.Length)
        {
            levelUI.ShowTrollMessage(teleportMessages[teleportsDone]);
        }

        teleportsDone++;
        Reposition();
    }

    private void Reposition()
    {
        if (snake == null) return;

        List<Vector2Int> occupied = snake.GetOccupiedCells();
        int maxAttempts = 100;
        
        while (maxAttempts > 0)
        {
            maxAttempts--;
            int rx = Random.Range(0, snake.gridWidth);
            int ry = Random.Range(0, snake.gridHeight);
            Vector2Int potentialPos = new Vector2Int(rx, ry);

            if (!occupied.Contains(potentialPos))
            {
                // To make the troll effective, don't spawn the troll apple right next to the snake head after teleporting
                if (applesEaten == 4 && teleportsDone > 0 && teleportsDone <= 3)
                {
                    if (snake.DistanceTo(potentialPos) <= 5)
                        continue; // try again
                }

                gridPos = potentialPos;
                transform.position = snake.GridToWorld(gridPos);
                
                // Adjust scale based on cell size
                float scale = snake.cellSize * 0.8f;
                transform.localScale = Vector3.one * scale;
                return;
            }
        }
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
