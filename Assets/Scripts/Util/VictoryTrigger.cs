using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class VictoryTrigger : MonoBehaviour
{
    [SerializeField] private string victoryMessage = "Victory, You beat the lava";
    [SerializeField] private string gameOverSceneName = "GameOverScene";

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is the player
        if (other.CompareTag("Player"))
        {
            Debug.Log("VictoryTrigger: Player entered victory zone.");

            // Set the custom message in the GameManager
            if (GameManager.Instance != null)
            {
                GameManager.SetGameOverMessage(victoryMessage);
            }
            else
            {
                Debug.LogError("VictoryTrigger: GameManager instance not found! Cannot set victory message.");
                // Optionally handle this case, maybe load scene anyway with default message?
            }

            // Stop lava rising and music
            StopLavaAndMusic();

            // Show cursor
            ShowCursor();

            // Load the Game Over scene
            if (!string.IsNullOrEmpty(gameOverSceneName))
            {
                Debug.Log($"VictoryTrigger: Loading scene: {gameOverSceneName}");
                SceneManager.LoadScene(gameOverSceneName);
            }
            else
            {
                Debug.LogError("VictoryTrigger: Game Over Scene Name is not set in the inspector!");
            }
        }
    }

    private void ShowCursor()
    {
        Debug.Log("VictoryTrigger: Showing cursor for victory screen");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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

    private void Reset()
    {
        // Ensure the trigger collider is set to be a trigger by default
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
             Debug.LogWarning("VictoryTrigger: No Collider found on this GameObject. Please add a Collider component.", this);
        }
    }
} 