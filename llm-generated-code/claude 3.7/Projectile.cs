using UnityEngine;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 5f;
    public float explosionRadius = 0f; // 0 means no explosion
    private bool hasHit = false;
    
    private void Start()
    {
        Debug.Log($"Projectile: Start function called on {gameObject.name}");
        Destroy(gameObject, lifeTime);
    }
    
    public void Initialize(Vector3 initialVelocity)
    {
        Debug.Log($"Projectile: Initialize function called with velocity {initialVelocity}");
        
        // Optional: Set trail or particle effects here
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Projectile: OnCollisionEnter function called - Collided with {collision.gameObject.name}");
        
        if (hasHit) return;
        hasHit = true;
        
        // Check for hit target with health component
        if (collision.gameObject.TryGetComponent(out Health health))
        {
            Debug.Log($"Projectile: Hit object with Health component");
            health.TakeDamage(damage);
        }
        
        // Handle explosion if radius > 0
        if (explosionRadius > 0)
        {
            Explode();
        }
        
        // Destroy the projectile on impact
        Destroy(gameObject);
    }
    
    private void Explode()
    {
        Debug.Log($"Projectile: Explode function called - Radius: {explosionRadius}");
        
        // Find all colliders in explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider collider in colliders)
        {
            // Apply damage to health components
            if (collider.TryGetComponent(out Health health))
            {
                // Calculate damage based on distance
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                float damageMultiplier = 1 - (distance / explosionRadius);
                float explosionDamage = damage * damageMultiplier;
                
                health.TakeDamage(explosionDamage);
                Debug.Log($"Projectile: Explosion damaged {collider.name} for {explosionDamage}");
            }
            
            // Apply force to rigidbodies
            if (collider.TryGetComponent(out Rigidbody rb))
            {
                rb.AddExplosionForce(damage * 10, transform.position, explosionRadius);
                Debug.Log($"Projectile: Explosion applied force to {collider.name}");
            }
        }
        
        // Optional: Play explosion effect here
    }
}