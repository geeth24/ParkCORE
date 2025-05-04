using UnityEngine;

public class LavaKillTrigger : MonoBehaviour
{
    private Transform playerRespawnPoint;
    private string playerTag = "Player";
    private LayerMask playerLayerMask;
    private float resetDelay = 0.5f;
    private GameManager gameManager;
    
    public void Initialize(Transform respawnPoint, string tag, LayerMask layerMask)
    {
        playerRespawnPoint = respawnPoint;
        playerTag = tag;
        playerLayerMask = layerMask;
        resetDelay = 0.5f;
    }
    
    private void Start()
    {
        // Verify respawn point is set
        if (playerRespawnPoint == null)
        {
            Debug.LogError("LavaKillTrigger: Player Respawn Point is not set!");
            
            // Try to find the respawn point in the scene
            GameObject respawnPointObj = GameObject.Find("PlayerRespawnPoint");
            if (respawnPointObj != null)
            {
                playerRespawnPoint = respawnPointObj.transform;
                Debug.Log("LavaKillTrigger: Found PlayerRespawnPoint in scene.");
            }
        }
        
        // Find game manager
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogWarning("LavaKillTrigger: GameManager not found! Lives system will not work.");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider is on the player layer or has the player tag
        bool isPlayer = other.CompareTag(playerTag);
        bool isPlayerLayer = (playerLayerMask.value & (1 << other.gameObject.layer)) != 0;
        
        if (isPlayer || isPlayerLayer)
        {
            Debug.Log("LavaKillTrigger: Player detected, starting reset");
            
            // Deduct a life
            if (gameManager != null)
            {
                gameManager.LoseLife();
            }
            
            StartCoroutine(ResetPlayer(other.gameObject));
        }
    }

    private System.Collections.IEnumerator ResetPlayer(GameObject player)
    {
        Debug.Log("LavaKillTrigger: Starting player reset sequence");
        
        // Disable player control temporarily
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            Debug.Log("LavaKillTrigger: Disabling player control");
            playerController.SetControl(false);
        }
        else
        {
            Debug.LogWarning("LavaKillTrigger: PlayerController component not found on player!");
        }

        // Wait for the delay
        yield return new WaitForSeconds(resetDelay);

        // Only respawn if we haven't reached game over (0 lives)
        if (gameManager == null || gameManager.CurrentLives > 0)
        {
            if (playerRespawnPoint != null)
            {
                Debug.Log($"LavaKillTrigger: Resetting player to position {playerRespawnPoint.position}");
                // Reset player position slightly above the point
                Vector3 respawnPosition = playerRespawnPoint.position + Vector3.up * 1.5f; // Added offset
                player.transform.position = respawnPosition;
                player.transform.rotation = playerRespawnPoint.rotation;
            }

            // Re-enable player control
            if (playerController != null)
            {
                Debug.Log("LavaKillTrigger: Re-enabling player control");
                playerController.SetControl(true);
            }
        }
        else
        {
            // Player has no more lives, stop lava and music
            // (GameManager will handle loading game over scene)
            StopLavaAndMusic();
        }
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
} 