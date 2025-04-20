using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public float health = 50f;
    public float moveSpeed = 3.5f;

    private Transform player;
    private NavMeshAgent agent;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        Debug.Log("EnemyAI.Start(): Enemy initialized. Following player.");
    }

    void Update()
    {
        if (player)
        {
            agent.SetDestination(player.position);
            Debug.Log("EnemyAI.Update(): Moving towards player at " + player.position);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log("EnemyAI.TakeDamage(): Enemy took " + damage + " damage. Remaining: " + health);

        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("EnemyAI.Die(): Enemy died.");
        Destroy(gameObject);
    }
}
