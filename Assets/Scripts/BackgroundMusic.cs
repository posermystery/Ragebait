using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;
    private AudioSource audioSource;

    [Header("Put the song for THIS scene here:")]
    public AudioClip sceneMusic; // Har scene ke BGM Manager me us scene ka gaana daalna

    void Awake()
    {
        if (instance == null)
        {
            // Pehli baar BGM Manager load ho raha hai
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            audioSource = GetComponent<AudioSource>();
            
            if (sceneMusic != null)
            {
                audioSource.clip = sceneMusic;
                audioSource.Play();
            }
        }
        else
        {
            // Koi purana BGM Manager already chal raha hai (from previous scene)
            if (instance.audioSource == null) instance.audioSource = instance.GetComponent<AudioSource>();

            // Kya is naye scene ka gaana purane wale se ALAG hai?
            if (this.sceneMusic != null && this.sceneMusic != instance.audioSource.clip)
            {
                // Agar alag hai (jaise Menu se Level 1 gaye), toh naya gaana change karke play karo!
                instance.audioSource.clip = this.sceneMusic;
                instance.audioSource.Play();
            }
            // Agar gaana SAME hai (jaise player mar ke wapas usi level me zinda hua), toh kuch mat karo, gaana purana wala hi bina ruke chalta rahega!

            // Is naye duplicate object ko chupchap delete kar do
            AudioSource duplicateAudio = GetComponent<AudioSource>();
            if (duplicateAudio != null) duplicateAudio.mute = true;
            Destroy(gameObject);
        }
    }
}
