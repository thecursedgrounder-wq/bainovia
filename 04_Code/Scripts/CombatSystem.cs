using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Combat System - Manages combat mechanics, hit detection, and damage calculations
/// </summary>
public class CombatSystem : MonoBehaviour
{
    [Header("Combat Settings")]
    public float lightAttackDamage = 25f;
    public float heavyAttackDamage = 50f;
    public float attackRange = 2.5f;
    public float attackCooldown = 1.2f;
    public LayerMask enemyLayers;

    [Header("Finisher Settings")]
    public float finisherRange = 1.5f;
    public float finisherDamage = 100f;
    public float finisherDuration = 2f;

    [Header("Parry Settings")]
    public float parryWindow = 0.3f;
    public float parryDamageMultiplier = 2f;
    public float parryStunDuration = 1f;

    [Header("References")]
    public Transform attackPoint;
    public BainoviaCharacterController characterController;
    public GameObject finisherVFX;

    private float attackTimer;
    private bool isParrying;
    private bool canFinisher;
    private List<GameObject> nearbyEnemies = new List<GameObject>();

    void Update()
    {
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        DetectNearbyEnemies();
        CheckFinisherOpportunity();
    }

    void DetectNearbyEnemies()
    {
        nearbyEnemies.Clear();
        Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayers);
        foreach (Collider enemy in enemies)
        {
            nearbyEnemies.Add(enemy.gameObject);
        }
    }

    void CheckFinisherOpportunity()
    {
        canFinisher = false;
        foreach (GameObject enemy in nearbyEnemies)
        {
            Health health = enemy.GetComponent<Health>();
            if (health != null && health.GetHealthPercent() <= 0.25f)
            {
                canFinisher = true;
                break;
            }
        }
    }

    public void PerformLightAttack()
    {
        if (attackTimer > 0) return;

        attackTimer = attackCooldown;
        DealDamage(lightAttackDamage, attackRange);
    }

    public void PerformHeavyAttack()
    {
        if (attackTimer > 0) return;

        attackTimer = attackCooldown * 1.5f;
        DealDamage(heavyAttackDamage, attackRange * 1.2f);
        ApplyKnockback(attackRange * 1.2f, 10f);
    }

    public void PerformFinisher(GameObject target)
    {
        if (!canFinisher || target == null) return;

        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth == null || targetHealth.GetHealthPercent() > 0.25f) return;

        // Play finisher animation
        // Deal massive damage
        targetHealth.TakeDamage(Mathf.RoundToInt(finisherDamage));

        // Spawn VFX
        if (finisherVFX != null)
            Instantiate(finisherVFX, target.transform.position, Quaternion.identity);

        // Heal player
        characterController?.Heal(50);
        characterController?.AddSpiritEssence(30);

        Debug.Log($"Finisher performed on {target.name}");
    }

    public void DealDamage(float damage, float range)
    {
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, range, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(Mathf.RoundToInt(damage));
            }
        }
    }

    public void ApplyKnockback(float range, float force)
    {
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

    public bool TryParry()
    {
        if (isParrying) return false;

        isParrying = true;
        Invoke(nameof(ResetParry), parryWindow);
        return true;
    }

    public void OnParrySuccess(GameObject attacker)
    {
        // Deal counter damage
        Health attackerHealth = attacker.GetComponent<Health>();
        if (attackerHealth != null)
        {
            float counterDamage = lightAttackDamage * parryDamageMultiplier;
            attackerHealth.TakeDamage(Mathf.RoundToInt(counterDamage));
        }

        // Stun attacker
        EnemyAI enemyAI = attacker.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.Stun(parryStunDuration);
        }

        Debug.Log($"Parry successful against {attacker.name}");
    }

    void ResetParry()
    {
        isParrying = false;
    }

    public bool CanFinisher()
    {
        return canFinisher;
    }

    public List<GameObject> GetNearbyEnemies()
    {
        return nearbyEnemies;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, finisherRange);
    }
}
