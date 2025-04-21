using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private GameObject damageEffectPrefab;
    [SerializeField] private GameObject deathEffectPrefab;
    
    // Events
    public UnityEvent OnDeath;
    public UnityEvent<float> OnDamage;
    public UnityEvent<float> OnHeal;
    
    private float currentHealth;
    
    private void Start()
    {
        Debug.Log($"Health: Start function called on {gameObject.name}");
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(float damage)
    {
        Debug.Log($"Health: TakeDamage function called - Amount: {damage}");
        
        if (damage <= 0) return;
        
        currentHealth -= damage;
        
        // Invoke damage event
        OnDamage?.Invoke(damage);
        
        // Spawn damage effect
        if (damageEffectPrefab != null)
        {
            GameObject effect = Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        Debug.Log($"Health: Heal function called - Amount: {amount}");
        
        if (amount <= 0) return;
        
        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        // Only invoke if healing actually occurred
        if (currentHealth > previousHealth)
        {
            OnHeal?.Invoke(currentHealth - previousHealth);
        }
    }
    
    private void Die()
    {
        Debug.Log($"Health: Die function called on {gameObject.name}");
        
        // Spawn death effect if available
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
        
        // Invoke death event
        OnDeath?.Invoke();
        
        // If this is not attached to an enemy (which handles its own death), destroy the object
        if (GetComponent<EnemyAI>() == null)
        {
            Destroy(gameObject);
        }
    }
    
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}