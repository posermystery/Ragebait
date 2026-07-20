using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SnakeLevelUI : MonoBehaviour
{
    public Text trollMessageText;
    public float messageDuration = 2.0f;

    void Start()
    {
        if (trollMessageText != null)
        {
            trollMessageText.gameObject.SetActive(false);
        }
    }

    public void ShowTrollMessage(string msg)
    {
        if (trollMessageText == null) return;

        StopAllCoroutines();
        StartCoroutine(DisplayMessageRoutine(msg));
    }

    private IEnumerator DisplayMessageRoutine(string msg)
    {
        trollMessageText.text = msg;
        trollMessageText.gameObject.SetActive(true);

        // Simple pop animation
        Vector3 origScale = Vector3.one;
        Vector3 popScale = Vector3.one * 1.5f;

        float animTime = 0.2f;
        float elapsed = 0f;

        while (elapsed < animTime)
        {
            elapsed += Time.deltaTime;
            trollMessageText.transform.localScale = Vector3.Lerp(origScale, popScale, elapsed / animTime);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < animTime)
        {
            elapsed += Time.deltaTime;
            trollMessageText.transform.localScale = Vector3.Lerp(popScale, origScale, elapsed / animTime);
            yield return null;
        }

        trollMessageText.transform.localScale = origScale;

        yield return new WaitForSeconds(messageDuration);

        trollMessageText.gameObject.SetActive(false);
    }
}
