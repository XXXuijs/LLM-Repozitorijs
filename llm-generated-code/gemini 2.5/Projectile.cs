using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 30f;
    [SerializeField] private float lifetime = 5.0f;
    [SerializeField] public int damage = 25; // <--- ADDED DAMAGE VALUE

    private Rigidbody rb;

    void Awake()
    {
        Debug.Log($"Projectile [{gameObject.name}]: Awake - Initializing.");
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError($"Projectile [{gameObject.name}]: Rigidbody component not found!");
        }
        else
        {
            Debug.Log($"Projectile [{gameObject.name}]: Rigidbody component retrieved.");
            // Consider Rigidbody settings (Use Gravity, Collision Detection)
        }
    }

    void Start()
    {
        Debug.Log($"Projectile [{gameObject.name}]: Start - Applying initial velocity and setting lifetime.");
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
            Debug.Log($"Projectile [{gameObject.name}]: Velocity set to {rb.linearVelocity.ToString("F3")} (Speed: {speed}).");
        }
        Destroy(gameObject, lifetime);
        Debug.Log($"Projectile [{gameObject.name}]: Scheduled destruction in {lifetime} seconds.");
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Projectile [{gameObject.name}]: Collision detected with {collision.gameObject.name} (Tag: {collision.gameObject.tag}).");

        // --- Check if we hit an Enemy ---
        // Try to get the EnemyAI component from the collided object
        EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();

        if (enemy != null)
        {
            // We hit an object with the EnemyAI script
            Debug.Log($"Projectile [{gameObject.name}]: Hit an Enemy ({collision.gameObject.name})! Applying {damage} damage.");
            enemy.TakeDamage(damage); // Tell the enemy to take damage
        }
        else
        {
            // We hit something else (environment, etc.)
            Debug.Log($"Projectile [{gameObject.name}]: Collision with non-enemy object {collision.gameObject.name}.");
        }

        // Destroy the projectile after any collision
        Debug.Log($"Projectile [{gameObject.name}]: Destroying self due to collision.");
        Destroy(gameObject);
    }
}