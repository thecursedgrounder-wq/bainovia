using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Bainovia Character Controller - Main controller for Kaelar "Storm-Breaker" Vane
/// Handles movement, combat, rune magic, and spirit abilities
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

    [Header("References")]
    public Transform attackPoint;
    public GameObject[] runeVFX;
    public AudioClip[] combatSounds;
    public AudioClip[] magicSounds;

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

    // Animation parameters
    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int IsGroundedParam = Animator.StringToHash("IsGrounded");
    private static readonly int AttackParam = Animator.StringToHash("Attack");
    private static readonly int HeavyAttackParam = Animator.StringToHash("HeavyAttack");
    private static readonly int IsBerserkParam = Animator.StringToHash("IsBerserk");
    private static readonly int IsDeadParam = Animator.StringToHash("IsDead");
    private static readonly int HorizontalParam = Animator.StringToHash("Horizontal");
    private static readonly int VerticalParam = Animator.StringToHash("Vertical");

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        navAgent = GetComponent<NavMeshAgent>();

        // Store base values for transformation
        baseMaxHealth = maxHealth;
        baseLightAttackDamage = lightAttackDamage;
        baseHeavyAttackDamage = heavyAttackDamage;
        baseWalkSpeed = walkSpeed;
        baseRunSpeed = runSpeed;

        currentHealth = maxHealth;
        currentSpiritEssence = maxSpiritEssence;
        transformationTimer = transformationCooldown;

        // Disable NavMeshAgent if present (we use CharacterController)
        if (navAgent != null)
            navAgent.enabled = false;

        // Initialize models
        if (humanModel != null && werebearModel != null)
        {
            werebearModel.SetActive(false);
            humanModel.SetActive(true);
        }
    }

    void Update()
    {
        if (isDead) return;

        HandleMovement();
        HandleCombat();
        HandleMagic();
        HandleBerserk();
        HandleTransformation();
        HandleTimers();
        HandleInput();
    }

    void HandleMovement()
    {
        // Ground check
        isGrounded = controller.isGrounded;
        animator.SetBool(IsGroundedParam, isGrounded);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(horizontal, 0f, vertical);

        // Check for run
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Calculate speed
        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        if (isBerserk) targetSpeed *= berserkSpeedMultiplier;

        // Move
        if (move.magnitude > 0.1f)
        {
            // Rotate towards movement direction
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Move character
            controller.Move(transform.forward * targetSpeed * Time.deltaTime);

            // Update animator
            float normalizedSpeed = isRunning ? 1f : 0.5f;
            if (isBerserk) normalizedSpeed *= 1.2f;
            animator.SetFloat(SpeedParam, normalizedSpeed, 0.1f, Time.deltaTime);
            animator.SetFloat(HorizontalParam, horizontal);
            animator.SetFloat(VerticalParam, vertical);
        }
        else
        {
            animator.SetFloat(SpeedParam, 0f, 0.1f, Time.deltaTime);
            animator.SetFloat(HorizontalParam, 0f);
            animator.SetFloat(VerticalParam, 0f);
        }

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleCombat()
    {
        // Light attack
        if (Input.GetMouseButtonDown(0) && attackTimer <= 0 && !isAttacking)
        {
            PerformLightAttack();
        }

        // Heavy attack
        if (Input.GetMouseButtonDown(1) && attackTimer <= 0 && !isAttacking)
        {
            PerformHeavyAttack();
        }

        // Dodge
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            PerformDodge();
        }
    }

    void PerformLightAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;
        animator.SetTrigger(AttackParam);

        // Play sound
        if (combatSounds.Length > 0 && audioSource != null)
            audioSource.PlayOneShot(combatSounds[0]);

        // Damage detection with delay
        Invoke(nameof(DetectLightHit), 0.3f);

        // Reset attack state
        Invoke(nameof(ResetAttackState), 0.8f);
    }

    void PerformHeavyAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown * 1.5f;
        animator.SetTrigger(HeavyAttackParam);

        // Play sound
        if (combatSounds.Length > 1 && audioSource != null)
            audioSource.PlayOneShot(combatSounds[1]);

        // Damage detection with delay
        Invoke(nameof(DetectHeavyHit), 0.5f);

        // Reset attack state
        Invoke(nameof(ResetAttackState), 1.2f);

        // Build rage
        BuildRage(10f);
    }

    void DetectLightHit()
    {
        int damage = isBerserk ? Mathf.RoundToInt(lightAttackDamage * berserkDamageMultiplier) : lightAttackDamage;
        DealDamage(damage, attackRange);
    }

    void DetectHeavyHit()
    {
        int damage = isBerserk ? Mathf.RoundToInt(heavyAttackDamage * berserkDamageMultiplier) : heavyAttackDamage;
        DealDamage(damage, attackRange * 1.2f);

        // Knockback
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange * 1.2f, enemyLayers);
        foreach (Collider enemy in hitEnemies)
        {
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                rb.AddForce(knockbackDirection * 10f, ForceMode.Impulse);
            }
        }
    }

    void DealDamage(int damage, float range)
    {
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, range, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                BuildRage(5f);
            }

            Debug.Log($"Hit: {enemy.name} for {damage} damage");
        }
    }

    void PerformDodge()
    {
        // Spirit dash - quick movement with i-frames
        Vector3 dodgeDirection = transform.forward;
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            dodgeDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;
        }

        controller.Move(dodgeDirection * 5f);
        BuildRage(2f);
    }

    void ResetAttackState()
    {
        isAttacking = false;
    }

    void HandleMagic()
    {
        // Rune magic (Q key)
        if (Input.GetKeyDown(KeyCode.Q) && runeTimer <= 0 && currentSpiritEssence >= 20)
        {
            PerformRuneAttack();
        }

        // Spirit projection (E key)
        if (Input.GetKeyDown(KeyCode.E) && spiritTimer <= 0 && currentSpiritEssence >= 30)
        {
            PerformSpiritProjection();
        }

        // Spirit drain (R key hold)
        if (Input.GetKey(KeyCode.R))
        {
            PerformSpiritDrain();
        }
    }

    void PerformRuneAttack()
    {
        runeTimer = runeCooldown;
        currentSpiritEssence -= 20;

        // Play VFX
        if (runeVFX.Length > 0)
        {
            Instantiate(runeVFX[0], attackPoint.position, Quaternion.identity);
        }

        // Play sound
        if (magicSounds.Length > 0 && audioSource != null)
            audioSource.PlayOneShot(magicSounds[0]);

        // Deal damage
        DealDamage(50, attackRange * 1.5f);

        Debug.Log("Rune Attack!");
    }

    void PerformSpiritProjection()
    {
        spiritTimer = spiritCooldown;
        currentSpiritEssence -= 30;

        // Play VFX
        if (runeVFX.Length > 1)
        {
            Instantiate(runeVFX[1], transform.position, Quaternion.identity);
        }

        // Play sound
        if (magicSounds.Length > 1 && audioSource != null)
            audioSource.PlayOneShot(magicSounds[1]);

        // Spirit projection logic (simplified)
        Debug.Log("Spirit Projection!");
    }

    void PerformSpiritDrain()
    {
        // Check for nearby enemies
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, 3f, enemyLayers);

        foreach (Collider enemy in nearbyEnemies)
        {
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null && !enemyHealth.IsDead())
            {
                // Drain essence
                currentSpiritEssence = Mathf.Min(currentSpiritEssence + 1, maxSpiritEssence);
                enemyHealth.TakeDamage(5);
            }
        }
    }

    void HandleBerserk()
    {
        // Activate berserk when rage meter is full (F key)
        if (Input.GetKeyDown(KeyCode.F) && rageMeter >= maxRageMeter && !isBerserk)
        {
            ActivateBerserk();
        }
    }

    void HandleTransformation()
    {
        // Transformation input (T key)
        if (Input.GetKeyDown(KeyCode.T) && transformationTimer <= 0)
        {
            if (isTransformed)
            {
                TransformToHuman();
            }
            else
            {
                TransformToWerebear();
            }
        }
    }

    void ActivateBerserk()
    {
        isBerserk = true;
        berserkTimer = berserkDuration;
        rageMeter = 0f;
        animator.SetBool(IsBerserkParam, true);

        // Play sound
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

        // Passive spirit essence regeneration
        if (currentSpiritEssence < maxSpiritEssence)
        {
            currentSpiritEssence = Mathf.Min(currentSpiritEssence + 1, maxSpiritEssence);
        }
    }

    void DeactivateBerserk()
    {
        isBerserk = false;
        animator.SetBool(IsBerserkParam, false);
        Debug.Log("Berserk mode ended.");
    }

    void TransformToWerebear()
    {
        if (isTransformed) return;

        StartCoroutine(TransformationRoutine(true));
    }

    void TransformToHuman()
    {
        if (!isTransformed) return;

        StartCoroutine(TransformationRoutine(false));
    }

    System.Collections.IEnumerator TransformationRoutine(bool toWerebear)
    {
        // Play transformation sound
        if (transformationSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(transformationSound);
        }

        // Spawn transformation VFX
        if (transformationVFX != null)
        {
            Instantiate(transformationVFX, transform.position, Quaternion.identity);
        }

        // Disable movement during transformation
        controller.enabled = false;

        yield return new WaitForSeconds(transformationDuration * 0.5f);

        // Swap models
        if (humanModel != null && werebearModel != null)
        {
            humanModel.SetActive(!toWerebear);
            werebearModel.SetActive(toWerebear);
        }

        // Apply werebear bonuses
        if (toWerebear)
        {
            isTransformed = true;
            maxHealth = Mathf.RoundToInt(baseMaxHealth * werebearHealthMultiplier);
            currentHealth = Mathf.Min(currentHealth + Mathf.RoundToInt(maxHealth * 0.2f), maxHealth);
            lightAttackDamage = Mathf.RoundToInt(baseLightAttackDamage * werebearDamageMultiplier);
            heavyAttackDamage = Mathf.RoundToInt(baseHeavyAttackDamage * werebearDamageMultiplier);
            walkSpeed = baseWalkSpeed * werebearSpeedMultiplier;
            runSpeed = baseRunSpeed * werebearSpeedMultiplier;
            Debug.Log("Transformed to Werebear form!");
        }
        else
        {
            isTransformed = false;
            maxHealth = baseMaxHealth;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            lightAttackDamage = baseLightAttackDamage;
            heavyAttackDamage = baseHeavyAttackDamage;
            walkSpeed = baseWalkSpeed;
            runSpeed = baseRunSpeed;
            Debug.Log("Transformed to Human form!");
        }

        yield return new WaitForSeconds(transformationDuration * 0.5f);

        // Re-enable movement
        controller.enabled = true;

        // Set cooldown
        transformationTimer = transformationCooldown;
    }

    void HandleInput()
    {
        // Debug/test inputs
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            Heal(20);
        }
    }

    void BuildRage(float amount)
    {
        rageMeter = Mathf.Min(rageMeter + amount, maxRageMeter);
    }

    // Public methods for external systems
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        float actualDamage = isBerserk ? damage * (1f + berserkDefenseReduction) : damage;
        currentHealth -= Mathf.RoundToInt(actualDamage);
        BuildRage(5f);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Debug.Log($"Took {actualDamage} damage. HP: {currentHealth}/{maxHealth}");
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
        currentSpiritEssence = Mathf.Min(currentSpiritEssence + amount, maxSpiritEssence);
    }

    void Die()
    {
        isDead = true;
        animator.SetBool(IsDeadParam, true);
        controller.enabled = false;
        this.enabled = false;

        Debug.Log("Kaelar has fallen...");
    }

    // Getters for UI
    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }

    public float GetSpiritEssencePercent()
    {
        return (float)currentSpiritEssence / maxSpiritEssence;
    }

    public float GetRageMeterPercent()
    {
        return rageMeter / maxRageMeter;
    }

    public bool IsBerserk()
    {
        return isBerserk;
    }

    void OnDrawGizmosSelected()
    {
        // Attack range visualization
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        // Spirit drain range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 3f);
    }
}
