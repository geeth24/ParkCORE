using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float gameOverDelay = 2f;
    [SerializeField] private string gameOverSceneName = "GameOverScene";
    [SerializeField] private string livesUISceneName = "LivesScene";
    
    private int currentLives;
    
    public static GameManager Instance { get; private set; }
    
    public int CurrentLives => currentLives;
    public int MaxLives => maxLives;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager: Instance initialized and set to DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("GameManager: Duplicate instance found, destroying this one");
            Destroy(gameObject);
            return;
        }
        
        currentLives = maxLives;
        
        // Register to scene loaded event to ensure UI is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        // Unregister from event when destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Skip if this is the GameOverScene
        if (scene.name == gameOverSceneName)
            return;
            
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
            StartCoroutine(GameOver());
        }
    }
    
    private System.Collections.IEnumerator GameOver()
    {
        Debug.Log("Game Over!");
        
        // Load the Game Over scene immediately
        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            Debug.Log($"GameManager: Loading Game Over scene: {gameOverSceneName}");
            SceneManager.LoadScene(gameOverSceneName);
            yield break; // Exit the coroutine after loading the scene
        }
        
        // If gameOverSceneName is not set, use the delay and restart current scene
        yield return new WaitForSeconds(gameOverDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void ResetLives()
    {
        currentLives = maxLives;
    }
} 