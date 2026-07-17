using UnityEngine;

public class HiddenWinTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Win the level because they figured out they just needed to walk backward!
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinLevel();
            }
        }
    }
}
