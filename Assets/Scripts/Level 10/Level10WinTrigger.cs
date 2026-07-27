using UnityEngine;

public class Level10WinTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Check if the button was pressed to open the dual gates!
            if (!Level10GateController.areGatesOpen)
            {
                Debug.Log("[Level10WinTrigger] Player reached win trigger, but the capsule button hasn't been pressed yet! Trigger is inactive.");
                return;
            }

            // Button was pressed, so trigger the victory!
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinLevel();
            }
        }
    }
}
