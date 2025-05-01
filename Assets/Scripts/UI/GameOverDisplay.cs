using UnityEngine;
using TMPro; // Need this for TextMeshPro
using System.Collections; // Required for Coroutines

public class GameOverDisplay : MonoBehaviour
{
    // You can assign this in the Inspector
    [SerializeField] private TextMeshProUGUI gameOverTextDisplay;

    // Alternatively, find it by name if not assigned (less robust)
    // [SerializeField] private string gameOverTextObjectName = "GameOverText";

    void Start()
    {
        // If the text display is not assigned in the inspector, try to find it by name
        if (gameOverTextDisplay == null)
        {
            GameObject textObject = GameObject.Find("GameOverText"); // Ensure this name matches your object
            if (textObject != null)
            {
                gameOverTextDisplay = textObject.GetComponent<TextMeshProUGUI>();
            }
        }

        // Check if we found the component before starting the coroutine
        if (gameOverTextDisplay != null)
        {
            StartCoroutine(UpdateGameOverText());
        }
        else
        {
            Debug.LogError("GameOverDisplay: TextMeshProUGUI component (GameOverText) not found or assigned in the inspector!");
        }
    }

    private IEnumerator UpdateGameOverText()
    {
        Debug.Log($"GameOverDisplay: Coroutine started. Checking GameManager.Instance before wait: {(GameManager.Instance == null ? "NULL" : "EXISTS")}");
        // Wait until the end of the frame to ensure GameManager instance is available
        yield return new WaitForEndOfFrame();
        Debug.Log($"GameOverDisplay: Finished WaitForEndOfFrame. Checking GameManager.Instance after wait: {(GameManager.Instance == null ? "NULL" : "EXISTS")}");

        // Now try to access the GameManager
        if (GameManager.Instance != null)
        {
            gameOverTextDisplay.text = GameManager.CustomGameOverMessage;
            Debug.Log($"GameOverDisplay: Set text to: {GameManager.CustomGameOverMessage}");

            // Optional: Reset the message after displaying it
            // GameManager.ResetGameOverMessage(); 
        }
        else
        {
            Debug.LogError("GameOverDisplay: GameManager instance still not found after waiting! Displaying default text.");
            // Display default text or keep what's in the scene
            // gameOverTextDisplay.text = "Game Over!"; 
        }
    }
} 