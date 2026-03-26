using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Death screen shown when player is caught.
/// Unity 2022.3.62f1 compatible.
/// </summary>
public class DeathScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private TextMeshProUGUI essenceEarnedText;
    [SerializeField] private TextMeshProUGUI totalEssenceText;
    [SerializeField] private TextMeshProUGUI runCountText;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button restartButton;

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

        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        if (shopButton != null) shopButton.onClick.AddListener(OpenShop);
        if (restartButton != null) restartButton.onClick.AddListener(RestartRun);
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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
}
