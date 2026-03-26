using UnityEngine;

/// <summary>
/// Controls atmospheric effects like fog and ambient lighting.
/// Unity 2022.3.62f1 compatible.
/// </summary>
public class AtmosphereController : MonoBehaviour
{
    [Header("Fog Settings")]
    [SerializeField] private bool enableFog = true;
    [SerializeField] private Color normalFogColor = new Color(0.1f, 0.1f, 0.15f);
    [SerializeField] private Color dangerFogColor = new Color(0.15f, 0.05f, 0.05f);
    [SerializeField] private float normalFogDensity = 0.02f;
    [SerializeField] private float dangerFogDensity = 0.04f;
    [SerializeField] private float fogTransitionSpeed = 2f;

    [Header("Ambient Light")]
    [SerializeField] private Color normalAmbientColor = new Color(0.1f, 0.1f, 0.15f);
    [SerializeField] private Color dangerAmbientColor = new Color(0.15f, 0.05f, 0.05f);

    [Header("References")]
    [SerializeField] private AIChaser enemy;

    private bool isDangerMode = false;
    private float targetFogDensity;
    private Color targetFogColor;
    private Color targetAmbientColor;

    private void Awake()
    {
        RenderSettings.fog = enableFog;
        RenderSettings.fogColor = normalFogColor;
        RenderSettings.fogDensity = normalFogDensity;
        RenderSettings.fogMode = FogMode.Exponential;

        RenderSettings.ambientLight = normalAmbientColor;
        RenderSettings.ambientMode = AmbientMode.Flat;

        targetFogDensity = normalFogDensity;
        targetFogColor = normalFogColor;
        targetAmbientColor = normalAmbientColor;
    }

    private void Start()
    {
        if (enemy != null)
        {
            enemy.OnStateChanged += OnAIStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (enemy != null)
        {
            enemy.OnStateChanged -= OnAIStateChanged;
        }
    }

    private void Update()
    {
        RenderSettings.fogDensity = Mathf.Lerp(
            RenderSettings.fogDensity,
            targetFogDensity,
            fogTransitionSpeed * Time.deltaTime
        );

        RenderSettings.fogColor = Color.Lerp(
            RenderSettings.fogColor,
            targetFogColor,
            fogTransitionSpeed * Time.deltaTime
        );

        RenderSettings.ambientLight = Color.Lerp(
            RenderSettings.ambientLight,
            targetAmbientColor,
            fogTransitionSpeed * Time.deltaTime
        );
    }

    private void OnAIStateChanged(AIChaser.AIState state)
    {
        if (state == AIChaser.AIState.Chasing)
        {
            SetDangerMode(true);
        }
        else if (isDangerMode)
        {
            SetDangerMode(false);
        }
    }

    public void SetDangerMode(bool danger)
    {
        isDangerMode = danger;

        if (danger)
        {
            targetFogDensity = dangerFogDensity;
            targetFogColor = dangerFogColor;
            targetAmbientColor = dangerAmbientColor;
        }
        else
        {
            targetFogDensity = normalFogDensity;
            targetFogColor = normalFogColor;
            targetAmbientColor = normalAmbientColor;
        }
    }

    public void SetCustomAtmosphere(Color fogColor, float fogDensity, Color ambientColor)
    {
        targetFogColor = fogColor;
        targetFogDensity = fogDensity;
        targetAmbientColor = ambientColor;
    }
}
