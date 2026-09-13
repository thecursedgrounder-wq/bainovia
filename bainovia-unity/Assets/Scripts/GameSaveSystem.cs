using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// GameSaveSystem - central quick-save / quick-load orchestrator (F5 save, F9 load).
/// Aggregates every sub-system's existing SaveData()/LoadData() plus the player's
/// live state and kindled sanctuaries. The JSON persistence approach loosely mirrors
/// the save handling in Skybound-unity (shawn-d123, MIT).
/// </summary>
public class GameSaveSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BainoviaCharacterController player;
    [SerializeField] private DialogueSystem dialogueSystem;
    [SerializeField] private FactionSystem factionSystem;
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private QuestSystem questSystem;
    [SerializeField] private SkillTreeSystem skillTreeSystem;
    [SerializeField] private SpiritCompanionSystem spiritCompanionSystem;
    [SerializeField] private RuneMagicSystem runeMagicSystem;
    [SerializeField] private SpiritSanctuary[] sanctuaries;

    [Header("Settings")]
    [SerializeField] private KeyCode saveKey = KeyCode.F5;
    [SerializeField] private KeyCode loadKey = KeyCode.F9;
    [SerializeField] private string saveDirectory = "Saves";
    [SerializeField] private string saveFileName = "slot1.sav";

    public static event Action OnGameSaved;
    public static event Action OnGameLoaded;

    private string savePath;

    void Awake()
    {
        ResolveReferences();
        savePath = Path.Combine(Application.persistentDataPath, saveDirectory, saveFileName);
    }

    void ResolveReferences()
    {
        if (player == null) player = FindObjectOfType<BainoviaCharacterController>();
        if (dialogueSystem == null) dialogueSystem = FindObjectOfType<DialogueSystem>();
        if (factionSystem == null) factionSystem = FindObjectOfType<FactionSystem>();
        if (inventorySystem == null) inventorySystem = FindObjectOfType<InventorySystem>();
        if (questSystem == null) questSystem = FindObjectOfType<QuestSystem>();
        if (skillTreeSystem == null) skillTreeSystem = FindObjectOfType<SkillTreeSystem>();
        if (spiritCompanionSystem == null) spiritCompanionSystem = FindObjectOfType<SpiritCompanionSystem>();
        if (runeMagicSystem == null) runeMagicSystem = FindObjectOfType<RuneMagicSystem>();
        if (sanctuaries == null || sanctuaries.Length == 0)
            sanctuaries = FindObjectsOfType<SpiritSanctuary>();
    }

    void Update()
    {
        if (Input.GetKeyDown(saveKey))
            SaveGame();
        if (Input.GetKeyDown(loadKey))
            LoadGame();
    }

    public string GetSavePath()
    {
        return savePath;
    }

    public void SaveGame()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            SaveFile file = new SaveFile();

            if (player != null)
            {
                Vector3 pos = player.transform.position;
                Vector3 spawn = player.GetSpawnPoint();
                file.posX = pos.x;
                file.posY = pos.y;
                file.posZ = pos.z;
                file.spawnX = spawn.x;
                file.spawnY = spawn.y;
                file.spawnZ = spawn.z;
                file.health = player.GetCurrentHealth();
                file.shield = player.GetCurrentShield();
                file.rage = player.GetRageMeterValue();
                file.essence = player.GetSpiritEssenceValue();
                file.isTransformed = player.IsTransformed();
                file.isBerserk = player.IsBerserk();
            }

            file.dialogue = dialogueSystem != null ? dialogueSystem.SaveData() : null;
            file.faction = factionSystem != null ? factionSystem.SaveData() : null;
            file.inventory = inventorySystem != null ? inventorySystem.SaveData() : null;
            file.quest = questSystem != null ? questSystem.SaveData() : null;
            file.skillTree = skillTreeSystem != null ? skillTreeSystem.SaveData() : null;
            file.spiritCompanion = spiritCompanionSystem != null ? spiritCompanionSystem.SaveData() : null;
            file.runeMagic = runeMagicSystem != null ? runeMagicSystem.SaveData() : null;

            if (sanctuaries != null)
            {
                file.sanctuaryStates = new List<SanctuaryState>();
                foreach (SpiritSanctuary san in sanctuaries)
                {
                    if (san != null)
                        file.sanctuaryStates.Add(new SanctuaryState { sanctuaryId = san.SanctuaryId, lit = san.IsLit });
                }
            }

            File.WriteAllText(savePath, JsonConvert.SerializeObject(file, Formatting.Indented));
            OnGameSaved?.Invoke();
            Debug.Log("Game saved to " + savePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Save failed: " + e.Message);
        }
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("No save found at " + savePath);
            return;
        }

        try
        {
            SaveFile file = JsonConvert.DeserializeObject<SaveFile>(File.ReadAllText(savePath));

            if (player != null)
            {
                player.transform.position = new Vector3(file.posX, file.posY, file.posZ);
                player.SetSpawnPoint(new Vector3(file.spawnX, file.spawnY, file.spawnZ));
                player.SetHealth(file.health);
                player.SetShield(file.shield);
                player.SetRageMeter(file.rage);
                player.SetSpiritEssence(file.essence);
                player.SetTransformationState(file.isTransformed);
            }

            if (dialogueSystem != null && file.dialogue != null)
                dialogueSystem.LoadData(file.dialogue);
            if (factionSystem != null && file.faction != null)
                factionSystem.LoadData(file.faction);
            if (inventorySystem != null && file.inventory != null)
                inventorySystem.LoadData(file.inventory);
            if (questSystem != null && file.quest != null)
                questSystem.LoadData(file.quest);
            if (skillTreeSystem != null && file.skillTree != null)
                skillTreeSystem.LoadData(file.skillTree);
            if (spiritCompanionSystem != null && file.spiritCompanion != null)
                spiritCompanionSystem.LoadData(file.spiritCompanion);
            if (runeMagicSystem != null && file.runeMagic != null)
                runeMagicSystem.LoadData(file.runeMagic);

            if (sanctuaries != null && file.sanctuaryStates != null)
            {
                foreach (SpiritSanctuary san in sanctuaries)
                {
                    if (san == null) continue;
                    foreach (SanctuaryState state in file.sanctuaryStates)
                    {
                        if (state.sanctuaryId == san.SanctuaryId)
                            san.SetLit(state.lit);
                    }
                }
            }

            OnGameLoaded?.Invoke();
            Debug.Log("Game loaded from " + savePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Load failed: " + e.Message);
        }
    }

    [Serializable]
    public class SaveFile
    {
        public float posX, posY, posZ;
        public float spawnX, spawnY, spawnZ;
        public int health;
        public int shield;
        public float rage;
        public int essence;
        public bool isTransformed;
        public bool isBerserk;
        public string dialogue;
        public string faction;
        public string inventory;
        public string quest;
        public string skillTree;
        public string spiritCompanion;
        public string runeMagic;
        public List<SanctuaryState> sanctuaryStates;
    }

    [Serializable]
    public class SanctuaryState
    {
        public string sanctuaryId;
        public bool lit;
    }
}