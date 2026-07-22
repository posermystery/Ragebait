using UnityEngine;
using UnityEngine.UI;

public class TermsLevel : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject termsPanel;

    [Header("Main Buttons")]
    public Button leftButton; // Originally Decline (Red)
    public Button centerButton; // Read More (Blue)
    public Button rightButton; // Originally Accept (Green)

    [Header("Terms Elements")]
    public Toggle acceptToggle;
    public Button backButton;

    private bool termsRead = false;
    private Text leftButtonText;
    private Text rightButtonText;

    void Start()
    {
        // Initial setup
        leftButtonText = leftButton.GetComponentInChildren<Text>();
        rightButtonText = rightButton.GetComponentInChildren<Text>();

        // Wire up initial buttons
        leftButton.onClick.AddListener(OnClickLeft);
        centerButton.onClick.AddListener(OnClickCenter);
        rightButton.onClick.AddListener(OnClickRight);

        // Wire up Terms panel
        acceptToggle.onValueChanged.AddListener(OnToggleChanged);
        backButton.onClick.AddListener(OnClickBack);

        // Back button only active if toggle is checked
        backButton.interactable = false;

        mainPanel.SetActive(true);
        termsPanel.SetActive(false);
    }

    private void OnClickLeft()
    {
        if (!termsRead)
        {
            // Original Decline
            GameManager.Instance.GameOver("You declined without reading? What did you expect?");
        }
        else
        {
            // Now it says Accept and is the WIN condition
            GameManager.Instance.WinLevel();
        }
    }

    private void OnClickRight()
    {
        if (!termsRead)
        {
            // Original Accept
            GameManager.Instance.GameOver("You blindly accepted? The terms said you owe me your soul.");
        }
        else
        {
            // It visually says Accept (Green), but logic is swapped so it acts as Decline (LOSE)
            GameManager.Instance.GameOver("You ticked the box saying you read the terms. If you actually did, you wouldn't have clicked that button.");
        }
    }

    private void OnClickCenter()
    {
        if (termsRead) return; // Can't click again after reading
        
        mainPanel.SetActive(false);
        termsPanel.SetActive(true);
    }

    private void OnToggleChanged(bool value)
    {
        backButton.interactable = value;
    }

    private void OnClickBack()
    {
        if (acceptToggle.isOn)
        {
            termsRead = true;
            termsPanel.SetActive(false);
            mainPanel.SetActive(true);

            // THE TRAP: Logic is swapped, but visual text remains exactly the same!

            // Disable Read More
            centerButton.interactable = false;
        }
    }
}

