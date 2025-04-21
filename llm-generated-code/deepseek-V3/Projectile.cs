using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private GameObject impactEffect;
    
    private Rigidbody rb;
    private int damage;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifetime);
        Debug.Log("[Projectile] Projectile spawned");
    }

    public void Initialize(Vector3 velocity, int projectileDamage)
    {
        rb.linearVelocity = velocity;
        damage = projectileDamage;
        Debug.Log($"[Projectile] Initialized with velocity: {velocity.magnitude}, damage: {damage}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        hasHit = true;

        // Check if we hit an enemy
        EnemyAI enemy = other.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"[Projectile] Hit enemy for {damage} damage");
        }

        // Instantiate impact effect
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        Debug.Log($"[Projectile] Collided with {other.gameObject.name}");
        Destroy(gameObject);
    }
}