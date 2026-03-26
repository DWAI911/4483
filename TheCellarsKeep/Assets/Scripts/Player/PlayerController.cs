using UnityEngine;

/// <summary>
/// Main player controller handling movement, camera, stamina, and noise generation.
/// Designed for horror gameplay with walk/run mechanics.
/// Unity 2022.3.62f1 compatible.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float staminaRegenDelay = 2f;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;

    [Header("Head Bob Settings")]
    [SerializeField] private float bobFrequency = 10f;
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float bobTransitionSpeed = 8f;

    [Header("Noise Settings")]
    [SerializeField] private float walkNoiseRadius = 5f;
    [SerializeField] private float runNoiseRadius = 15f;
    [SerializeField] private float noiseUpdateInterval = 0.5f;

    [Header("Flashlight")]
    [SerializeField] private Light flashlight;
    [SerializeField] private KeyCode flashlightKey = KeyCode.F;

    // Private variables
    private CharacterController characterController;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private float currentStamina;
    private float staminaRegenTimer;
    private bool isGrounded;
    private bool isRunning;
    private Vector3 cameraStartPosition;
    private float bobTimer;
    private float lastNoiseTime;
    private bool flashlightOn = false;

    // Public properties
    public bool IsRunning => isRunning;
    public bool IsMoving { get; private set; }
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float NoiseRadius { get; private set; }
    public Vector3 Position => transform.position;

    // Events
    public event System.Action<float> OnNoiseGenerated;
    public event System.Action OnDeath;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        if (cameraTransform == null)
        {
            Debug.LogError("Camera Transform not assigned!");
            enabled = false;
            return;
        }

        cameraStartPosition = cameraTransform.localPosition;
        currentStamina = maxStamina;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (flashlight != null)
            flashlight.enabled = flashlightOn;
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleCameraRotation();
        HandleMovement();
        HandleStamina();
        HandleHeadBob();
        HandleFlashlight();
        HandleNoiseGeneration();
        ApplyGravity();
    }

    private void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundMask);
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        IsMoving = moveDirection.magnitude > 0.1f;

        isRunning = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0 && IsMoving;

        float speed = isRunning ? runSpeed : walkSpeed;
        
        characterController.Move(moveDirection.normalized * speed * Time.deltaTime);
    }

    private void HandleStamina()
    {
        if (isRunning)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            staminaRegenTimer = staminaRegenDelay;

            if (currentStamina <= 0)
            {
                isRunning = false;
            }
        }
        else
        {
            staminaRegenTimer -= Time.deltaTime;
            
            if (staminaRegenTimer <= 0 && currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
            }
        }
    }

    private void HandleHeadBob()
    {
        if (!IsMoving || !isGrounded)
        {
            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                cameraStartPosition,
                bobTransitionSpeed * Time.deltaTime
            );
            bobTimer = 0f;
            return;
        }

        float speed = isRunning ? runSpeed : walkSpeed;
        bobTimer += Time.deltaTime * bobFrequency * (speed / walkSpeed);

        float bobOffsetY = Mathf.Sin(bobTimer) * bobAmplitude;
        float bobOffsetX = Mathf.Cos(bobTimer * 0.5f) * bobAmplitude * 0.5f;

        Vector3 newPosition = cameraStartPosition + new Vector3(bobOffsetX, bobOffsetY, 0f);
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            newPosition,
            bobTransitionSpeed * Time.deltaTime
        );
    }

    private void HandleFlashlight()
    {
        if (Input.GetKeyDown(flashlightKey))
        {
            flashlightOn = !flashlightOn;
            if (flashlight != null)
            {
                flashlight.enabled = flashlightOn;
            }
        }
    }

    private void HandleNoiseGeneration()
    {
        if (!IsMoving) 
        {
            NoiseRadius = 0f;
            return;
        }

        NoiseRadius = isRunning ? runNoiseRadius : walkNoiseRadius;

        if (Time.time - lastNoiseTime >= noiseUpdateInterval && NoiseRadius > 0)
        {
            lastNoiseTime = Time.time;
            OnNoiseGenerated?.Invoke(NoiseRadius);
        }
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    public void Kill()
    {
        OnDeath?.Invoke();
    }

    public void ResetStamina()
    {
        currentStamina = maxStamina;
        staminaRegenTimer = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, walkNoiseRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, runNoiseRadius);
    }
}
