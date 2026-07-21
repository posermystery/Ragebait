using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TrickCircleLevel : MonoBehaviour
{
    public Button correctButton;
    public Button[] decoyButtons;

    private string[] roastMessages = {
        "That's literally massive. Are you blind?",
        "Bro skipped preschool shapes class.",
        "You have the observation skills of a brick.",
        "It said SMALLEST. Not biggest. Read.",
        "I'm starting to think you don't know what a circle is."
    };

    void Start()
    {
        SetButtonsInteractable(false);
        StartCoroutine(EnableButtonsAfterDelay(0.4f));
        if (correctButton != null)
        {
            correctButton.onClick.AddListener(OnCorrectClicked);
        }

        foreach (var btn in decoyButtons)
        {
            if (btn != null)
            {
                btn.onClick.AddListener(OnWrongClicked);
            }
        }
    }

    private void SetButtonsInteractable(bool state)
    {
        if (correctButton != null) correctButton.interactable = state;
        foreach (var btn in decoyButtons)
        {
            if (btn != null) btn.interactable = state;
        }
    }

    private IEnumerator EnableButtonsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetButtonsInteractable(true);
    }

    void OnWrongClicked()
    {
        if (GameManager.Instance != null)
        {
            string msg = roastMessages[Random.Range(0, roastMessages.Length)];
            GameManager.Instance.GameOver(msg);
        }
    }

    void OnCorrectClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinLevel();
        }
    }
}
