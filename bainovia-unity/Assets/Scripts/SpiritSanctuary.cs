using UnityEngine;

/// <summary>
/// SpiritSanctuary - checkpoint rest point adapted from the Bonfire mechanic in
/// DragonSouls-Unity3D (btuhany, MIT). Kindling a sanctuary on the first interaction
/// lights its flame, sets the player's respawn point, and writes a save. Resting at
/// a kindled sanctuary restores health and spirit essence (the DragonSouls bonfire's
/// "ResetHealth / ResetStamina" mapped onto Bainovia's resources).
/// </summary>
public class SpiritSanctuary : Interactable
{
    [Header("Sanctuary Settings")]
    [SerializeField] private GameObject flameVFX;
    [SerializeField] private GameSaveSystem saveSystem;
    [SerializeField] private string sanctuaryId = "spirit_sanctuary";
    [SerializeField] private string sanctuaryName = "Spirit Sanctuary";
    [SerializeField] private bool isLit;

    protected override void Start()
    {
        base.Start();
        if (flameVFX != null)
            flameVFX.SetActive(isLit);
    }

    public override bool CanInteract()
    {
        // Unlike quest/item-gated Interactables, sanctuaries are always usable.
        return true;
    }

    public override void Interact(BainoviaCharacterController player)
    {
        if (!isLit)
        {
            Kindle(player);
            return;
        }

        TakeRest(player);
    }

    /// <summary>
    /// First interaction: light the flame (KindleBonfire equivalent), bind the
    /// respawn point to this location, and persist a save (the bonfire's respawn
    /// + RegisterKindledBonfire behavior).
    /// </summary>
    void Kindle(BainoviaCharacterController player)
    {
        isLit = true;
        if (flameVFX != null)
            flameVFX.SetActive(true);

        player.SetSpawnPoint(transform.position);

        if (saveSystem == null)
            saveSystem = FindObjectOfType<GameSaveSystem>();
        if (saveSystem != null)
            saveSystem.SaveGame();

        CombatFX.ShowText(transform.position + Vector3.up * 1.8f,
            sanctuaryName + " kindled. Your soul is bound here.",
            new Color(1f, 0.85f, 0.4f));

        Debug.Log($"{sanctuaryName} kindled - respawn point set.");
    }

    /// <summary>
    /// Rest at a kindled sanctuary: restore health and spirit essence (the
    /// bonfire's TakeRest core, without a stamina system).
    /// </summary>
    void TakeRest(BainoviaCharacterController player)
    {
        if (player != null)
            player.RestoreToFull();

        CombatFX.ShowText(transform.position + Vector3.up * 1.8f,
            "You rest. Vigor returns.",
            new Color(0.5f, 1f, 0.6f));

        Debug.Log("Rested at the " + sanctuaryName + ".");
    }

    public string SanctuaryId
    {
        get { return sanctuaryId; }
    }

    public bool IsLit
    {
        get { return isLit; }
    }

    public void SetLit(bool lit)
    {
        isLit = lit;
        if (flameVFX != null)
            flameVFX.SetActive(lit);
    }

    public override string GetInteractionText()
    {
        return isLit ? "Rest at the " + sanctuaryName : "Kindle the " + sanctuaryName;
    }
}