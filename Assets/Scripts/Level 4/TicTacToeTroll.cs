using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TicTacToeTroll : MonoBehaviour
{
    [Header("UI Board")]
    public Button[] buttons = new Button[9];
    
    [Header("Board Size (Adjust in Inspector!)")]
    [Tooltip("Change this number to resize the entire Tic-Tac-Toe grid.")]
    [Range(400, 1200)]
    public float boardSize = 900f;

    [Header("Win Line Settings")]
    [Tooltip("Color of the line drawn through winning cells")]
    public Color winLineColor = Color.yellow;
    [Tooltip("Seconds to show the win line before Game Over screen")]
    public float winLineDelay = 1.5f;

    [Header("Audio")]
    public AudioClip clickSound;
    public AudioClip trollSound; // Sound when 25% chance triggers
    private AudioSource audioSource;

    private int[] board = new int[9]; // 0 = empty, 1 = Player (X), 2 = Computer (O)
    private bool isPlayerTurn = true;
    private bool gameOver = false;
    private bool inputLocked = true; // Block clicks for a moment after scene load

    // Win line indices for all 8 possible wins
    private static readonly int[,] winLines = new int[,] {
        {0, 1, 2}, {3, 4, 5}, {6, 7, 8}, // Rows
        {0, 3, 6}, {1, 4, 7}, {2, 5, 8}, // Cols
        {0, 4, 8}, {2, 4, 6}             // Diagonals
    };

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Apply board size from Inspector
        ApplyBoardSize();

        // Setup buttons
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Closure
            buttons[i].onClick.AddListener(() => OnButtonClicked(index));
            buttons[i].GetComponentInChildren<Text>().text = "";
        }

        // Unlock input after a short delay so the restart tap doesn't place a symbol
        StartCoroutine(UnlockInputAfterDelay());
    }

    private void ApplyBoardSize()
    {
        if (buttons.Length > 0 && buttons[0] != null)
        {
            Transform boardTransform = buttons[0].transform.parent;
            if (boardTransform != null)
            {
                // Resize the board panel
                RectTransform boardRect = boardTransform.GetComponent<RectTransform>();
                if (boardRect != null)
                {
                    boardRect.sizeDelta = new Vector2(boardSize, boardSize);
                }

                // Resize cells in the grid layout
                GridLayoutGroup grid = boardTransform.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    float cellSize = (boardSize - grid.spacing.x * 2f - grid.padding.left - grid.padding.right) / 3f;
                    grid.cellSize = new Vector2(cellSize, cellSize);
                }
            }
        }
    }

    private IEnumerator UnlockInputAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);
        inputLocked = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void OnButtonClicked(int index)
    {
        if (gameOver || !isPlayerTurn || inputLocked || board[index] != 0) return;

        // TROLL MECHANIC: 25% chance to place opponent's symbol
        bool trolled = Random.Range(0, 100) < 25;

        if (trolled)
        {
            // Place Computer's symbol instead!
            PlaySound(trollSound);
            PlaceSymbol(index, 2); // 2 is O
        }
        else
        {
            // Normal placement
            PlaySound(clickSound);
            PlaceSymbol(index, 1); // 1 is X
        }

        if (CheckWinAndDraw()) return;

        // Computer's turn (Even if the player placed an O, computer still goes!)
        StartCoroutine(ComputerTurnDelay());
    }

    private void PlaceSymbol(int index, int playerID)
    {
        board[index] = playerID;
        Text btnText = buttons[index].GetComponentInChildren<Text>();
        
        if (playerID == 1)
        {
            btnText.text = "X";
            btnText.color = Color.blue;
        }
        else
        {
            btnText.text = "O";
            btnText.color = Color.red;
        }
    }

    private IEnumerator ComputerTurnDelay()
    {
        isPlayerTurn = false;
        yield return new WaitForSeconds(0.6f); // Fake thinking time
        
        if (!gameOver)
        {
            ComputerPlaySmart();
            CheckWinAndDraw();
        }
        
        isPlayerTurn = true;
    }

    private void ComputerPlaySmart()
    {
        // 1. Check if Computer can win
        int winMove = FindWinningMove(2);
        if (winMove != -1)
        {
            PlaySound(clickSound);
            PlaceSymbol(winMove, 2);
            return;
        }

        // 2. Check if Player is about to win, and BLOCK it
        int blockMove = FindWinningMove(1);
        if (blockMove != -1)
        {
            PlaySound(clickSound);
            PlaceSymbol(blockMove, 2);
            return;
        }

        // 3. Take center if available
        if (board[4] == 0)
        {
            PlaySound(clickSound);
            PlaceSymbol(4, 2);
            return;
        }

        // 4. Otherwise, pick a random empty spot
        List<int> emptySpots = new List<int>();
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == 0) emptySpots.Add(i);
        }

        if (emptySpots.Count > 0)
        {
            int randomIndex = emptySpots[Random.Range(0, emptySpots.Count)];
            PlaySound(clickSound);
            PlaceSymbol(randomIndex, 2);
        }
    }

    private int FindWinningMove(int playerID)
    {
        for (int i = 0; i < 8; i++)
        {
            int a = winLines[i, 0], b = winLines[i, 1], c = winLines[i, 2];

            // Check if two spots belong to playerID and one is empty
            if (board[a] == playerID && board[b] == playerID && board[c] == 0) return c;
            if (board[a] == playerID && board[c] == playerID && board[b] == 0) return b;
            if (board[b] == playerID && board[c] == playerID && board[a] == 0) return a;
        }
        return -1; // No winning move found
    }

    private bool CheckWinAndDraw()
    {
        int[] winningLine;
        int winner = GetWinner(out winningLine);

        if (winner == 1)
        {
            // Player won — show line, then trigger win
            gameOver = true;
            StartCoroutine(ShowWinLineThenEnd(winningLine, true, ""));
            return true;
        }
        else if (winner == 2)
        {
            // Computer won — show line, then trigger game over
            gameOver = true;
            StartCoroutine(ShowWinLineThenEnd(winningLine, false, "You lost a rigged game of Tic-Tac-Toe. Obviously."));
            return true;
        }
        else if (IsBoardFull())
        {
            // Draw = Death (no line to show)
            gameOver = true;
            StartCoroutine(ShowWinLineThenEnd(null, false, "A draw? Real gamers don't tie. You die."));
            return true;
        }

        return false;
    }

    private IEnumerator ShowWinLineThenEnd(int[] winningCells, bool isWin, string loseMessage)
    {
        // Highlight winning cells if there are any
        if (winningCells != null)
        {
            foreach (int cellIndex in winningCells)
            {
                // Make the winning cells glow with the win line color
                Image btnImage = buttons[cellIndex].GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = winLineColor;
                }

                // Also make the text bigger/bolder for emphasis
                Text btnText = buttons[cellIndex].GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.fontSize = (int)(btnText.fontSize * 1.3f);
                    btnText.fontStyle = FontStyle.Bold;
                }
            }
        }

        // Wait so the player can see what happened
        yield return new WaitForSeconds(winLineDelay);

        // Now hide the board and show game result
        HideBoard();

        if (isWin)
        {
            if (GameManager.Instance != null) GameManager.Instance.WinLevel();
        }
        else
        {
            if (GameManager.Instance != null) GameManager.Instance.GameOver(loseMessage);
        }
    }

    private void HideBoard()
    {
        if (buttons.Length > 0 && buttons[0] != null)
        {
            // The parent of the buttons is the Tic-Tac-Toe Board panel. Hiding it cleans the screen.
            buttons[0].transform.parent.gameObject.SetActive(false);
        }
    }

    private int GetWinner(out int[] winningLine)
    {
        winningLine = null;

        for (int i = 0; i < 8; i++)
        {
            int a = winLines[i, 0], b = winLines[i, 1], c = winLines[i, 2];
            
            if (board[a] != 0 && board[a] == board[b] && board[a] == board[c])
            {
                winningLine = new int[] { a, b, c };
                return board[a]; // Returns 1 (Player) or 2 (Computer)
            }
        }
        return 0; // No winner yet
    }

    private bool IsBoardFull()
    {
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == 0) return false;
        }
        return true;
    }
}
