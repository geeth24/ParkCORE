using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LivesUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private Image[] livesImages;
    [SerializeField] private Sprite activeLifeSprite;
    [SerializeField] private Sprite inactiveLifeSprite;
    [SerializeField] private bool useIconsMode = true;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;
        
        if (gameManager == null)
        {
            Debug.LogError("LivesUI: GameManager instance not found!");
            return;
        }
        
        Debug.Log("LivesUI: GameManager found. Initializing display.");
        UpdateLivesDisplay();
    }

    private void Update()
    {
        if (gameManager != null)
        {
            UpdateLivesDisplay();
        }
        else if (gameManager == null)
        {
             // Attempt to find GameManager again if it wasn't found initially
             gameManager = GameManager.Instance;
             if (gameManager == null)
             {
                Debug.LogError("LivesUI: GameManager instance still not found in Update!");
                return; // Stop trying if still not found
             } else {
                Debug.Log("LivesUI: GameManager found in Update. Initializing display.");
             }
        }
    }

    private void UpdateLivesDisplay()
    {
        if (useIconsMode)
        {
            if (livesImages != null && livesImages.Length > 0)
            {
                 // Debug.Log("LivesUI: Updating Icons Mode"); // Optional: uncomment if debugging icon mode
                for (int i = 0; i < livesImages.Length; i++)
                {
                    if (livesImages[i] != null)
                    {
                        livesImages[i].sprite = (i < gameManager.CurrentLives) ? activeLifeSprite : inactiveLifeSprite;
                        livesImages[i].enabled = (i < gameManager.MaxLives);
                    }
                }
            }
            else
            {
                Debug.LogWarning("LivesUI: In Icons Mode, but livesImages array is null or empty.");
            }
        }
        else 
        {
            if (livesText != null)
            {
                string newText = $"Lives: {gameManager.CurrentLives}";
                // Only update if the text has actually changed to avoid unnecessary updates
                if (livesText.text != newText)
                {
                    Debug.Log($"LivesUI: Updating Text Mode - Setting text to: {newText}");
                    livesText.text = newText;
                }
            }
            else
            {
                Debug.LogError("LivesUI: In Text Mode, but livesText reference is not assigned in the Inspector!");
            }
        }
    }
} 