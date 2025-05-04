using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartGameButton : MonoBehaviour
{
    [SerializeField] private string lavaLevelSceneName = "Level 1 (Lava)";
    private Button startButton;

    private void Awake()
    {
        Debug.Log("StartGameButton: Awake() called");
    }

    private void Start()
    {
        Debug.Log("StartGameButton: Start() called");
        
        // Try to get the button component directly
        startButton = GetComponent<Button>();
        if (startButton != null)
        {
            // Clear any existing listeners and add our own
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
            Debug.Log("StartGameButton: Button listener added manually in Start()");
            Debug.Log($"StartGameButton: Target scene is '{lavaLevelSceneName}'");
            
            // Verify EventSystem exists
            if (EventSystem.current == null)
            {
                Debug.LogError("StartGameButton: No EventSystem found in the scene!");
            }
            else
            {
                Debug.Log("StartGameButton: EventSystem found");
            }
        }
        else
        {
            Debug.LogError("StartGameButton: No Button component found on this GameObject!");
        }
    }

    public void StartGame()
    {
        Debug.Log($"StartGameButton: StartGame() called, loading scene: {lavaLevelSceneName}");
        SceneManager.LoadScene(lavaLevelSceneName);
    }

    // This is a public method that can be called directly from the Button's onClick in the Inspector
    public void LoadLavaLevel()
    {
        Debug.Log("StartGameButton: LoadLavaLevel() called directly from Inspector");
        SceneManager.LoadScene(lavaLevelSceneName);
    }

    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
        }
    }
} 