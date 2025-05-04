using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip lavaRisingSound;
    [SerializeField] private float volume = 0.7f;
    
    private AudioSource audioSource;
    private bool isPlaying = false;
    
    public static AudioManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.volume = volume;
            
            // Try to load the lava sound if not assigned
            if (lavaRisingSound == null)
            {
                lavaRisingSound = Resources.Load<AudioClip>("Lava Sound Effects 3");
                if (lavaRisingSound == null)
                {
                    Debug.LogWarning("AudioManager: Failed to auto-load lava sound effect");
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PlayLavaRisingSound()
    {
        if (!isPlaying && lavaRisingSound != null)
        {
            audioSource.clip = lavaRisingSound;
            audioSource.Play();
            isPlaying = true;
            Debug.Log("AudioManager: Started playing lava rising sound");
        }
    }
    
    public void StopLavaRisingSound()
    {
        if (isPlaying)
        {
            audioSource.Stop();
            isPlaying = false;
            Debug.Log("AudioManager: Stopped playing lava rising sound");
        }
    }
    
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
} 