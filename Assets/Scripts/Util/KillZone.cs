using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private Transform playerRespawnPoint;
    [SerializeField] private float resetDelay = 0.5f;
    
    private void Start()
    {
        // Verify respawn point is set
        if (playerRespawnPoint == null)
        {
            Debug.LogError("KillZone: Player Respawn Point is not set!");
        }
        
        // Make sure the collider is set to trigger
        var collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
        {
            Debug.LogWarning("KillZone: Collider is not set as trigger! Setting it now.");
            collider.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"KillZone: Something entered trigger - Tag: {other.tag}");
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("KillZone: Player detected, starting reset");
            StartCoroutine(ResetPlayer(other.gameObject));
        }
    }

    private System.Collections.IEnumerator ResetPlayer(GameObject player)
    {
        Debug.Log("KillZone: Starting player reset sequence");
        
        // Disable player control temporarily
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            Debug.Log("KillZone: Disabling player control");
            playerController.SetControl(false);
        }
        else
        {
            Debug.LogError("KillZone: PlayerController component not found on player!");
        }

        // Wait for the delay
        yield return new WaitForSeconds(resetDelay);

        if (playerRespawnPoint != null)
        {
            Debug.Log($"KillZone: Resetting player to position {playerRespawnPoint.position}");
            // Reset player position
            player.transform.position = playerRespawnPoint.position;
            player.transform.rotation = playerRespawnPoint.rotation;
        }

        // Re-enable player control
        if (playerController != null)
        {
            Debug.Log("KillZone: Re-enabling player control");
            playerController.SetControl(true);
        }
    }
} 