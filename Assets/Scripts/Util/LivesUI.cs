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
    [SerializeField] private float retryInterval = 0.5f;

    private GameManager gameManager;
    private float nextRetryTime;
    private bool showedWarning = false;

    private void Start()
    {
        FindGameManager();
        nextRetryTime = Time.time + retryInterval;
    }

    private void FindGameManager()
    {
        if (gameManager != null) return;
        
        gameManager = GameManager.Instance;
        
        if (gameManager != null)
        {
            Debug.Log("LivesUI: GameManager found. Initializing display.");
            UpdateLivesDisplay();
            showedWarning = false;
        }
    }

    private void Update()
    {
        if (gameManager != null)
        {
            UpdateLivesDisplay();
            return;
        }
        
        // Only retry finding GameManager at intervals to avoid spamming
        if (Time.time >= nextRetryTime)
        {
            FindGameManager();
            
            // Show warning message only once
            if (gameManager == null && !showedWarning)
            {
                Debug.LogWarning("LivesUI: GameManager instance not found in Update!");
                showedWarning = true;
            }
            
            nextRetryTime = Time.time + retryInterval;
        }
    }

    private void UpdateLivesDisplay()
    {
        if (gameManager == null) return;
        
        if (useIconsMode)
        {
            if (livesImages != null && livesImages.Length > 0)
            {
                for (int i = 0; i < livesImages.Length; i++)
                {
                    if (livesImages[i] != null)
                    {
                        livesImages[i].sprite = (i < gameManager.CurrentLives) ? activeLifeSprite : inactiveLifeSprite;
                        livesImages[i].enabled = (i < gameManager.MaxLives);
                    }
                }
            }
            else if (!showedWarning)
            {
                Debug.LogWarning("LivesUI: In Icons Mode, but livesImages array is null or empty.");
                showedWarning = true;
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
                    livesText.text = newText;
                }
            }
            else if (!showedWarning)
            {
                Debug.LogError("LivesUI: In Text Mode, but livesText reference is not assigned in the Inspector!");
                showedWarning = true;
            }
        }
    }
} 