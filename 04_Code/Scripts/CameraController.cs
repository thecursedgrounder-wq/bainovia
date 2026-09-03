using UnityEngine;

/// <summary>
/// Camera Controller - First-person camera control with mouse look
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform playerBody;
    [SerializeField, Range(50f, 800f)] private float mouseSensitivity = 250f;
    [SerializeField] private bool lockCursor = true;
    [SerializeField, Range(-89f, 0f)] private float minPitch = -80f;
    [SerializeField, Range(0f, 89f)] private float maxPitch = 80f;

    [Header("Camera Effects")]
    [SerializeField] private float bobSpeed = 10f;
    [SerializeField] private float bobAmount = 0.05f;
    [SerializeField] private bool enableHeadBob = true;

    [Header("Combat Camera")]
    [SerializeField] private float combatZoomFOV = 60f;
    [SerializeField] private float normalFOV = 90f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float combatShakeIntensity = 0.5f;
    [SerializeField] private float combatShakeDuration = 0.3f;

    private float pitch;
    private Camera mainCamera;
    private BainoviaCharacterController player;
    private float defaultFOV;
    private float shakeTimer;
    private Vector3 originalPosition;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
            mainCamera = Camera.main;

        player = FindObjectOfType<BainoviaCharacterController>();
        defaultFOV = mainCamera.fieldOfView;
        originalPosition = transform.localPosition;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnDisable()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void Update()
    {
        HandleCameraRotation();
        HandleHeadBob();
        HandleCombatZoom();
        HandleCameraShake();
    }

    void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    void HandleHeadBob()
    {
        if (!enableHeadBob || player == null)
            return;

        // Check if player is moving
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        bool isMoving = moveDirection.magnitude > 0.1f;

        if (isMoving && player != null)
        {
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.localPosition = originalPosition + Vector3.up * bobOffset;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * 10f);
        }
    }

    void HandleCombatZoom()
    {
        if (player == null || mainCamera == null)
            return;

        bool isInCombat = Input.GetMouseButton(0) || Input.GetMouseButton(1);
        float targetFOV = isInCombat ? combatZoomFOV : normalFOV;

        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }

    void HandleCameraShake()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            if (shakeTimer > 0)
            {
                Vector3 shakeOffset = Random.insideUnitSphere * combatShakeIntensity;
                transform.localPosition += shakeOffset;
            }
            else
            {
                shakeTimer = 0f;
            }
        }
    }

    public void TriggerCombatShake()
    {
        shakeTimer = combatShakeDuration;
    }

    public void SetSensitivity(float sensitivity)
    {
        mouseSensitivity = Mathf.Clamp(sensitivity, 50f, 800f);
    }

    public void SetLockCursor(bool lockState)
    {
        lockCursor = lockState;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public float GetPitch()
    {
        return pitch;
    }

    public void SetPitch(float newPitch)
    {
        pitch = Mathf.Clamp(newPitch, minPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
