using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Debug.Log("Projectile.Start(): Projectile launched with velocity " + rb.linearVelocity);

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
    if (other.CompareTag("Enemy"))
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(25f); // Tune damage here
                Debug.Log("Projectile.OnTriggerEnter(): Hit enemy, applied damage.");
            }

            Destroy(gameObject);
        }
    }

}
