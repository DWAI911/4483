using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Win screen shown when player successfully escapes.
/// Unity 2022.3.62f1 compatible.
/// </summary>
public class WinScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject trueEndingPanel;
    [SerializeField] private TextMeshProUGUI essenceEarnedText;
    [SerializeField] private TextMeshProUGUI totalEssenceText;
    [SerializeField] private TextMeshProUGUI notesCollectedText;
    [SerializeField] private Button continueButton;

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

        if (winPanel != null) winPanel.SetActive(false);
        if (trueEndingPanel != null) trueEndingPanel.SetActive(false);

        if (continueButton != null) continueButton.onClick.AddListener(ContinueToShop);
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

        if (gameState != null)
        {
            if (essenceEarnedText != null)
            {
                int bonus = 50;
                essenceEarnedText.text = $"Fear Essence Earned: +{gameState.CurrentRunEssence + bonus}";
            }

            if (totalEssenceText != null)
            {
                totalEssenceText.text = $"Total Fear Essence: {gameState.TotalFearEssence}";
            }

            if (notesCollectedText != null)
            {
                notesCollectedText.text = $"Lore Notes: {gameState.TotalNotesCollected}/10";
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AudioClip soundToPlay = isTrueEnding ? trueEndingSound : winSound;
        if (soundToPlay != null && audioSource != null)
        {
            audioSource.PlayOneShot(soundToPlay);
        }
    }

    private void ContinueToShop()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (trueEndingPanel != null) trueEndingPanel.SetActive(false);
        
        if (gameState != null)
        {
            gameState.OpenShop();
        }
    }
}
