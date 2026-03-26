using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Win screen shown when player successfully escapes.
/// Displays run stats and celebrates the achievement.
/// </summary>
public class WinScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject trueEndingPanel;
    [SerializeField] private Text essenceEarnedText;
    [SerializeField] private Text totalEssenceText;
    [SerializeField] private Text survivalTimeText;
    [SerializeField] private Text notesCollectedText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Audio")]
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip trueEndingSound;

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
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (trueEndingPanel != null)
        {
            trueEndingPanel.SetActive(false);
        }

        // Button listeners
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueToShop);
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
            gameState.OnPlayerEscape += ShowWinScreen;
        }
    }

    private void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.OnPlayerEscape -= ShowWinScreen;
        }
    }

    public void ShowWinScreen()
    {
        bool isTrueEnding = gameState != null && gameState.TotalNotesCollected >= 10;

        if (isTrueEnding && trueEndingPanel != null)
        {
            trueEndingPanel.SetActive(true);
        }
        else if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        // Update stats display
        if (gameState != null)
        {
            if (essenceEarnedText != null)
            {
                int bonus = 50; // Escape bonus
                essenceEarnedText.text = $"Fear Essence Earned: +{gameState.CurrentRunEssence + bonus}";
            }

            if (totalEssenceText != null)
            {
                totalEssenceText.text = $"Total Fear Essence: {gameState.TotalFearEssence}";
            }

            if (survivalTimeText != null)
            {
                // This would need to be calculated from run start time
                survivalTimeText.text = $"Escape Time: ?";
            }

            if (notesCollectedText != null)
            {
                notesCollectedText.text = $"Lore Notes: {gameState.TotalNotesCollected}/10";
            }
        }

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Play appropriate sound
        AudioClip soundToPlay = isTrueEnding ? trueEndingSound : winSound;
        if (soundToPlay != null && audioSource != null)
        {
            audioSource.PlayOneShot(soundToPlay);
        }
    }

    private void ContinueToShop()
    {
        HideWinScreen();
        
        if (gameState != null)
        {
            gameState.OpenShop();
        }
    }

    private void ReturnToMainMenu()
    {
        HideWinScreen();
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private void HideWinScreen()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (trueEndingPanel != null)
        {
            trueEndingPanel.SetActive(false);
        }
    }
}
