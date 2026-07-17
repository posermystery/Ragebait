using UnityEngine;

public class TriggerButton : MonoBehaviour
{
    public GameObject gateToOpen;
    private bool isPressed = false;
    
    // Optional audio for button click
    private AudioSource audioSource;
    public AudioClip clickSound;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // When player steps on button
        if (!isPressed && collision.CompareTag("Player"))
        {
            isPressed = true;
            
            // Play sound
            if (clickSound != null) audioSource.PlayOneShot(clickSound);
            
            // Visual feedback: squash the button down
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * 0.2f, transform.localScale.z);
            
            // Open the gate (e.g. destroy it or move it)
            if (gateToOpen != null)
            {
                Destroy(gateToOpen); // Or you could animate it sliding up
            }
        }
    }
}
