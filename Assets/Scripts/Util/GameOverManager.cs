using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameOverMessageText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gamePlaySceneName = "Level1";

    private void Awake()
    {
        // Ensure cursor is visible and not locked in game over screen
        ShowCursor();
    }

    private void Start()
    {
        // Set up message from the GameManager
        SetupGameOverMessage();
        
        // Set up button listeners
        SetupButtons();
    }

    private void OnEnable()
    {
        // Find the restart button if not assigned
        if (restartButton == null)
        {
            GameObject restartObj = GameObject.Find("RestartButton");
            if (restartObj != null)
            {
                restartButton = restartObj.GetComponent<Button>();
                if (restartButton != null)
                {
                    restartButton.onClick.AddListener(OnRestartClicked);
                    Debug.Log("GameOverManager: Found and connected RestartButton");
                }
            }
        }
    }

    private void ShowCursor()
    {
        Debug.Log("GameOverManager: Ensuring cursor is visible");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void SetupGameOverMessage()
    {
        if (gameOverMessageText == null)
        {
            // Try to find GameOverText
            GameObject textObj = GameObject.Find("GameOverText");
            if (textObj != null)
            {
                gameOverMessageText = textObj.GetComponent<TextMeshProUGUI>();
            }
        }
        
        if (gameOverMessageText != null)
        {
            string message = "Game Over!"; // Default message
            
            // Get custom message if available
            if (GameManager.Instance != null)
            {
                message = GameManager.CustomGameOverMessage;
            }
            
            gameOverMessageText.text = message;
        }
    }

    private void SetupButtons()
    {
        // Set up restart button
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners(); // Clear any existing listeners
            restartButton.onClick.AddListener(OnRestartClicked);
            Debug.Log("GameOverManager: Restart button listeners set up");
        }
        else
        {
            Debug.LogWarning("GameOverManager: Restart button not assigned. Looking for it by name...");
            GameObject restartObj = GameObject.Find("RestartButton");
            if (restartObj != null)
            {
                restartButton = restartObj.GetComponent<Button>();
                if (restartButton != null)
                {
                    restartButton.onClick.AddListener(OnRestartClicked);
                    Debug.Log("GameOverManager: Found and connected RestartButton");
                }
            }
        }
        
        // Set up main menu button
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    private void OnRestartClicked()
    {
        Debug.Log("GameOverManager: Restart button clicked");
        
        // Reset lives in GameManager if it exists
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetLives();
            Debug.Log("GameOverManager: Reset lives in GameManager");
        }
        
        // Load the gameplay scene
        Debug.Log($"GameOverManager: Loading gameplay scene: {gamePlaySceneName}");
        SceneManager.LoadScene(gamePlaySceneName);
    }

    private void OnMainMenuClicked()
    {
        Debug.Log("GameOverManager: Main Menu button clicked");
        
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
} 