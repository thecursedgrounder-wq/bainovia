using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Enemy AI - Base AI controller for all enemy types
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float detectionRange = 20f;
    public float attackRange = 2.5f;
    public float chaseSpeed = 5f;
    public float patrolSpeed = 2f;
    public float attackCooldown = 2f;

    [Header("Combat Settings")]
    public int damage = 20;
    public LayerMask playerLayer;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public int currentPatrolIndex;

    [Header("References")]
    public Health health;
    public BainoviaCharacterController player;
    public Animator animator;

    private NavMeshAgent navAgent;
    private enum AIState { Idle, Patrol, Chase, Attack, Stunned, Flee }
    private AIState currentState;
    private float attackTimer;
    private float stunTimer;
    private bool isDead;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (health == null) health = GetComponent<Health>();
        if (animator == null) animator = GetComponent<Animator>();

        if (player == null)
            player = FindObjectOfType<BainoviaCharacterController>();

        currentState = AIState.Patrol;
        navAgent.speed = patrolSpeed;
    }

    void Update()
    {
        if (isDead) return;

        if (health != null && health.IsDead())
        {
            Die();
            return;
        }

        // Do not chase/attack a dead player (it would otherwise be a forever-loop).
        if (player != null && player.IsDead())
        {
            if (navAgent != null && navAgent.hasPath)
                navAgent.ResetPath();
            if (animator != null)
            {
                animator.SetBool("IsRunning", false);
                animator.SetBool("IsAttacking", false);
            }
            return;
        }

        HandleStun();
        UpdateAIState();
    }

    void HandleStun()
    {
        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                currentState = AIState.Chase;
            }
        }
    }

    void UpdateAIState()
    {
        switch (currentState)
        {
            case AIState.Idle:
                HandleIdle();
                break;
            case AIState.Patrol:
                HandlePatrol();
                break;
            case AIState.Chase:
                HandleChase();
                break;
            case AIState.Attack:
                HandleAttack();
                break;
            case AIState.Stunned:
                HandleStunned();
                break;
            case AIState.Flee:
                HandleFlee();
                break;
        }
    }

    void HandleIdle()
    {
        // Check for player
        if (DetectPlayer())
        {
            currentState = AIState.Chase;
            return;
        }

        // Transition to patrol after delay
        if (Random.value < 0.01f)
        {
            currentState = AIState.Patrol;
        }
    }

    void HandlePatrol()
    {
        if (patrolPoints.Length == 0)
        {
            currentState = AIState.Idle;
            return;
        }

        // Check for player
        if (DetectPlayer())
        {
            currentState = AIState.Chase;
            navAgent.speed = chaseSpeed;
            return;
        }

        // Move to patrol point
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        navAgent.SetDestination(targetPoint.position);

        if (navAgent.remainingDistance < 1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }

        // Update animator
        if (animator != null)
        {
            animator.SetBool("IsWalking", navAgent.velocity.magnitude > 0.1f);
        }
    }

    void HandleChase()
    {
        if (player == null)
        {
            currentState = AIState.Patrol;
            navAgent.speed = patrolSpeed;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // Check if player is too far
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            currentState = AIState.Patrol;
            navAgent.speed = patrolSpeed;
            return;
        }

        // Check if in attack range
        if (distanceToPlayer <= attackRange)
        {
            currentState = AIState.Attack;
            return;
        }

        // Chase player
        navAgent.SetDestination(player.transform.position);
        navAgent.speed = chaseSpeed;

        // Update animator
        if (animator != null)
        {
            animator.SetBool("IsRunning", true);
        }
    }

    void HandleAttack()
    {
        if (player == null)
        {
            currentState = AIState.Patrol;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // Check if player is out of range
        if (distanceToPlayer > attackRange * 1.2f)
        {
            currentState = AIState.Chase;
            if (animator != null)
            {
                animator.SetBool("IsRunning", true);
                animator.SetBool("IsAttacking", false);
            }
            return;
        }

        // Face player
        Vector3 lookDirection = (player.transform.position - transform.position).normalized;
        lookDirection.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 10f);

        // Attack
        if (attackTimer <= 0)
        {
            PerformAttack();
            attackTimer = attackCooldown;
        }

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        // Update animator
        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
            animator.SetBool("IsRunning", false);
        }
    }

    void HandleStunned()
    {
        // Wait for stun to end
        if (stunTimer <= 0)
        {
            currentState = AIState.Chase;
        }
    }

    void HandleFlee()
    {
        if (player == null)
        {
            currentState = AIState.Patrol;
            return;
        }

        // Run away from player
        Vector3 fleeDirection = (transform.position - player.transform.position).normalized;
        Vector3 fleePosition = transform.position + fleeDirection * 10f;
        navAgent.SetDestination(fleePosition);
        navAgent.speed = chaseSpeed * 1.2f;

        // Check if far enough
        if (Vector3.Distance(transform.position, player.transform.position) > detectionRange)
        {
            currentState = AIState.Patrol;
            navAgent.speed = patrolSpeed;
        }
    }

    void PerformAttack()
    {
        if (player == null) return;

        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Deal damage
        player.TakeDamage(damage);

        Debug.Log($"{gameObject.name} attacked player for {damage} damage");
    }

    bool DetectPlayer()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        return distanceToPlayer <= detectionRange;
    }

    public void Stun(float duration)
    {
        stunTimer = duration;
        currentState = AIState.Stunned;

        if (animator != null)
        {
            animator.SetBool("IsStunned", true);
        }
    }

    public void Flee()
    {
        currentState = AIState.Flee;
    }

    void Die()
    {
        isDead = true;
        navAgent.enabled = false;

        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }

        // Disable this script
        this.enabled = false;

        Debug.Log($"{gameObject.name} has died");
    }

    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
