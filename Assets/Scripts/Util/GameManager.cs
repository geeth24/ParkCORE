using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Add this if TextMeshPro operations are needed directly here, otherwise might belong in GameOverUI

public class GameManager : MonoBehaviour
{
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float gameOverDelay = 2f;
    [SerializeField] private string gameOverSceneName = "GameOverScene";
    [SerializeField] private string livesUISceneName = "LivesScene";
    // Optional: Reference to a TextMeshProUGUI in the main game scene if needed, unlikely for game over message
    // [SerializeField] private TextMeshProUGUI gameOverTextDisplay; 

    private int currentLives;
    
    public static GameManager Instance { get; private set; }
    public static string CustomGameOverMessage { get; private set; } = "Game Over!"; // Default message
    
    public int CurrentLives => currentLives;
    public int MaxLives => maxLives;
    
    private void Awake()
    {
        Debug.Log($"GameManager: Awake() called on GameObject '{gameObject.name}' in scene '{SceneManager.GetActiveScene().name}'");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager: Instance assigned and marked DontDestroyOnLoad.");
            ResetGameOverMessage(); // Ensure default message on start/reload
        }
        else
        {
            Debug.Log($"GameManager: Duplicate instance found ('{gameObject.name}'), destroying this one.");
            Destroy(gameObject);
            return;
        }
        
        currentLives = maxLives;
        
        // Register to scene loaded event to ensure UI is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        Debug.Log($"GameManager: OnDestroy() called on GameObject '{gameObject.name}'. Is this the persistent instance? {(Instance == this ? "YES" : "NO - Likely a duplicate or scene unload")}");
        // Unregister from event when destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
        // If this *is* the singleton instance being destroyed, nullify the static reference
        if (Instance == this)
        {
            Instance = null;
            Debug.Log("GameManager: Static Instance reference nulled.");
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If this is the GameOverScene, show the cursor
        if (scene.name == gameOverSceneName)
        {
            ShowCursor();
            return;
        }
            
        // Check if Lives UI scene is loaded
        if (!string.IsNullOrEmpty(livesUISceneName) && !SceneManager.GetSceneByName(livesUISceneName).isLoaded)
        {
            Debug.Log($"GameManager: Loading Lives UI scene after scene change: {livesUISceneName}");
            SceneManager.LoadSceneAsync(livesUISceneName, LoadSceneMode.Additive);
        }
    }
    
    private void Start()
    {
        // Load the UI scene additively if it's not already loaded
        if (!string.IsNullOrEmpty(livesUISceneName))
        {
            if (!SceneManager.GetSceneByName(livesUISceneName).isLoaded)
            {
                Debug.Log($"GameManager: Attempting to load additive scene: {livesUISceneName}");
                SceneManager.LoadSceneAsync(livesUISceneName, LoadSceneMode.Additive);
            }
            else
            {
                Debug.Log($"GameManager: Scene {livesUISceneName} is already loaded.");
            }
        }
        else
        {
             Debug.LogWarning("GameManager: Lives UI Scene Name is not set in the inspector!");
        }
    }
    
    public void LoseLife()
    {
        currentLives--;
        Debug.Log($"Lives remaining: {currentLives}");
        
        if (currentLives <= 0)
        {
            // Ensure default message is used for losing all lives
            SetGameOverMessage("Out of Lives! Game Over!"); // Set specific message for 0 lives
            StartCoroutine(GameOver());
        }
    }
    
    private System.Collections.IEnumerator GameOver()
    {
        Debug.Log("Game Over!");
        
        // Stop lava rising and music
        StopLavaAndMusic();
        
        // Show cursor
        ShowCursor();
        
        // Load the Game Over scene immediately
        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            Debug.Log($"GameManager: Loading Game Over scene: {gameOverSceneName}");
            // Message is set *before* calling this typically
            SceneManager.LoadScene(gameOverSceneName);
            yield break; // Exit the coroutine after loading the scene
        }
        
        // If gameOverSceneName is not set, use the delay and restart current scene
        yield return new WaitForSeconds(gameOverDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void StopLavaAndMusic()
    {
        // Find all LavaRiser components in the scene and stop them
        LavaRiser[] lavaRisers = FindObjectsOfType<LavaRiser>();
        foreach (LavaRiser riser in lavaRisers)
        {
            riser.StopRising();
        }

        // Stop music via AudioManager if available
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopLavaRisingSound();
        }
    }
    
    private void ShowCursor()
    {
        Debug.Log("GameManager: Showing cursor for game over screen");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    public void ResetLives()
    {
        currentLives = maxLives;
        ResetGameOverMessage(); // Also reset message when lives are reset
    }

    public static void SetGameOverMessage(string message)
    {
        CustomGameOverMessage = message;
        Debug.Log($"GameManager: Custom game over message set: {message}");
    }

    public static void ResetGameOverMessage()
    {
        CustomGameOverMessage = "Game Over!"; // Reset to default
        Debug.Log("GameManager: Game over message reset to default.");
    }
} 