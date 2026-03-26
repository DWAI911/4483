using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main HUD displaying player stats and inventory.
/// Unity 2022.3.62f1 compatible.
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Image staminaFill;
    [SerializeField] private Color staminaFullColor = Color.green;
    [SerializeField] private Color staminaLowColor = Color.red;

    [Header("Inventory Display")]
    [SerializeField] private TextMeshProUGUI keyCountText;
    [SerializeField] private TextMeshProUGUI fuseCountText;
    [SerializeField] private TextMeshProUGUI essenceCountText;

    [Header("Interaction")]
    [SerializeField] private TextMeshProUGUI interactPrompt;
    [SerializeField] private GameObject crosshair;

    [Header("AI State")]
    [SerializeField] private GameObject dangerIndicator;
    [SerializeField] private Image dangerPulse;
    [SerializeField] private float dangerPulseSpeed = 2f;

    [Header("Objectives")]
    [SerializeField] private TextMeshProUGUI objectiveText;
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
        if (player == null) player = FindObjectOfType<PlayerController>();
        if (inventory == null) inventory = FindObjectOfType<PlayerInventory>();
        if (enemy == null) enemy = FindObjectOfType<AIChaser>();

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
        UpdateKeyDisplay(0);
        UpdateFuseDisplay(0);
        UpdateEssenceDisplay(0);
        
        if (dangerIndicator != null)
        {
            dangerIndicator.SetActive(false);
        }

        UpdateObjective("Find 3 fuses to power the exit");
    }

    private void Update()
    {
        UpdateStamina();
        UpdateDangerIndicator();
    }

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
            int required = GameStateManager.Instance != null ? GameStateManager.Instance.FusesRequired : 3;
            fuseCountText.text = $"Fuses: {count}/{required}";
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

    private void OnAIStateChanged(AIChaser.AIState state)
    {
        isDangerActive = (state == AIChaser.AIState.Chasing);
        
        if (dangerIndicator != null)
        {
            dangerIndicator.SetActive(isDangerActive);
        }
    }

    private void UpdateDangerIndicator()
    {
        if (!isDangerActive || dangerPulse == null) return;

        float pulse = Mathf.Sin(Time.time * dangerPulseSpeed) * 0.5f + 0.5f;
        Color color = dangerPulse.color;
        color.a = pulse;
        dangerPulse.color = color;
    }

    public void UpdateObjective(string text)
    {
        if (objectiveText != null)
        {
            objectiveText.text = text;
        }
    }

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
