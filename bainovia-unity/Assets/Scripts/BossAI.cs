using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// BossAI - phase-based boss fight for the Alpha Loup-Tonnerre. The structure
/// (chase / attack / telegraphs / charge) is adapted from the behavior-tree and
/// melee-boss patterns in DragonSouls-Unity3D (btuhany, MIT), trimmed to work
/// against Bainovia's click-to-move player:
///   Phase 1           base stats
///   Phase 2 (&lt;=66%)   faster + storm aura, more elemental attacks
///   Phase 3 (&lt;=33%)   enraged: raw damage spikes, rapid lightning
/// </summary>
public class BossAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float detectionRange = 25f;
    public float attackRange = 3.2f;
    public float chaseSpeed = 6f;
    public float attackCooldown = 1.8f;

    [Header("Combat - Combo")]
    public int comboDamage = 18;
    public int comboHits = 3;
    public float comboDelay = 0.35f;

    [Header("Combat - Roar")]
    public int roarDamage = 25;
    public float roarRadius = 6f;

    [Header("Combat - Lightning")]
    public int lightningDamage = 30;
    public float lightningRadius = 2.5f;
    public float lightningWindup = 0.9f;

    [Header("Combat - Charge")]
    public int chargeDamage = 35;
    public float chargeSpeed = 18f;
    public float chargeDuration = 0.8f;

    [Header("Phases")]
    public float phase2HealthRatio = 0.66f;
    public float phase3HealthRatio = 0.33f;
    public float phaseTransitionDelay = 1.5f;

    [Header("References")]
    public Health health;
    public Animator animator;
    public GameObject tempestAura;
    public GameObject lightingWarningPrefab;

    private NavMeshAgent navAgent;
    private BainoviaCharacterController player;
    private int currentPhase = 1;
    private float attackTimer;
    private bool isDead;
    private bool transitioning;
    private bool charging;
    private bool telegraphing;
    private Vector3 chargeDirection;
    private Coroutine actionRoutine;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (health == null) health = GetComponent<Health>();
        if (animator == null) animator = GetComponent<Animator>();

        if (player == null)
            player = FindObjectOfType<BainoviaCharacterController>();

        if (health != null)
        {
            health.onDeath.AddListener(OnBossDefeated);
            health.destroyOnDeath = false;
        }
    }

    void Update()
    {
        if (isDead || transitioning || charging || telegraphing)
            return;

        if (health != null && health.IsDead())
            return;

        if (player == null)
            player = FindObjectOfType<BainoviaCharacterController>();

        if (player == null || player.IsDead())
        {
            if (navAgent != null && navAgent.hasPath)
                navAgent.ResetPath();
            return;
        }

        CheckPhaseTransitions();

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= attackRange && attackTimer <= 0f && actionRoutine == null)
        {
            if (navAgent != null)
                navAgent.ResetPath();
            actionRoutine = StartCoroutine(RunAttack());
            return;
        }

        ChasePlayer();
    }

    void CheckPhaseTransitions()
    {
        if (health == null) return;

        float ratio = (float)health.GetCurrentHealth() / health.GetMaxHealth();
        if (ratio <= phase3HealthRatio && currentPhase < 3)
            StartCoroutine(EnterNextPhase(3));
        else if (ratio <= phase2HealthRatio && currentPhase < 2)
            StartCoroutine(EnterNextPhase(2));
    }

    void ChasePlayer()
    {
        if (navAgent == null || player == null)
            return;

        navAgent.speed = chaseSpeed;
        navAgent.SetDestination(player.transform.position);

        if (Vector3.Distance(transform.position, player.transform.position) > 1.2f)
        {
            Vector3 dir = player.transform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    Quaternion.LookRotation(dir),
                    720f * Time.deltaTime);
            }
        }
    }

    IEnumerator RunAttack()
    {
        if (player == null)
            yield break;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        int choice = PickAttack(dist);

        switch (choice)
        {
            case 0:
                yield return StartCoroutine(RunCombo());
                break;
            case 1:
                yield return StartCoroutine(RunRoar());
                break;
            case 2:
                yield return StartCoroutine(RunLightning());
                break;
            case 3:
                yield return StartCoroutine(RunCharge());
                break;
        }

        actionRoutine = null;
        attackTimer = AttackCooldownForPhase();
    }

    int PickAttack(float distance)
    {
        // Close range favours melee; at range the pack leader zaps or charges.
        int stormBonus = currentPhase >= 2 ? 5 : 0;
        int chargeWeight = currentPhase >= 2 ? 20 : 10;

        int comboWeight = distance <= attackRange ? 45 : 5;
        int roarWeight = distance <= attackRange ? 20 : 5;
        int lightningWeight = distance > attackRange ? 45 + stormBonus : 25 + stormBonus;
        int cWeight = distance > attackRange ? chargeWeight : 10;

        int total = comboWeight + roarWeight + lightningWeight + cWeight;
        int roll = Random.Range(0, total);

        if (roll < comboWeight) return 0;
        roll -= comboWeight;
        if (roll < roarWeight) return 1;
        roll -= roarWeight;
        if (roll < lightningWeight) return 2;
        return 3;
    }

    IEnumerator RunCombo()
    {
        if (player == null) yield break;

        for (int i = 0; i < comboHits; i++)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist <= attackRange * 1.4f)
            {
                int dmg = Mathf.RoundToInt(comboDamage * DamageMultiplier());
                player.TakeDamage(dmg);
                HitStop.Pause(0.06f);
                if (animator != null) animator.SetTrigger("Attack");
            }

            if (i < comboHits - 1)
                yield return new WaitForSecondsRealtime(comboDelay);
            else
                yield return new WaitForSecondsRealtime(0.3f);
        }
    }

    IEnumerator RunRoar()
    {
        if (player == null) yield break;

        if (animator != null) animator.SetTrigger("Roar");
        yield return new WaitForSecondsRealtime(0.4f);

        Collider[] targets = Physics.OverlapSphere(transform.position, roarRadius);
        foreach (Collider col in targets)
        {
            if (col.CompareTag("Player"))
            {
                player.TakeDamage(Mathf.RoundToInt(roarDamage * DamageMultiplier()));
            }
        }

        HitStop.Pause(0.08f);
        yield return new WaitForSecondsRealtime(0.5f);
    }

    IEnumerator RunLightning()
    {
        if (player == null) yield break;

        telegraphing = true;
        if (navAgent != null)
            navAgent.ResetPath();

        Vector3 strikePoint = player.transform.position;
        GameObject warning = null;
        if (lightingWarningPrefab != null)
        {
            warning = Instantiate(lightingWarningPrefab, strikePoint, Quaternion.identity);
            warning.transform.localScale = Vector3.one * (lightningRadius * 2f);
        }

        yield return new WaitForSecondsRealtime(lightningWindup);
        telegraphing = false;

        Collider[] targets = Physics.OverlapSphere(strikePoint, lightningRadius);
        foreach (Collider col in targets)
        {
            if (col.CompareTag("Player"))
            {
                player.TakeDamage(Mathf.RoundToInt(lightningDamage * DamageMultiplier()));
                HitStop.Pause(0.08f);
            }
        }

        if (warning != null)
            Destroy(warning);
    }

    IEnumerator RunCharge()
    {
        if (player == null) yield break;

        telegraphing = true;
        if (navAgent != null)
            navAgent.ResetPath();

        chargeDirection = (player.transform.position - transform.position).normalized;
        chargeDirection.y = 0f;
        if (chargeDirection.sqrMagnitude < 0.01f)
            chargeDirection = transform.forward;

        yield return new WaitForSecondsRealtime(0.35f);
        telegraphing = false;

        charging = true;
        if (navAgent != null)
            navAgent.enabled = false;

        float elapsed = 0f;
        while (elapsed < chargeDuration)
        {
            transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;

            if (player != null &&
                Vector3.Distance(transform.position, player.transform.position) <= 1.8f)
            {
                player.TakeDamage(Mathf.RoundToInt(chargeDamage * DamageMultiplier()));
                HitStop.Pause(0.1f);
                break;
            }

            yield return null;
        }

        charging = false;
        if (navAgent != null)
            navAgent.enabled = true;
    }

    IEnumerator EnterNextPhase(int newPhase)
    {
        if (isDead || transitioning)
            yield break;

        transitioning = true;
        if (navAgent != null)
            navAgent.ResetPath();

        if (animator != null) animator.SetTrigger("Roar");

        CombatFX.ShowText(transform.position + Vector3.up * 2.5f,
            newPhase == 3 ? "ALPHA LOUP-TONNERRE ENRAGED!" : "The Pack Leader Calls the Storm!",
            new Color(0.9f, 0.4f, 0.1f));

        yield return new WaitForSecondsRealtime(phaseTransitionDelay);

        currentPhase = newPhase;

        ApplyTempestAura();

        GameObject burst = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        burst.transform.position = transform.position;
        Destroy(burst.GetComponent<Collider>());
        var burstFx = burst.AddComponent<RuneBurstFX>();
        burstFx.radius = 5f;
        var mr = burst.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(0.35f, 0.6f, 1f, 0.9f);
            mr.sharedMaterial = mat;
        }
        burst.transform.localScale = Vector3.one * 8f;

        transitioning = false;
    }

    void ApplyTempestAura()
    {
        if (tempestAura == null)
            return;

        tempestAura.SetActive(true);
        if (currentPhase >= 3)
            tempestAura.transform.localScale = Vector3.one * 1.5f;
    }

    float DamageMultiplier()
    {
        if (currentPhase >= 3) return 1.5f;
        if (currentPhase >= 2) return 1.25f;
        return 1f;
    }

    float AttackCooldownForPhase()
    {
        if (currentPhase >= 3) return attackCooldown * 0.55f;
        if (currentPhase >= 2) return attackCooldown * 0.75f;
        return attackCooldown;
    }

    void OnBossDefeated()
    {
        isDead = true;
        if (navAgent != null)
        {
            navAgent.ResetPath();
            navAgent.enabled = false;
        }

        CancelInvoke();
        if (animator != null) animator.SetTrigger("Die");

        if (player != null)
        {
            player.AddSpiritEssence(120);
            player.AddRage(40f);
        }

        CombatFX.ShowText(transform.position + Vector3.up * 2.5f,
            "The Alpha Loup-Tonnerre has fallen. The storm honors you.",
            new Color(1f, 0.85f, 0.4f));

        GameObject burst = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        burst.transform.position = transform.position;
        Destroy(burst.GetComponent<Collider>());

        GameObject.Destroy(gameObject, 3f);
    }
}