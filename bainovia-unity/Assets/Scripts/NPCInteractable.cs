using UnityEngine;

/// <summary>
/// NPC Interactable - For talking to NPCs
/// </summary>
public class NPCInteractable : Interactable
{
    [Header("NPC Settings")]
    [SerializeField] private string npcName;
    [SerializeField] private string startDialogueNodeId;
    [SerializeField] private DialogueSystem dialogueSystem;

    protected override void Start()
    {
        base.Start();
        if (dialogueSystem == null)
            dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    public override bool CanInteract()
    {
        return base.CanInteract() && dialogueSystem != null && !dialogueSystem.IsDialogueActive();
    }

    public override void Interact(BainoviaCharacterController player)
    {
        if (dialogueSystem != null && !string.IsNullOrEmpty(startDialogueNodeId))
        {
            dialogueSystem.StartDialogue(startDialogueNodeId);
        }
    }

    public override string GetInteractionText()
    {
        return $"Talk to {npcName}";
    }
}