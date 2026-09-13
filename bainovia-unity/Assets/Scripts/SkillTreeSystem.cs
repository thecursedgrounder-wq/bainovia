using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Skill Tree System - Manages skill progression across Combat, Exploration, and Social branches
/// </summary>
public class SkillTreeSystem : MonoBehaviour
{
    [System.Serializable]
    public class Skill
    {
        public string skillId;
        public string skillName;
        public string description;
        public SkillBranch branch;
        public int maxLevel;
        public int currentLevel;
        public int skillPointsToUnlock;
        public int[] skillPointsPerLevel;
        public List<string> prerequisiteSkills;
        public bool isUnlocked;
        public Sprite icon;
    }

    public enum SkillBranch
    {
        Combat,
        Exploration,
        Social
    }

    [Header("Skill Points")]
    public int totalSkillPoints = 0;
    public int availableSkillPoints = 0;
    public int skillPointsPerLevel = 1;

    [Header("Skills")]
    public List<Skill> allSkills;

    [Header("References")]
    public BainoviaCharacterController player;
    public RuneMagicSystem runeSystem;
    public FactionSystem factionSystem;

    private Dictionary<string, Skill> skillDictionary = new Dictionary<string, Skill>();

    // Cached base stats so skill bonuses are computed from an un-mutated baseline
    // (prevents compounding stat inflation on repeated level-ups / loads)
    private int baseLightAttackDamage;
    private int baseHeavyAttackDamage;
    private float baseWalkSpeed;
    private float baseRunSpeed;
    private bool baseStatsCached = false;

    void Start()
    {
        InitializeSkills();
        availableSkillPoints = totalSkillPoints;
    }

    void CacheBaseStats()
    {
        if (baseStatsCached || player == null) return;
        baseLightAttackDamage = player.lightAttackDamage;
        baseHeavyAttackDamage = player.heavyAttackDamage;
        baseWalkSpeed = player.walkSpeed;
        baseRunSpeed = player.runSpeed;
        baseStatsCached = true;
    }

    void RestoreBaseStats()
    {
        if (baseStatsCached || player == null) return;
        player.lightAttackDamage = baseLightAttackDamage;
        player.heavyAttackDamage = baseHeavyAttackDamage;
        player.walkSpeed = baseWalkSpeed;
        player.runSpeed = baseRunSpeed;
    }

    void InitializeSkills()
    {
        foreach (Skill skill in allSkills)
        {
            skillDictionary[skill.skillId] = skill;
        }
    }

    public void AddSkillPoints(int amount)
    {
        totalSkillPoints += amount;
        availableSkillPoints += amount;
        Debug.Log($"Added {amount} skill points. Available: {availableSkillPoints}");
    }

    public bool UnlockSkill(string skillId)
    {
        if (!skillDictionary.ContainsKey(skillId))
            return false;

        Skill skill = skillDictionary[skillId];

        if (skill.isUnlocked)
            return false;

        if (availableSkillPoints < skill.skillPointsToUnlock)
            return false;

        if (!CheckPrerequisites(skill))
            return false;

        availableSkillPoints -= skill.skillPointsToUnlock;
        skill.isUnlocked = true;
        skill.currentLevel = 1;

        ApplySkillEffect(skill);

        Debug.Log($"Unlocked skill: {skill.skillName}");
        return true;
    }

    public bool UpgradeSkill(string skillId)
    {
        if (!skillDictionary.ContainsKey(skillId))
            return false;

        Skill skill = skillDictionary[skillId];

        if (!skill.isUnlocked)
            return false;

        if (skill.currentLevel >= skill.maxLevel)
            return false;

        int pointsNeeded = skill.skillPointsPerLevel[skill.currentLevel - 1];

        if (availableSkillPoints < pointsNeeded)
            return false;

        availableSkillPoints -= pointsNeeded;
        skill.currentLevel++;

        ApplySkillEffect(skill);

        Debug.Log($"Upgraded {skill.skillName} to level {skill.currentLevel}");
        return true;
    }

    bool CheckPrerequisites(Skill skill)
    {
        foreach (string prereqId in skill.prerequisiteSkills)
        {
            if (!skillDictionary.ContainsKey(prereqId))
                return false;

            Skill prereq = skillDictionary[prereqId];
            if (!prereq.isUnlocked)
                return false;
        }
        return true;
    }

    void ApplySkillEffect(Skill skill)
    {
        switch (skill.skillId)
        {
            // Combat Skills
            case "claw_mastery":
                if (player != null)
                {
                    CacheBaseStats();
                    // Recompute from base so the bonus is level-accurate and idempotent
                    player.lightAttackDamage = Mathf.RoundToInt(baseLightAttackDamage * (1f + skill.currentLevel * 0.1f));
                    player.heavyAttackDamage = Mathf.RoundToInt(baseHeavyAttackDamage * (1f + skill.currentLevel * 0.1f));
                }
                break;

            case "rune_proficiency":
                if (runeSystem != null)
                {
                    // Reduce rune cooldowns
                    foreach (var rune in runeSystem.GetAvailableRunes())
                    {
                        runeSystem.UpgradeRune(rune);
                    }
                }
                break;

            case "berserk_control":
                if (player != null)
                {
                    // Increase berserk duration
                    // This would need a reference to modify berserk settings
                }
                break;

            // Exploration Skills
            case "movement_speed":
                if (player != null)
                {
                    CacheBaseStats();
                    // Recompute from base so the bonus is level-accurate and idempotent
                    player.walkSpeed = baseWalkSpeed * (1f + skill.currentLevel * 0.05f);
                    player.runSpeed = baseRunSpeed * (1f + skill.currentLevel * 0.05f);
                }
                break;

            case "spirit_sense":
                // Increase spirit detection range
                break;

            // Social Skills
            case "dialogue_persuasion":
                // Unlock additional dialogue options
                break;

            case "trading_bonus":
                if (factionSystem != null)
                {
                    // Increase shop discounts
                }
                break;
        }
    }

    public Skill GetSkill(string skillId)
    {
        if (!skillDictionary.ContainsKey(skillId))
            return null;

        return skillDictionary[skillId];
    }

    public List<Skill> GetSkillsByBranch(SkillBranch branch)
    {
        List<Skill> branchSkills = new List<Skill>();
        foreach (Skill skill in allSkills)
        {
            if (skill.branch == branch)
            {
                branchSkills.Add(skill);
            }
        }
        return branchSkills;
    }

    public List<Skill> GetAvailableSkills()
    {
        List<Skill> available = new List<Skill>();
        foreach (Skill skill in allSkills)
        {
            if (!skill.isUnlocked && CheckPrerequisites(skill))
            {
                available.Add(skill);
            }
        }
        return available;
    }

    public List<Skill> GetUpgradeableSkills()
    {
        List<Skill> upgradeable = new List<Skill>();
        foreach (Skill skill in allSkills)
        {
            if (skill.isUnlocked && skill.currentLevel < skill.maxLevel)
            {
                upgradeable.Add(skill);
            }
        }
        return upgradeable;
    }

    public int GetTotalSkillPoints()
    {
        return totalSkillPoints;
    }

    public int GetAvailableSkillPoints()
    {
        return availableSkillPoints;
    }

    // Save/Load system
    public string SaveData()
    {
        SkillTreeSaveData data = new SkillTreeSaveData();
        data.totalSkillPoints = totalSkillPoints;
        data.availableSkillPoints = availableSkillPoints;

        foreach (Skill skill in allSkills)
        {
            data.skillStates[skill.skillId] = new SkillState
            {
                isUnlocked = skill.isUnlocked,
                currentLevel = skill.currentLevel
            };
        }

        // JsonUtility cannot serialize Dictionary; use System.Text.Json instead
        return JsonConvert.SerializeObject(data);
    }

    public void LoadData(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData)) return;
        SkillTreeSaveData data = JsonConvert.DeserializeObject<SkillTreeSaveData>(jsonData);
        if (data == null) return;
        totalSkillPoints = data.totalSkillPoints;
        availableSkillPoints = data.availableSkillPoints;

        foreach (var kvp in data.skillStates)
        {
            if (skillDictionary.ContainsKey(kvp.Key))
            {
                Skill skill = skillDictionary[kvp.Key];
                skill.isUnlocked = kvp.Value.isUnlocked;
                skill.currentLevel = kvp.Value.currentLevel;

                if (skill.isUnlocked)
                {
                    ApplySkillEffect(skill);
                }
            }
        }
    }

    [System.Serializable]
    private class SkillTreeSaveData
    {
        public int totalSkillPoints;
        public int availableSkillPoints;
        public Dictionary<string, SkillState> skillStates = new Dictionary<string, SkillState>();
    }

    [System.Serializable]
    private class SkillState
    {
        public bool isUnlocked;
        public int currentLevel;
    }
}

