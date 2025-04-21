using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    // Enemy settings
    [Header("Enemy Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackRate = 1.5f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float losePlayerRadius = 25f;
    [SerializeField] private LayerMask playerLayer;
    
    // Visual feedback
    [Header("Visual Feedback")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material damagedMaterial;
    [SerializeField] private float flashDuration = 0.15f;
    
    // Nav mesh agent reference
    private NavMeshAgent agent;
    
    // Health and status tracking
    private float currentHealth;
    private bool isDead = false;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;
    
    // Player tracking
    private Transform playerTransform;
    private bool playerDetected = false;
    private Vector3 lastKnownPlayerPosition;
    
    // References to components
    private Renderer enemyRenderer;
    
    // Animation states
    private enum EnemyState { Idle, Chasing, Attacking, Searching, Dead }
    private EnemyState currentState = EnemyState.Idle;

    private void Start()
    {
        Debug.Log($"EnemyAI: Start function called on {gameObject.name}");
        
        // Get component references
        agent = GetComponent<NavMeshAgent>();
        enemyRenderer = GetComponentInChildren<Renderer>();
        
        // Check if NavMeshAgent exists
        if (agent == null)
        {
            Debug.LogError($"EnemyAI: NavMeshAgent component missing on {gameObject.name}");
            agent = gameObject.AddComponent<NavMeshAgent>();
            agent.speed = 3.5f;
            agent.acceleration = 8.0f;
            agent.angularSpeed = 120f;
        }
        
        // Set up collision
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider == null)
        {
            Debug.LogWarning($"EnemyAI: Collider missing on {gameObject.name}, adding CapsuleCollider");
            CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.5f;
            capsule.center = new Vector3(0, 1f, 0);
        }
        
        // Initialize health
        currentHealth = maxHealth;
        
        // Try to find the player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            Debug.Log($"EnemyAI: Found player at {playerTransform.position}");
        }
        else
        {
            Debug.LogWarning("EnemyAI: Player not found! Make sure player has 'Player' tag.");
        }
        
        // Start behavior routines
        StartCoroutine(DetectionRoutine());
        
        Debug.Log($"EnemyAI: Initialization complete on {gameObject.name}");
    }

    private void Update()
    {
        Debug.Log($"EnemyAI: Update function called on {gameObject.name}, State: {currentState}");
        
        if (isDead)
        {
            return;
        }
        
        // Update behavior based on current state
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
                
            case EnemyState.Chasing:
                HandleChasingState();
                break;
                
            case EnemyState.Attacking:
                HandleAttackingState();
                break;
                
            case EnemyState.Searching:
                HandleSearchingState();
                break;
        }
    }
    
    private void HandleIdleState()
    {
        Debug.Log("EnemyAI: HandleIdleState function called");
        
        // In idle state, we just wait for player detection
        if (playerDetected)
        {
            Debug.Log("EnemyAI: Transitioning from Idle to Chasing state");
            ChangeState(EnemyState.Chasing);
        }
    }
    
    private void HandleChasingState()
    {
        Debug.Log("EnemyAI: HandleChasingState function called");
        
        if (playerTransform == null || !playerDetected)
        {
            Debug.Log("EnemyAI: Lost player, transitioning to Searching state");
            lastKnownPlayerPosition = playerTransform != null ? playerTransform.position : transform.position;
            ChangeState(EnemyState.Searching);
            return;
        }
        
        // Update destination to player position
        agent.SetDestination(playerTransform.position);
        lastKnownPlayerPosition = playerTransform.position;
        
        // Check if within attack range
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= attackRange)
        {
            Debug.Log($"EnemyAI: Player in attack range ({distanceToPlayer} <= {attackRange}), transitioning to Attacking state");
            ChangeState(EnemyState.Attacking);
        }
    }
    
    private void HandleAttackingState()
    {
        Debug.Log("EnemyAI: HandleAttackingState function called");
        
        if (playerTransform == null || !playerDetected)
        {
            Debug.Log("EnemyAI: Lost player during attack, transitioning to Searching state");
            ChangeState(EnemyState.Searching);
            return;
        }
        
        // Face the player
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        
        // Stop moving
        agent.SetDestination(transform.position);
        
        // Check if still in attack range
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > attackRange * 1.1f)
        {
            Debug.Log($"EnemyAI: Player left attack range ({distanceToPlayer} > {attackRange}), transitioning to Chasing state");
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // Perform attack if cooldown is done
        if (Time.time >= nextAttackTime && !isAttacking)
        {
            StartCoroutine(AttackPlayer());
        }
    }
    
    private void HandleSearchingState()
    {
        Debug.Log("EnemyAI: HandleSearchingState function called");
        
        // If player is detected again, resume chase
        if (playerDetected)
        {
            Debug.Log("EnemyAI: Found player during search, transitioning to Chasing state");
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // Check if we've reached the last known position
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            Debug.Log("EnemyAI: Reached last known player position, returning to Idle state");
            ChangeState(EnemyState.Idle);
        }
    }
    
    private void ChangeState(EnemyState newState)
    {
        Debug.Log($"EnemyAI: ChangeState called - {currentState} -> {newState}");
        currentState = newState;
        
        // Handle state entry actions
        switch (newState)
        {
            case EnemyState.Idle:
                agent.speed = 2f;
                agent.isStopped = true;
                break;
                
            case EnemyState.Chasing:
                agent.speed = 3.5f;
                agent.isStopped = false;
                break;
                
            case EnemyState.Attacking:
                agent.isStopped = true;
                break;
                
            case EnemyState.Searching:
                agent.speed = 2.5f;
                agent.isStopped = false;
                agent.SetDestination(lastKnownPlayerPosition);
                break;
        }
    }
    
    private IEnumerator DetectionRoutine()
    {
        Debug.Log("EnemyAI: DetectionRoutine coroutine started");
        
        while (!isDead)
        {
            yield return new WaitForSeconds(0.2f); // Check every 0.2 seconds
            
            if (playerTransform == null)
            {
                // Try to find player again
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
                else
                {
                    continue;
                }
            }
            
            // Check distance to player
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            // Update detection status
            if (distanceToPlayer <= detectionRadius)
            {
                // Check if there's a clear line of sight
                if (HasLineOfSightToPlayer())
                {
                    if (!playerDetected)
                    {
                        Debug.Log($"EnemyAI: Player detected at distance {distanceToPlayer}");
                    }
                    playerDetected = true;
                }
            }
            else if (distanceToPlayer > losePlayerRadius)
            {
                if (playerDetected)
                {
                    Debug.Log($"EnemyAI: Player lost at distance {distanceToPlayer}");
                }
                playerDetected = false;
            }
        }
    }
    
    private bool HasLineOfSightToPlayer()
    {
        Debug.Log("EnemyAI: HasLineOfSightToPlayer function called");
        
        if (playerTransform == null)
            return false;
        
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        
        // Cast a ray towards the player
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, out RaycastHit hit, distanceToPlayer))
        {
            // Check if we hit the player
            if (hit.transform == playerTransform || hit.transform.CompareTag("Player"))
            {
                Debug.Log("EnemyAI: Has line of sight to player");
                return true;
            }
            
            Debug.Log($"EnemyAI: Line of sight blocked by {hit.transform.name}");
            return false;
        }
        
        // If ray didn't hit anything, we have line of sight
        return true;
    }
    
    private IEnumerator AttackPlayer()
    {
        Debug.Log("EnemyAI: AttackPlayer coroutine started");
        
        isAttacking = true;
        nextAttackTime = Time.time + attackRate;
        
        // Attack animation would play here
        
        // Wait a bit before applying damage (animation timing)
        yield return new WaitForSeconds(0.5f);
        
        // Check if player is still in range and we have line of sight
        if (playerTransform != null && 
            Vector3.Distance(transform.position, playerTransform.position) <= attackRange &&
            HasLineOfSightToPlayer())
        {
            // Apply damage to player
            FirstPersonController playerController = playerTransform.GetComponent<FirstPersonController>();
            if (playerController != null && playerController.TryGetComponent(out Health playerHealth))
            {
                Debug.Log($"EnemyAI: Attacking player for {attackDamage} damage");
                playerHealth.TakeDamage(attackDamage);
            }
            else
            {
                Debug.LogWarning("EnemyAI: Player has no Health component to damage");
            }
        }
        
        // Attack cooldown
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }
    
    public void TakeDamage(float damage, Vector3 hitPoint)
    {
        Debug.Log($"EnemyAI: TakeDamage function called - Amount: {damage}, Hit point: {hitPoint}");
        
        if (isDead)
            return;
        
        currentHealth -= damage;
        
        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
            Destroy(hitEffect, 2f);
        }
        
        // Flash damage material
        StartCoroutine(FlashDamage());
        
        // Alert to player presence
        if (!playerDetected && playerTransform != null)
        {
            playerDetected = true;
            lastKnownPlayerPosition = playerTransform.position;
            
            if (currentState == EnemyState.Idle || currentState == EnemyState.Searching)
            {
                ChangeState(EnemyState.Chasing);
            }
        }
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private IEnumerator FlashDamage()
    {
        Debug.Log("EnemyAI: FlashDamage coroutine started");
        
        if (enemyRenderer != null && damagedMaterial != null)
        {
            Material originalMaterial = enemyRenderer.material;
            enemyRenderer.material = damagedMaterial;
            yield return new WaitForSeconds(flashDuration);
            enemyRenderer.material = originalMaterial;
        }
        else
        {
            Debug.LogWarning("EnemyAI: Missing renderer or damage material");
        }
    }
    
    private void Die()
    {
        Debug.Log($"EnemyAI: Die function called on {gameObject.name}");
        
        isDead = true;
        ChangeState(EnemyState.Dead);
        
        // Disable components
        if (agent != null)
            agent.enabled = false;
        
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
            enemyCollider.enabled = false;
        
        // Spawn death effect
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 5f);
        }
        
        // Death animation would play here
        // For now, we'll just disable the enemy after a delay
        StartCoroutine(DestroyAfterDelay(3f));
    }
    
    private IEnumerator DestroyAfterDelay(float delay)
    {
        Debug.Log($"EnemyAI: DestroyAfterDelay coroutine started, delay: {delay}s");
        
        // Could play death animation here
        yield return new WaitForSeconds(delay);
        
        Destroy(gameObject);
    }
    
    // Public method to manually set the player target
    public void SetPlayerTarget(Transform player)
    {
        Debug.Log($"EnemyAI: SetPlayerTarget called with {(player ? player.name : "null")}");
        playerTransform = player;
    }
    
    // Called when enemy is hit by a projectile
    public void OnProjectileHit(Projectile projectile, Vector3 hitPoint)
    {
        Debug.Log($"EnemyAI: OnProjectileHit function called from {projectile.gameObject.name}");
        TakeDamage(projectile.damage, hitPoint);
    }
    
    // For debugging visualization
    private void OnDrawGizmosSelected()
    {
        // Detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Lose player radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, losePlayerRadius);
        
        // Attack range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}