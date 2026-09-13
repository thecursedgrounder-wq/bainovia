using UnityEngine;

/// <summary>
/// Camera Controller - Fixed-angle isometric follow camera (Albion / League style).
/// Tracks the player from a high angled view; Alt + right-drag orbits for scouting,
/// mouse wheel zooms, Space returns to the home angle/zoom. Cursor stays visible so
/// the player can right-click to move and aim runes.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform playerBody;
    public float followSmooth = 8f;
    public Vector3 positionOffset = new Vector3(0f, 2f, 0f);

    [Header("View Settings")]
    public float defaultDistance = 13f;
    public float defaultPitch = 16f;
    public float defaultYaw = 0f;
    public float minDistance = 8f;
    public float maxDistance = 24f;
    public float zoomSpeed = 3f;
    public float orbitSpeed = 3f;
    public float fieldOfView = 60f;

    [Header("Combat Camera")]
    public float combatShakeIntensity = 0.4f;
    public float combatShakeDuration = 0.25f;

    private Camera mainCamera;
    private float pitch;
    private float yaw;
    private float distance;
    private float shakeTimer;
    private bool orbitActive;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (playerBody == null)
        {
            var player = FindObjectOfType<BainoviaCharacterController>();
            if (player != null)
                playerBody = player.transform;
        }

        pitch = defaultPitch;
        yaw = defaultYaw;
        distance = defaultDistance;

        if (mainCamera != null)
            mainCamera.fieldOfView = fieldOfView;

        // Mouse-driven gameplay needs a free, visible cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ApplyTransforms(true);
    }

    void LateUpdate()
    {
        HandleOrbitZoom();
        ApplyTransforms(false);
        HandleCameraShake();
    }

    void HandleOrbitZoom()
    {
        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(wheel) > 0.001f)
        {
            distance = Mathf.Clamp(distance - wheel * zoomSpeed * 4f, minDistance, maxDistance);
        }

        bool orbitHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        if (orbitHeld && Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * orbitSpeed;
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * orbitSpeed, 8f, 50f);
            orbitActive = true;
        }
        else if (orbitActive)
        {
            // Decay back to the home angle once the player releases the orbit.
            yaw = Mathf.Lerp(yaw, defaultYaw, Time.deltaTime * 4f);
            pitch = Mathf.Lerp(pitch, defaultPitch, Time.deltaTime * 4f);

            if (Mathf.Abs(yaw - defaultYaw) < 0.05f && Mathf.Abs(pitch - defaultPitch) < 0.05f)
            {
                yaw = defaultYaw;
                pitch = defaultPitch;
                orbitActive = false;
            }
        }

        // Tab re-centers the view (Space is the dodge key).
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            distance = defaultDistance;
            yaw = defaultYaw;
            pitch = defaultPitch;
            orbitActive = false;
        }
    }

    void ApplyTransforms(bool snap)
    {
        if (playerBody == null)
            return;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPosition = playerBody.position + positionOffset + rotation * (Vector3.back * distance);

        if (snap)
        {
            transform.position = targetPosition;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSmooth);
        }

        transform.rotation = rotation;
    }

    void HandleCameraShake()
    {
        if (shakeTimer <= 0)
            return;

        shakeTimer -= Time.deltaTime;

        if (shakeTimer > 0)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * combatShakeIntensity * Time.deltaTime * 10f;
            shakeOffset.y *= 0.5f;
            transform.position += shakeOffset;
        }
        else
        {
            shakeTimer = 0f;
        }
    }

    public void TriggerCombatShake()
    {
        shakeTimer = combatShakeDuration;
    }

    public void SetSensitivity(float sensitivity)
    {
        orbitSpeed = Mathf.Clamp(sensitivity / 80f, 0.5f, 6f);
    }
}