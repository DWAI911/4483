using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main HUD displaying player stats, inventory, and game state.
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Image staminaFill;
    [SerializeField] private Color staminaFullColor = Color.green;
    [SerializeField] private Color staminaLowColor = Color.red;

    [Header("Inventory Display")]
    [SerializeField] private Text keyCountText;
    [SerializeField] private Text fuseCountText;
    [SerializeField] private Text essenceCountText;
    [SerializeField] private Transform consumableContainer;
    [SerializeField] private GameObject consumableSlotPrefab;

    [Header("Interaction")]
    [SerializeField] private Text interactPrompt;
    [SerializeField] private GameObject crosshair;

    [Header("AI State")]
    [SerializeField] private GameObject dangerIndicator;
    [SerializeField] private Image dangerPulse;
    [SerializeField] private float dangerPulseSpeed = 2f;

    [Header("Objectives")]
    [SerializeField] private Text objectiveText;
    [SerializeField] private GameObject objectivePanel;

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private AIChaser enemy;

    private bool isDangerActive = false;

    private void Start()
    {
        FindReferences();
        InitializeHUD();
    }

    private void FindReferences()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }

        if (inventory == null)
        {
            inventory = FindObjectOfType<PlayerInventory>();
        }

        if (enemy == null)
        {
            enemy = FindObjectOfType<AIChaser>();
        }

        // Subscribe to events
        if (player != null)
        {
            // Stamina would be updated in Update loop for now
        }

        if (inventory != null)
        {
            inventory.OnKeysChanged += UpdateKeyDisplay;
            inventory.OnFusesChanged += UpdateFuseDisplay;
            inventory.OnFearEssenceChanged += UpdateEssenceDisplay;
        }

        if (enemy != null)
        {
            enemy.OnStateChanged += OnAIStateChanged;
        }
    }

    private void InitializeHUD()
    {
        // Set initial values
        UpdateStaminaDisplay(100f, 100f);
        UpdateKeyDisplay(0);
        UpdateFuseDisplay(0);
        UpdateEssenceDisplay(0);
        
        if (dangerIndicator != null)
        {
            dangerIndicator.SetActive(false);
        }

        if (objectivePanel != null)
        {
            UpdateObjective("Find 3 fuses to power the exit");
        }
    }

    private void Update()
    {
        UpdateStamina();
        UpdateDangerIndicator();
    }

    #region Player Stats
    private void UpdateStamina()
    {
        if (player == null || staminaSlider == null) return;

        float current = player.CurrentStamina;
        float max = player.MaxStamina;
        
        staminaSlider.value = current / max;
        
        if (staminaFill != null)
        {
            staminaFill.color = Color.Lerp(staminaLowColor, staminaFullColor, current / max);
        }
    }

    private void UpdateStaminaDisplay(float current, float max)
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = current / max;
        }
    }
    #endregion

    #region Inventory Display
    private void UpdateKeyDisplay(int count)
    {
        if (keyCountText != null)
        {
            keyCountText.text = $"Keys: {count}";
        }
    }

    private void UpdateFuseDisplay(int count)
    {
        if (fuseCountText != null)
        {
            fuseCountText.text = $"Fuses: {count}/{GameStateManager.Instance?.FusesRequired ?? 3}";
        }
    }

    private void UpdateEssenceDisplay(int count)
    {
        if (essenceCountText != null)
        {
            essenceCountText.text = $"Essence: {count}";
        }
    }

    public void ShowInteractPrompt(string text)
    {
        if (interactPrompt != null)
        {
            interactPrompt.text = text;
            interactPrompt.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }
    }

    public void HideInteractPrompt()
    {
        if (interactPrompt != null)
        {
            interactPrompt.gameObject.SetActive(false);
        }
    }
    #endregion

    #region AI State
    private void OnAIStateChanged(AIChaser.AIState state)
    {
        isDangerActive = (state == AIChaser.AIState.Chasing);
        
        if (dangerIndicator != null)
        {
            dangerIndicator.SetActive(isDangerActive);
        }

        // Could also change screen effects (vignette, color, etc.)
    }

    private void UpdateDangerIndicator()
    {
        if (!isDangerActive || dangerPulse == null) return;

        float pulse = Mathf.Sin(Time.time * dangerPulseSpeed) * 0.5f + 0.5f;
        Color color = dangerPulse.color;
        color.a = pulse;
        dangerPulse.color = color;
    }
    #endregion

    #region Objectives
    public void UpdateObjective(string text)
    {
        if (objectiveText != null)
        {
            objectiveText.text = text;
        }
    }

    public void ShowObjective(bool show)
    {
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(show);
        }
    }
    #endregion

    #region Crosshair
    public void SetCrosshairActive(bool active)
    {
        if (crosshair != null)
        {
            crosshair.SetActive(active);
        }
    }
    #endregion

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnKeysChanged -= UpdateKeyDisplay;
            inventory.OnFusesChanged -= UpdateFuseDisplay;
            inventory.OnFearEssenceChanged -= UpdateEssenceDisplay;
        }

        if (enemy != null)
        {
            enemy.OnStateChanged -= OnAIStateChanged;
        }
    }
}
