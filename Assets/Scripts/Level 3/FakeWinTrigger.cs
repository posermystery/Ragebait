using UnityEngine;

public class FakeWinTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlatformerPlayer player = collision.GetComponent<PlatformerPlayer>();
            if (player != null)
            {
                // The ultimate troll
                player.Die("You thought you won? Imagine falling for a fake door. Classic.");
            }
        }
    }
}
