using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Death screen shown when player is caught.
/// Displays run stats and allows proceeding to shop or restarting.
/// </summary>
public class DeathScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Text essenceEarnedText;
    [SerializeField] private Text totalEssenceText;
    [SerializeField] private Text survivalTimeText;
    [SerializeField] private Text runCountText;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Audio")]
    [SerializeField] private AudioClip showSound;

    private GameStateManager gameState;
    private AudioSource audioSource;

    private void Awake()
    {
        gameState = GameStateManager.Instance;
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Initially hidden
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        // Button listeners
        if (shopButton != null)
        {
            shopButton.onClick.AddListener(OpenShop);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartRun);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    private void Start()
    {
        if (gameState != null)
        {
            gameState.OnPlayerDeath += ShowDeathScreen;
        }
    }

    private void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.OnPlayerDeath -= ShowDeathScreen;
        }
    }

    public void ShowDeathScreen()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }

        // Update stats display
        if (gameState != null)
        {
            if (essenceEarnedText != null)
            {
                essenceEarnedText.text = $"Fear Essence Earned: +{gameState.CurrentRunEssence}";
            }

            if (totalEssenceText != null)
            {
                totalEssenceText.text = $"Total Fear Essence: {gameState.TotalFearEssence}";
            }

            if (runCountText != null)
            {
                runCountText.text = $"Run #{gameState.TotalRuns}";
            }
        }

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Play sound
        if (showSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(showSound);
        }
    }

    public void HideDeathScreen()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
    }

    private void OpenShop()
    {
        HideDeathScreen();
        
        if (gameState != null)
        {
            gameState.OpenShop();
        }
    }

    private void RestartRun()
    {
        HideDeathScreen();
        Time.timeScale = 1f;
        
        if (gameState != null)
        {
            gameState.RestartRun();
        }
    }

    private void ReturnToMainMenu()
    {
        HideDeathScreen();
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
