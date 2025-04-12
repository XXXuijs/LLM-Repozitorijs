using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float patrolWaitTime = 3f;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Combat Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private GameObject deathEffect;

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private int currentHealth;
    private float lastAttackTime;
    private int currentPatrolIndex;
    private float waitTimer;
    private bool isWaiting;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        Debug.Log($"[EnemyAI] Enemy spawned with {currentHealth}/{maxHealth} health");
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer(distanceToPlayer);
        }
        else
        {
            Patrol();
        }

        UpdateAnimator();
    }

    private void ChasePlayer(float distance)
    {
        agent.speed = chaseSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(player.position);

        // Attack if in range
        if (distance <= stoppingDistance && Time.time > lastAttackTime + attackCooldown)
        {
            AttackPlayer();
            lastAttackTime = Time.time;
        }

        Debug.DrawLine(transform.position, player.position, Color.red);
    }

    private void Patrol()
    {
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0f;

        if (patrolPoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = Time.time + patrolWaitTime;
                Debug.Log($"[EnemyAI] Reached patrol point {currentPatrolIndex}, waiting...");
            }
            else if (Time.time >= waitTimer)
            {
                isWaiting = false;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                Debug.Log($"[EnemyAI] Moving to next patrol point {currentPatrolIndex}");
            }
        }
    }

    private void AttackPlayer()
    {
        // Raycast to ensure we have line of sight
        RaycastHit hit;
        if (Physics.Raycast(transform.position, (player.position - transform.position).normalized, out hit, stoppingDistance + 1f))
        {
            if (hit.transform == player)
            {
                FirstPersonShooter playerHealth = player.GetComponent<FirstPersonShooter>();
                if (playerHealth != null)
                {
                    // Player damage logic would go here if we implement player health
                    Debug.Log($"[EnemyAI] Attacked player for {attackDamage} damage");
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"[EnemyAI] Took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Optional: Add hurt reaction
            if (animator != null) animator.SetTrigger("Hurt");
        }
    }

    private void Die()
    {
        Debug.Log("[EnemyAI] Enemy died");
        
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Disable enemy
        agent.enabled = false;
        this.enabled = false;
        
        // Optional: Add death animation
        if (animator != null) animator.SetTrigger("Die");

        // Destroy after delay
        Destroy(gameObject, 2f);
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        
        animator.SetFloat("Speed", agent.velocity.magnitude);
        animator.SetBool("IsAttacking", Time.time < lastAttackTime + 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}