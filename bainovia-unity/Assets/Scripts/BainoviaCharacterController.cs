using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Bainovia Character Controller - Main controller for Kaelar "Storm-Breaker" Vane
/// League-style controls: right-click to move / attack-move, left-click to attack,
/// abilities on Q W E R F D aimed at the cursor (backtick = target-only), Space dodge, T interact.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class BainoviaCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 8f;
    public float rotationSpeed = 360f;
    public float jumpForce = 8f;
    public float gravity = -9.81f;

    [Header("Combat Settings")]
    public float attackRange = 2.5f;
    public int lightAttackDamage = 25;
    public int heavyAttackDamage = 50;
    public float attackCooldown = 1.2f;
    public LayerMask enemyLayers;

    [Header("Rune Magic Settings")]
    public float runeCooldown = 8f;
    public float spiritCooldown = 15f;
    public int maxSpiritEssence = 100;
    public int currentSpiritEssence;

    [Header("Berserk Settings")]
    public float berserkDuration = 15f;
    public float berserkDamageMultiplier = 1.5f;
    public float berserkSpeedMultiplier = 1.3f;
    public float berserkDefenseReduction = 0.25f;

    [Header("Transformation Settings")]
    public bool isTransformed = false;
    public float transformationDuration = 2f;
    public float transformationCooldown = 30f;
    public float werebearDamageMultiplier = 2f;
    public float werebearSpeedMultiplier = 1.2f;
    public float werebearHealthMultiplier = 1.5f;
    public GameObject humanModel;
    public GameObject werebearModel;
    public GameObject transformationVFX;
    public AudioClip transformationSound;

    [Header("Health Settings")]
    public int maxHealth = 200;
    public int currentHealth;

    [Header("Spirit Shield")]
    public int maxShield = 100;
    public int currentShield;

    [Header("References")]
    public Transform attackPoint;
    public GameObject[] runeVFX;
    public AudioClip[] combatSounds;
    public AudioClip[] magicSounds;
    public RuneMagicSystem runeSystem;
    public CombatSystem combatSystem;
    public DialogueSystem dialogueSystem;

    // Components
    private CharacterController controller;
    private Animator animator;
    private AudioSource audioSource;
    private NavMeshAgent navAgent;

    // State
    private Vector3 velocity;
    private bool isGrounded;
    private bool isAttacking;
    private bool isBerserk;
    private bool isDead;
    private float attackTimer;
    private float runeTimer;
    private float spiritTimer;
    private float berserkTimer;
    private float transformationTimer;
    private float rageMeter = 0f;
    private const float maxRageMeter = 100f;
    private int baseMaxHealth;
    private int baseLightAttackDamage;
    private int baseHeavyAttackDamage;
    private float baseWalkSpeed;
    private float baseRunSpeed;

    // Click-to-move combat target
    private Health engageTarget;

    // Animation parameters
    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int IsGroundedParam = Animator.StringToHash("IsGrounded");
    private static readonly int AttackParam = Animator.StringToHash("Attack");
    private static readonly int HeavyAttackParam = Animator.StringToHash("HeavyAttack");
    private static readonly int IsBerserkParam = Animator.StringToHash("IsBerserk");
    private static readonly int IsDeadParam = Animator.StringToHash("IsDead");
    private static readonly int CastParam = Animator.StringToHash("Cast");
    private static readonly int HurtParam = Animator.StringToHash("Hurt");

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        navAgent = GetComponent<NavMeshAgent>();

        // Initialize models: start as the werebear (Kaelar, canonical form).
        if (humanModel != null && werebearModel != null)
        {
            werebearModel.SetActive(true);
            humanModel.SetActive(false);
            isTransformed = true;
        }

        // The human root Animator is a dummy (empty controller); all gameplay
        // animation params belong on the werebear's own Animator. Force-route to it
        // so Speed/Attack/Cast/Hurt drive the bear, not the empty root controller.
        var original = animator;
        animator = werebearModel != null ? werebearModel.GetComponentInChildren<Animator>(true) : null;
        if (animator == null)
        {
            animator = original;
            Debug.LogWarning("BainoviaCharacterController: no Animator found on werebear; falling back to root.");
        }
        else if (original != animator)
        {
            Debug.Log($"BainoviaCharacterController: animator routed to {animator.gameObject.name} ctrl={animator.runtimeAnimatorController?.name}");
        }

        // Store base values for transformation
        baseMaxHealth = maxHealth;
        baseLightAttackDamage = lightAttackDamage;
        baseHeavyAttackDamage = heavyAttackDamage;
        baseWalkSpeed = walkSpeed;
        baseRunSpeed = runSpeed;

        currentHealth = maxHealth;
        currentShield = 0;
        currentSpiritEssence = maxSpiritEssence;
        transformationTimer = transformationCooldown;

        // League-style movement uses the NavMeshAgent; the required CharacterController
        // stays disabled so the two motion drivers never fight.
        if (navAgent != null)
        {
            navAgent.enabled = true;
            navAgent.speed = runSpeed;
            navAgent.stoppingDistance = 0.15f;
            spawnPoint = transform.position;
            if (controller != null)
                controller.enabled = false;
        }
        else
        {
            Debug.LogWarning("BainoviaCharacterController: no NavMeshAgent found - click-to-move disabled.");
            spawnPoint = transform.position;
        }

        if (runeSystem == null)
            runeSystem = GetComponent<RuneMagicSystem>();
        if (combatSystem == null)
            combatSystem = GetComponent<CombatSystem>();
        if (dialogueSystem == null)
            dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    void Update()
    {
        if (isDead) return;

        HandleMovement();
        HandleCombat();
        HandleMagic();
        HandleTimers();
        HandleInput();
    }

    // ---------------------------------------------------------------- movement

    bool CanControl()
    {
        return dialogueSystem == null || !dialogueSystem.IsDialogueActive();
    }

    void HandleMovement()
    {
        isGrounded = true;
        if (animator != null) animator.SetBool(IsGroundedParam, isGrounded);

        bool orbiting = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        // Right-click: engage enemy under cursor, otherwise move to ground point.
        if (Input.GetMouseButtonDown(1) && !orbiting)
        {
            HandleRightClick();
        }

        // Move toward the engaged target each frame.
        if (engageTarget != null)
        {
            if (engageTarget.IsDead())
            {
                ClearEngage();
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, engageTarget.transform.position);
            if (distanceToTarget > attackRange * 0.85f)
            {
                if (navAgent != null)
                {
                    navAgent.stoppingDistance = attackRange * 0.85f;
                    navAgent.SetDestination(engageTarget.transform.position);
                }
            }
            FaceTarget(engageTarget.transform.position);
        }

        // Animator speed from agent velocity.
        float speed = navAgent != null ? navAgent.velocity.magnitude : 0f;
        if (animator != null)
            animator.SetFloat(SpeedParam, speed > 0.1f ? Mathf.Clamp01(speed / runSpeed) : 0f, 0.1f, Time.deltaTime);

        // Face movement direction when not locked on a target.
        if (engageTarget == null && navAgent != null && navAgent.velocity.magnitude > 0.3f)
        {
            Vector3 dir = navAgent.desiredVelocity;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    Quaternion.LookRotation(dir),
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    void HandleRightClick()
    {
        Health clicked = GetEnemyUnderCursor();

        if (clicked != null)
        {
            engageTarget = clicked;
            if (navAgent != null)
            {
                navAgent.stoppingDistance = attackRange * 0.85f;
                navAgent.SetDestination(clicked.transform.position);
            }
            return;
        }

        ClearEngage();

        Vector3 destination = GetGroundPoint();
        if (navAgent != null)
        {
            navAgent.stoppingDistance = 0.15f;
            navAgent.SetDestination(destination);
        }
    }

    void ClearEngage()
    {
        engageTarget = null;
        if (navAgent != null && navAgent.hasPath)
            navAgent.SetDestination(transform.position);
    }

    Health GetEnemyUnderCursor()
    {
        if (Camera.main == null)
            return null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, enemyLayers))
        {
            Health h = hit.collider.GetComponent<Health>();
            if (h != null && !h.IsDead())
                return h;
        }
        return null;
    }

    Vector3 GetGroundPoint()
    {
        if (Camera.main == null)
            return transform.position;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int mask = ~(LayerMask.GetMask("Enemy", "Interactable"));

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, mask))
            return hit.point;

        return transform.position;
    }

    void FaceTarget(Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude <= 0.01f)
            return;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(dir),
            rotationSpeed * Time.deltaTime
        );
    }

    // ---------------------------------------------------------------- combat

    void HandleCombat()
    {
        if (!CanControl())
            return;

        // Left-click: light attack; clicking an enemy also engages it.
        if (Input.GetMouseButtonDown(0) && attackTimer <= 0 && !isAttacking)
        {
            Health clicked = GetEnemyUnderCursor();
            if (clicked != null)
            {
                engageTarget = clicked;
                if (navAgent != null)
                {
                    navAgent.stoppingDistance = attackRange * 0.85f;
                    navAgent.SetDestination(clicked.transform.position);
                }
            }
            else
            {
                PerformLightAttack();
            }
        }

        // Right-click: heavy attack with knockback. Right-click also moves/engages
        // in HandleMovement, so only heavy when actually clicking an enemy within
        // reach (and never while orbiting the camera with Alt+right-drag).
        bool orbiting = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        if (Input.GetMouseButtonDown(1) && !orbiting && attackTimer <= 0 && !isAttacking)
        {
            Health clicked = GetEnemyUnderCursor();
            if (clicked != null &&
                Vector3.Distance(transform.position, clicked.transform.position) <= attackRange * 1.4f)
            {
                PerformHeavyAttack();
            }
        }

        // Auto-attack the engaged target when in range.
        if (engageTarget != null && !engageTarget.IsDead())
        {
            float distanceToTarget = Vector3.Distance(transform.position, engageTarget.transform.position);
            FaceTarget(engageTarget.transform.position);

            if (distanceToTarget <= attackRange && attackTimer <= 0 && !isAttacking)
            {
                PerformLightAttack();
            }
        }

        // Dodge (Space)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            PerformDodge();
        }
    }

    void PerformLightAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;
        if (animator != null) animator.SetTrigger(AttackParam);

        if (combatSounds.Length > 0 && audioSource != null)
            audioSource.PlayOneShot(combatSounds[0]);

        Invoke(nameof(DetectLightHit), 0.3f);
        Invoke(nameof(ResetAttackState), 0.8f);
    }

    void PerformHeavyAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown * 1.5f;
        if (animator != null) animator.SetTrigger(HeavyAttackParam);

        if (combatSounds.Length > 1 && audioSource != null)
            audioSource.PlayOneShot(combatSounds[1]);

        HitStop.Pause(0.08f);

        Invoke(nameof(DetectHeavyHit), 0.45f);
        Invoke(nameof(ResetAttackState), 1.1f);
    }

    void DetectHeavyHit()
    {
        int damage = isBerserk ? Mathf.RoundToInt(heavyAttackDamage * berserkDamageMultiplier) : heavyAttackDamage;
        DealDamage(damage, attackRange * 1.2f);
        ApplyKnockback(attackRange * 1.2f, 10f);
    }

    void DetectLightHit()
    {
        int damage = isBerserk ? Mathf.RoundToInt(lightAttackDamage * berserkDamageMultiplier) : lightAttackDamage;
        DealDamage(damage, attackRange);
    }

    void DealDamage(int damage, float range)
    {
        if (attackPoint == null) return;
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, range, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                BuildRage(5f);
            }

            CombatFX.ShowDamage(enemy.transform.position, damage, Color.white);
        }
    }

    void ApplyKnockback(float range, float force)
    {
        if (attackPoint == null) return;
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, range, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                rb.AddForce(knockbackDirection * force, ForceMode.Impulse);
            }
        }
    }

    void PerformDodge()
    {
        if (navAgent != null)
        {
            navAgent.Move(transform.forward * 5f);
            BuildRage(2f);
        }
    }

    void ResetAttackState()
    {
        isAttacking = false;
    }

    // ---------------------------------------------------------------- rune magic

    void HandleMagic()
    {
        if (!CanControl())
            return;

        bool targetOnly = Input.GetKey(KeyCode.BackQuote);

        bool anyCast = false;
        if (Input.GetKeyDown(KeyCode.Q) && runeSystem != null)
            anyCast |= runeSystem.CastRune(RuneMagicSystem.RuneType.Storm, targetOnly);
        if (Input.GetKeyDown(KeyCode.W) && runeSystem != null)
            anyCast |= runeSystem.CastRune(RuneMagicSystem.RuneType.Frost, targetOnly);
        if (Input.GetKeyDown(KeyCode.E) && runeSystem != null)
            anyCast |= runeSystem.CastRune(RuneMagicSystem.RuneType.Wind, targetOnly);
        if (Input.GetKeyDown(KeyCode.R) && runeSystem != null)
            anyCast |= runeSystem.CastRune(RuneMagicSystem.RuneType.Spirit, targetOnly);
        if (Input.GetKeyDown(KeyCode.F) && runeSystem != null)
            anyCast |= runeSystem.CastRune(RuneMagicSystem.RuneType.Blood, targetOnly);
        if (Input.GetKeyDown(KeyCode.D) && runeSystem != null)
            anyCast |= runeSystem.CastRune(RuneMagicSystem.RuneType.Forbidden, targetOnly);

        // Only play the cast animation when the rune actually fired (cooldown and
        // essence are checked inside CastRune; failed casts should not fake a cast).
        if (anyCast && animator != null) animator.SetTrigger(CastParam);
    }

    void ActivateBerserk()
    {
        isBerserk = true;
        berserkTimer = berserkDuration;
        rageMeter = 0f;
        if (animator != null) animator.SetBool(IsBerserkParam, true);

        if (combatSounds.Length > 2 && audioSource != null)
            audioSource.PlayOneShot(combatSounds[2]);

        Debug.Log("BERSERK MODE ACTIVATED!");
    }

    void HandleTimers()
    {
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (runeTimer > 0) runeTimer -= Time.deltaTime;
        if (spiritTimer > 0) spiritTimer -= Time.deltaTime;
        if (transformationTimer > 0) transformationTimer -= Time.deltaTime;

        if (isBerserk)
        {
            berserkTimer -= Time.deltaTime;
            if (berserkTimer <= 0)
            {
                DeactivateBerserk();
            }
        }

        // Keep agent speed in sync with berserk (transformation removed).
        if (navAgent != null)
        {
            float speed = runSpeed;
            if (isBerserk) speed *= berserkSpeedMultiplier;
            navAgent.speed = speed;
        }

        // Auto-berserk once rage is full (rage otherwise has no spender).
        if (!isBerserk && !isDead && rageMeter >= maxRageMeter)
        {
            ActivateBerserk();
        }
    }

    void DeactivateBerserk()
    {
        isBerserk = false;
        if (animator != null) animator.SetBool(IsBerserkParam, false);
        Debug.Log("Berserk mode ended.");
    }

    void HandleInput()
    {
        // Debug/test inputs
        if (Input.GetKeyDown(KeyCode.J))
        {
            TakeDamage(10);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Heal(20);
        }

        // Toggle between werebear (URSIAN) and human form, canon: Kaelar "Storm-Breaker".
        if (Input.GetKeyDown(KeyCode.G) && humanModel != null && werebearModel != null)
        {
            isTransformed = !isTransformed;
            werebearModel.SetActive(isTransformed);
            humanModel.SetActive(!isTransformed);
            Debug.Log(isTransformed ? "Transformed into the werebear." : "Returned to human form.");
        }
    }

    // ---------------------------------------------------------------- damage / resources

    void BuildRage(float amount)
    {
        rageMeter = Mathf.Min(rageMeter + amount, maxRageMeter);
    }

    public void AddRage(float amount)
    {
        BuildRage(amount);
    }

    public void ApplySpiritShield(int amount)
    {
        currentShield = Mathf.Min(currentShield + amount, maxShield);
        CombatFX.ShowText(transform.position + Vector3.up * 1.5f, "+" + amount, new Color(0.5f, 0.8f, 1f));
        Debug.Log($"Spirit shield absorbed: {currentShield}/{maxShield}");
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        int remaining = damage;

        if (currentShield > 0)
        {
            int absorbed = Mathf.Min(currentShield, remaining);
            currentShield -= absorbed;
            remaining -= absorbed;
        }

        float actualDamage = isBerserk ? remaining * (1f + berserkDefenseReduction) : remaining;
        currentHealth -= Mathf.RoundToInt(actualDamage);
        BuildRage(5f);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Debug.Log($"Took {Mathf.RoundToInt(actualDamage)} damage. HP: {currentHealth}/{maxHealth}");
            if (animator != null) animator.SetTrigger(HurtParam);
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Healed {amount}. HP: {currentHealth}/{maxHealth}");
    }

    public void AddSpiritEssence(int amount)
    {
        if (runeSystem != null)
            runeSystem.AddSpiritEssence(amount);
        else
            currentSpiritEssence = Mathf.Min(currentSpiritEssence + amount, maxSpiritEssence);
    }

    private Vector3 spawnPoint;
    public float respawnDelay = 4f;

    void Die()
    {
        if (isDead) return;
        isDead = true;
        if (animator != null) animator.SetBool(IsDeadParam, true);
        if (navAgent != null)
        {
            navAgent.ResetPath();
            navAgent.enabled = false;
        }
        if (controller != null)
            controller.enabled = false;
        CancelInvoke();

        Debug.Log("Kaelar has fallen...");
        Invoke(nameof(Respawn), respawnDelay);
    }

    void Respawn()
    {
        transform.position = spawnPoint;
        transform.rotation = Quaternion.identity;
        currentHealth = maxHealth;
        currentShield = 0;
        rageMeter = 0;
        isBerserk = false;
        isDead = false;
        if (animator != null) animator.SetBool(IsDeadParam, false);
        if (navAgent != null)
            navAgent.enabled = true;
        if (controller != null)
            controller.enabled = false;
        Debug.Log("Kaelar stands again.");
    }

    public bool IsDead()
    {
        return isDead;
    }

    // ---------------------------------------------------------------- spawn point

    public void SetSpawnPoint(Vector3 point)
    {
        spawnPoint = point;
    }

    public Vector3 GetSpawnPoint()
    {
        return spawnPoint;
    }

    // ---------------------------------------------------------------- resource access (save system / sanctuary)

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealthValue()
    {
        return maxHealth;
    }

    public void SetHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
    }

    public int GetCurrentShield()
    {
        return currentShield;
    }

    public int GetMaxShieldValue()
    {
        return maxShield;
    }

    public void SetShield(int value)
    {
        currentShield = Mathf.Clamp(value, 0, maxShield);
    }

    public float GetRageMeterValue()
    {
        return rageMeter;
    }

    public void SetRageMeter(float value)
    {
        rageMeter = Mathf.Clamp(value, 0f, maxRageMeter);
    }

    public int GetSpiritEssenceValue()
    {
        if (runeSystem != null)
            return runeSystem.GetSpiritEssenceValue();
        return currentSpiritEssence;
    }

    public void SetSpiritEssence(int amount)
    {
        if (runeSystem != null)
            runeSystem.SetSpiritEssence(amount);
        else
            currentSpiritEssence = Mathf.Clamp(amount, 0, maxSpiritEssence);
    }

    public bool IsTransformed()
    {
        return isTransformed;
    }

    public void SetTransformationState(bool value)
    {
        isTransformed = value;
        if (humanModel != null && werebearModel != null)
        {
            werebearModel.SetActive(value);
            humanModel.SetActive(!value);
        }
    }

    /// <summary>
    /// Full rest used by the Spirit Sanctuary: health and spirit essence restored,
    /// shield and rage reset (mirrors the bonfire's reset-health behavior).
    /// </summary>
    public void RestoreToFull()
    {
        currentHealth = maxHealth;
        currentShield = 0;
        if (runeSystem != null)
            runeSystem.SetSpiritEssence(maxSpiritEssence);
        else
            currentSpiritEssence = maxSpiritEssence;
        rageMeter = 0f;
        isBerserk = false;
        if (animator != null) animator.SetBool(IsBerserkParam, false);
    }

    // Getters for UI
    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }

    public float GetSpiritEssencePercent()
    {
        if (runeSystem != null)
            return runeSystem.GetSpiritEssencePercent();
        return (float)currentSpiritEssence / maxSpiritEssence;
    }

    public float GetRageMeterPercent()
    {
        return rageMeter / maxRageMeter;
    }

    public float GetShieldPercent()
    {
        return (float)currentShield / maxShield;
    }

    public bool IsBerserk()
    {
        return isBerserk;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 4f);
    }
}