using UnityEngine;
using System.Collections.Generic;

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

    void Start()
    {
        InitializeSkills();
        availableSkillPoints = totalSkillPoints;
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
                    player.lightAttackDamage = Mathf.RoundToInt(player.lightAttackDamage * (1f + skill.currentLevel * 0.1f));
                    player.heavyAttackDamage = Mathf.RoundToInt(player.heavyAttackDamage * (1f + skill.currentLevel * 0.1f));
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
                    player.walkSpeed *= (1f + skill.currentLevel * 0.05f);
                    player.runSpeed *= (1f + skill.currentLevel * 0.05f);
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

        return JsonUtility.ToJson(data);
    }

    public void LoadData(string jsonData)
    {
        SkillTreeSaveData data = JsonUtility.FromJson<SkillTreeSaveData>(jsonData);
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
