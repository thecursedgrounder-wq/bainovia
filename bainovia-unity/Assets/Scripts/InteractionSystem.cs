using UnityEngine;

/// <summary>
/// Interaction System - Handles NPC and object interactions with raycasting
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactableMask = ~0;
    [SerializeField] private KeyCode interactKey = KeyCode.T;
    [SerializeField] private KeyCode alternateInteractKey = KeyCode.N;

    [Header("UI References")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private UnityEngine.UI.Text promptText;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private BainoviaCharacterController player;

    private Interactable currentTarget;
    private bool canInteract;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (player == null)
            player = FindObjectOfType<BainoviaCharacterController>();
    }

    void Update()
    {
        CheckForInteractables();
        HandleInteractionInput();
        UpdatePrompt();
    }

    void CheckForInteractables()
    {
        if (playerCamera == null)
        {
            currentTarget = null;
            canInteract = false;
            return;
        }

        // Isometric camera: cast through the mouse cursor instead of camera.forward.
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, 100f, interactableMask, QueryTriggerInteraction.Ignore);

        if (hitSomething && player != null)
        {
            if (Vector3.Distance(hit.point, player.transform.position) > interactDistance)
            {
                hitSomething = false;
            }
        }

        if (hitSomething)
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null && interactable.CanInteract())
            {
                currentTarget = interactable;
                canInteract = true;
                return;
            }
        }

        currentTarget = null;
        canInteract = false;
    }

    void HandleInteractionInput()
    {
        if (canInteract && currentTarget != null)
        {
            // Primary interaction (E key)
            if (Input.GetKeyDown(interactKey))
            {
                currentTarget.Interact(player);
            }

            // Alternate interaction (Q key)
            if (Input.GetKeyDown(alternateInteractKey))
            {
                currentTarget.AlternateInteract(player);
            }
        }
    }

    void UpdatePrompt()
    {
        if (interactionPrompt == null)
            return;

        bool inDialogue = player != null && player.dialogueSystem != null && player.dialogueSystem.IsDialogueActive();
        if (canInteract && currentTarget != null && !inDialogue)
        {
            interactionPrompt.SetActive(true);

            if (promptText != null)
            {
                string prompt = $"[{interactKey}] {currentTarget.GetInteractionText()}";
                
                if (currentTarget.HasAlternateInteraction())
                {
                    prompt += $"  [{alternateInteractKey}] {currentTarget.GetAlternateInteractionText()}";
                }

                promptText.text = prompt;
            }
        }
        else
        {
            interactionPrompt.SetActive(false);
        }
    }

    public Interactable GetCurrentTarget()
    {
        return currentTarget;
    }

    public bool CanInteract()
    {
        return canInteract;
    }

    public void SetInteractDistance(float distance)
    {
        interactDistance = distance;
    }

    void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance);
        }
    }
}