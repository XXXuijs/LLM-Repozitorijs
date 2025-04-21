using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    // Enemy variables
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float detectionRange = 10f;

    // Components
    private NavMeshAgent agent;
    private Transform player;
    private float currentHealth;

    void Start()
    {
        InitializeEnemy();
    }

    void Update()
    {
        CheckPlayerProximity();
        FollowPlayer();
    }

    private void InitializeEnemy()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogWarning("Enemy: Player not found!");
        }
        Debug.Log($"Enemy: Initialized with health: {currentHealth}, speed: {moveSpeed}");
    }

    private void CheckPlayerProximity()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            bool playerDetected = distanceToPlayer <= detectionRange;
            Debug.Log($"Enemy: Distance to player: {distanceToPlayer}, Player detected: {playerDetected}");
        }
    }

    private void FollowPlayer()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            agent.SetDestination(player.position);
            Debug.Log($"Enemy: Following player to position: {player.position}");
        }
        else
        {
            agent.ResetPath();
            Debug.Log("Enemy: Player out of range, stopping movement.");
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"Enemy: Took {damage} damage, current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy: Enemy destroyed.");
        Destroy(gameObject);
    }
}