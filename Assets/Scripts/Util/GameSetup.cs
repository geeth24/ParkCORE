using UnityEngine;

public class GameSetup : MonoBehaviour
{
    void Awake()
    {
        // Create GameManager if not already present
        if (GameManager.Instance == null)
        {
            Debug.Log("GameSetup: Creating GameManager");
            GameObject gameManagerObj = new GameObject("GameManager");
            gameManagerObj.AddComponent<GameManager>();
        }
        
        // Create AudioManager if not already present
        if (AudioManager.Instance == null)
        {
            Debug.Log("GameSetup: Creating AudioManager");
            GameObject audioManagerObj = new GameObject("AudioManager");
            AudioManager audioManager = audioManagerObj.AddComponent<AudioManager>();
            
            // Try to set the audio clip
            AudioClip lavaSound = Resources.Load<AudioClip>("Lava Sound Effects 3");
            if (lavaSound == null)
            {
                Debug.LogWarning("GameSetup: Could not find lava sound in Resources folder");
            }
            else
            {
                audioManager.lavaRisingSound = lavaSound;
                Debug.Log("GameSetup: Successfully assigned lava sound to AudioManager");
            }
        }
    }
} 