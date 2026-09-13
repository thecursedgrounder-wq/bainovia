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
    public CameraController cameraController;

    private float attackTimer;
    private bool isParrying;

    void Update()
    {
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
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
        if (target == null) return;

        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth == null || targetHealth.GetHealthPercent() > 0.25f) return;

        // Play finisher animation
        // Deal massive damage
        targetHealth.TakeDamage(Mathf.RoundToInt(finisherDamage));
        CombatFX.ShowDamage(target.transform.position + Vector3.up * 0.8f, Mathf.RoundToInt(finisherDamage), new Color(1f, 0.85f, 0.3f));

        // Spawn VFX
        if (finisherVFX != null)
            Instantiate(finisherVFX, target.transform.position, Quaternion.identity);

        // Heal player
        characterController?.Heal(50);
        characterController?.AddSpiritEssence(30);

        // Face the victim and shake the camera
        if (characterController != null)
        {
            Vector3 dir = target.transform.position - characterController.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                characterController.transform.rotation = Quaternion.LookRotation(dir);
        }

        if (cameraController != null)
            cameraController.TriggerCombatShake();

        Debug.Log($"Finisher performed on {target.name}");
    }

    /// <summary>
    /// Performs a finisher on the nearest enemy within finisherRange at or below 25% HP.
    /// </summary>
    public bool PerformFinisherOnNearest()
    {
        Collider[] candidates = Physics.OverlapSphere(transform.position, finisherRange, enemyLayers);

        GameObject best = null;
        float bestSqr = float.MaxValue;

        foreach (Collider candidate in candidates)
        {
            Health h = candidate.GetComponent<Health>();
            if (h == null || h.IsDead() || h.GetHealthPercent() > 0.25f)
                continue;

            float sqr = (candidate.transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = candidate.gameObject;
            }
        }

        if (best == null)
            return false;

        PerformFinisher(best);
        return true;
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
