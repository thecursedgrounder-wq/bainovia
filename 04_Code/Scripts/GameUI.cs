using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Game UI - Manages all HUD elements and menus
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("Health UI")]
    public Slider healthBar;
    public Text healthText;

    [Header("Spirit Essence UI")]
    public Slider spiritBar;
    public Text spiritText;

    [Header("Rage Meter UI")]
    public Slider rageBar;
    public Text rageText;
    public GameObject berserkIndicator;

    [Header("Rune Cooldown UI")]
    public Image[] runeCooldownImages;
    public Text[] runeCooldownTexts;

    [Header("Quest UI")]
    public GameObject questPanel;
    public Text questTitle;
    public Text questDescription;
    public Slider questProgress;

    [Header("References")]
    public BainoviaCharacterController player;
    public RuneMagicSystem runeSystem;
    public QuestSystem questSystem;

    void Start()
    {
        if (player == null)
            player = FindObjectOfType<BainoviaCharacterController>();

        if (runeSystem == null)
            runeSystem = FindObjectOfType<RuneMagicSystem>();

        if (questSystem == null)
            questSystem = FindObjectOfType<QuestSystem>();
    }

    void Update()
    {
        UpdateHealthUI();
        UpdateSpiritEssenceUI();
        UpdateRageMeterUI();
        UpdateRuneCooldownUI();
        UpdateQuestUI();
    }

    void UpdateHealthUI()
    {
        if (player == null || healthBar == null) return;

        float healthPercent = player.GetHealthPercent();
        healthBar.value = healthPercent;

        if (healthText != null)
        {
            healthText.text = $"{Mathf.Round(healthPercent * 100)}%";
        }
    }

    void UpdateSpiritEssenceUI()
    {
        if (runeSystem == null || spiritBar == null) return;

        float spiritPercent = runeSystem.GetSpiritEssencePercent();
        spiritBar.value = spiritPercent;

        if (spiritText != null)
        {
            spiritText.text = $"{Mathf.Round(spiritPercent * 100)}%";
        }
    }

    void UpdateRageMeterUI()
    {
        if (player == null || rageBar == null) return;

        float ragePercent = player.GetRageMeterPercent();
        rageBar.value = ragePercent;

        if (rageText != null)
        {
            rageText.text = $"{Mathf.Round(ragePercent * 100)}%";
        }

        if (berserkIndicator != null)
        {
            berserkIndicator.SetActive(player.IsBerserk());
        }
    }

    void UpdateRuneCooldownUI()
    {
        if (runeSystem == null) return;

        var availableRunes = runeSystem.GetAvailableRunes();
        for (int i = 0; i < runeCooldownImages.Length && i < availableRunes.Count; i++)
        {
            RuneMagicSystem.RuneType runeType = availableRunes[i];
            float cooldown = runeSystem.GetRuneCooldown(runeType);

            if (runeCooldownImages[i] != null)
            {
                runeCooldownImages[i].fillAmount = cooldown > 0 ? cooldown / 8f : 0f;
            }

            if (runeCooldownTexts[i] != null)
            {
                runeCooldownTexts[i].text = cooldown > 0 ? Mathf.Ceil(cooldown).ToString() : "";
            }
        }
    }

    void UpdateQuestUI()
    {
        if (questSystem == null || questPanel == null) return;

        var activeQuests = questSystem.GetActiveQuests();

        if (activeQuests.Count > 0)
        {
            QuestSystem.Quest currentQuest = activeQuests[0];
            float progress = questSystem.GetQuestProgress(currentQuest.questId);

            if (questTitle != null)
                questTitle.text = currentQuest.questName;

            if (questDescription != null)
                questDescription.text = currentQuest.description;

            if (questProgress != null)
                questProgress.value = progress;

            questPanel.SetActive(true);
        }
        else
        {
            questPanel.SetActive(false);
        }
    }

    public void ShowQuestPanel()
    {
        if (questPanel != null)
            questPanel.SetActive(true);
    }

    public void HideQuestPanel()
    {
        if (questPanel != null)
            questPanel.SetActive(false);
    }
}
