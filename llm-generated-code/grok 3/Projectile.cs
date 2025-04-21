using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Projectile variables
    [SerializeField] private float damage = 25f;

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);
    }

    private void HandleCollision(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"Projectile: Hit enemy, dealt {damage} damage.");
            Destroy(gameObject);
            Debug.Log("Projectile: Projectile destroyed on enemy impact.");
        }
        else if (!other.CompareTag("Player"))
        {
            Debug.Log("Projectile: Hit non-enemy object, destroying projectile.");
            Destroy(gameObject);
        }
    }
}