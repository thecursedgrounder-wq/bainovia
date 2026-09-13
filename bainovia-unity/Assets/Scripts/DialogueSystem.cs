using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Dialogue System - Manages branching dialogue with reputation-based options and spirit insight
/// </summary>
public class DialogueSystem : MonoBehaviour
{
    [System.Serializable]
    public class DialogueNode
    {
        public string nodeId;
        public string speakerName;
        public string dialogueText;
        public List<DialogueOption> options;
        public AudioClip voiceClip;
        public Sprite portrait;
    }

    [System.Serializable]
    public class DialogueOption
    {
        public string optionText;
        public string nextNodeId;
        public int requiredReputation;
        public string requiredFaction;
        public bool requiresSpiritInsight;
        public bool givesReputation;
        public int reputationAmount;
        public string reputationFaction;
        public bool givesQuest;
        public string questId;
        public bool endsDialogue;
    }

    [Header("Dialogue Data")]
    public List<DialogueNode> dialogueNodes;

    [Header("Current Dialogue")]
    private DialogueNode currentNode;
    private bool isDialogueActive;

    [Header("References")]
    public BainoviaCharacterController player;
    public FactionSystem factionSystem;
    public QuestSystem questSystem;
    public GameObject dialogueUI;

    [Header("UI References")]
    public UnityEngine.UI.Text speakerText;
    public UnityEngine.UI.Text dialogueText;
    public UnityEngine.UI.Text hintText;
    public UnityEngine.UI.Text[] optionTexts = new UnityEngine.UI.Text[9];

    private Dictionary<string, DialogueNode> nodeDictionary = new Dictionary<string, DialogueNode>();

    void Start()
    {
        InitializeDialogue();
    }

    void InitializeDialogue()
    {
        foreach (DialogueNode node in dialogueNodes)
        {
            nodeDictionary[node.nodeId] = node;
        }
    }

    public void StartDialogue(string startNodeId)
    {
        if (!nodeDictionary.ContainsKey(startNodeId))
        {
            Debug.Log($"Dialogue node not found: {startNodeId}");
            return;
        }

        currentNode = nodeDictionary[startNodeId];
        isDialogueActive = true;

        if (dialogueUI != null)
            dialogueUI.SetActive(true);

        DisplayNode(currentNode);
    }

    void DisplayNode(DialogueNode node)
    {
        // Render speaker and dialogue body to the UI.
        if (speakerText != null)
            speakerText.text = node.speakerName;

        if (dialogueText != null)
            dialogueText.text = node.dialogueText;

        if (hintText != null)
            hintText.text = "Press 1-9 to choose, ESC to exit";

        // Play voice clip if available
        if (node.voiceClip != null)
        {
            AudioSource.PlayClipAtPoint(node.voiceClip, Camera.main.transform.position);
        }

        // Display available options
        DisplayOptions(node.options);
    }

    void DisplayOptions(List<DialogueOption> options)
    {
        for (int i = 0; i < optionTexts.Length; i++)
        {
            var text = optionTexts[i];
            if (text == null)
                continue;

            if (i < options.Count)
            {
                text.gameObject.SetActive(true);
                text.text = (i + 1) + ". " + options[i].optionText;
                text.color = CanSelectOption(options[i]) ? Color.white : new Color(0.55f, 0.55f, 0.55f, 1f);
            }
            else
            {
                text.gameObject.SetActive(false);
            }
        }
    }

    bool CanSelectOption(DialogueOption option)
    {
        // Check reputation requirement
        if (option.requiredReputation > 0)
        {
            if (factionSystem != null)
            {
                int reputation = factionSystem.GetReputation(option.requiredFaction);
                if (reputation < option.requiredReputation)
                    return false;
            }
        }

        // Check spirit insight requirement
        if (option.requiresSpiritInsight)
        {
            if (player != null)
            {
                // Check if player has spirit insight ability
                // This would need to be implemented in the player controller
            }
        }

        return true;
    }

    public void SelectOption(int optionIndex)
    {
        if (currentNode == null || optionIndex >= currentNode.options.Count)
            return;

        DialogueOption selectedOption = currentNode.options[optionIndex];

        if (!CanSelectOption(selectedOption))
        {
            Debug.Log("Cannot select this option");
            return;
        }

        // Apply option effects
        ApplyOptionEffects(selectedOption);

        // Check if dialogue ends
        if (selectedOption.endsDialogue)
        {
            EndDialogue();
            return;
        }

        // Move to next node
        if (nodeDictionary.ContainsKey(selectedOption.nextNodeId))
        {
            currentNode = nodeDictionary[selectedOption.nextNodeId];
            DisplayNode(currentNode);
        }
        else
        {
            EndDialogue();
        }
    }

    void ApplyOptionEffects(DialogueOption option)
    {
        // Give reputation
        if (option.givesReputation && factionSystem != null)
        {
            factionSystem.AddReputation(option.reputationFaction, option.reputationAmount);
            Debug.Log($"Gained {option.reputationAmount} reputation with {option.reputationFaction}");
        }

        // Give quest
        if (option.givesQuest && questSystem != null)
        {
            questSystem.AcceptQuest(option.questId);
            Debug.Log($"Accepted quest: {option.questId}");
        }
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        currentNode = null;

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        Debug.Log("Dialogue ended");
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    public DialogueNode GetCurrentNode()
    {
        return currentNode;
    }

    public void AddDialogueNode(DialogueNode node)
    {
        dialogueNodes.Add(node);
        nodeDictionary[node.nodeId] = node;
    }

    public DialogueNode GetNode(string nodeId)
    {
        if (!nodeDictionary.ContainsKey(nodeId))
            return null;

        return nodeDictionary[nodeId];
    }

    // Spirit insight - reveals hidden dialogue options
    public void ActivateSpiritInsight()
    {
        if (!isDialogueActive || currentNode == null)
            return;

        Debug.Log("Spirit insight activated - hidden options revealed");

        // This would update the UI to show spirit insight options
    }

    // Memory fragments - unlock backstory through dialogue
    public void UnlockMemoryFragment(string memoryId)
    {
        Debug.Log($"Memory fragment unlocked: {memoryId}");

        // This would add to a collection of memory fragments
    }

    void Update()
    {
        // Handle dialogue input
        if (isDialogueActive)
        {
            HandleDialogueInput();
        }
    }

    void HandleDialogueInput()
    {
        // Number keys to select options
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (currentNode != null && i < currentNode.options.Count)
                {
                    SelectOption(i);
                }
            }
        }

        // Escape to end dialogue
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndDialogue();
        }
    }

    // Save/Load system
    public string SaveData()
    {
        DialogueSaveData data = new DialogueSaveData();
        data.isDialogueActive = isDialogueActive;
        data.currentNodeId = currentNode != null ? currentNode.nodeId : "";
        return JsonUtility.ToJson(data);
    }

    public void LoadData(string jsonData)
    {
        DialogueSaveData data = JsonUtility.FromJson<DialogueSaveData>(jsonData);
        isDialogueActive = data.isDialogueActive;

        if (isDialogueActive && !string.IsNullOrEmpty(data.currentNodeId))
        {
            StartDialogue(data.currentNodeId);
        }
    }

    [System.Serializable]
    private class DialogueSaveData
    {
        public bool isDialogueActive;
        public string currentNodeId;
    }
}
