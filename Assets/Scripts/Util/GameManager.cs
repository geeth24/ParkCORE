using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float gameOverDelay = 2f;
    [SerializeField] private string gameOverSceneName = "GameOver";
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
        }
        else
        {
            Destroy(gameObject);
        }
        
        currentLives = maxLives;
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
        
        yield return new WaitForSeconds(gameOverDelay);
        
        // Option 1: Load a Game Over scene
        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        // Option 2: Just restart the current scene
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    
    public void ResetLives()
    {
        currentLives = maxLives;
    }
} 