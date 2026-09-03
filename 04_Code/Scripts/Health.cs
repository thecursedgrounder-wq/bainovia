using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Health Component - Manages health, damage, and death for any character
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool destroyOnDeath = true;
    public float destroyDelay = 2f;

    [Header("Regeneration")]
    public bool canRegenerate = false;
    public float regenerationRate = 1f;
    public float regenerationDelay = 5f;
    public float regenerationTimer;

    [Header("Events")]
    public UnityEvent<int> onHealthChanged;
    public UnityEvent onDeath;
    public UnityEvent<int> onDamageTaken;

    private bool isDead;

    void Start()
    {
        currentHealth = maxHealth;
        regenerationTimer = regenerationDelay;
    }

    void Update()
    {
        HandleRegeneration();
    }

    void HandleRegeneration()
    {
        if (!canRegenerate || isDead) return;

        if (currentHealth < maxHealth)
        {
            regenerationTimer -= Time.deltaTime;
            
            if (regenerationTimer <= 0)
            {
                currentHealth = Mathf.Min(currentHealth + Mathf.RoundToInt(regenerationRate), maxHealth);
                onHealthChanged?.Invoke(currentHealth);
            }
        }
        else
        {
            regenerationTimer = regenerationDelay;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        onHealthChanged?.Invoke(currentHealth);
        onDamageTaken?.Invoke(damage);

        regenerationTimer = regenerationDelay;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHealthChanged?.Invoke(currentHealth);
    }

    public void SetHealth(int health)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        onHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        onDeath?.Invoke();

        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    public void Revive(int healthAmount)
    {
        isDead = false;
        currentHealth = Mathf.Min(healthAmount, maxHealth);
        onHealthChanged?.Invoke(currentHealth);
    }

    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    void OnDrawGizmosSelected()
    {
        // Visual indicator for health status
        Gizmos.color = isDead ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
