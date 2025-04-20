using UnityEngine;
using UnityEngine.AI; // Required for NavMeshAgent

[RequireComponent(typeof(NavMeshAgent))] // Ensure NavMeshAgent is present
[RequireComponent(typeof(Collider))]   // Ensure Collider is present for collisions
public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Movement & Detection")]
    [SerializeField] private float detectionRadius = 20f; // How far away the enemy can detect the player
    [SerializeField] private float attackRange = 2f;      // How close the enemy gets before stopping (stopping distance)
    [SerializeField] private string playerTag = "Player"; // Tag used to find the player GameObject

    private NavMeshAgent navAgent;
    private Transform playerTransform;
    private bool isPlayerDetected = false;
    private bool isDead = false; // Prevent actions after death

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        Debug.Log($"EnemyAI [{gameObject.name}]: Awake - Initializing.");
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError($"EnemyAI [{gameObject.name}]: NavMeshAgent component not found!");
            this.enabled = false; // Disable script if NavMeshAgent is missing
            return;
        }
         else
        {
             Debug.Log($"EnemyAI [{gameObject.name}]: NavMeshAgent component retrieved.");
        }

        // Attempt to find the player GameObject using the specified tag
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            Debug.Log($"EnemyAI [{gameObject.name}]: Found Player object '{playerObject.name}' with tag '{playerTag}'.");
        }
        else
        {
            Debug.LogError($"EnemyAI [{gameObject.name}]: Could not find GameObject with tag '{playerTag}'! Enemy will not follow.");
            // Optionally disable the script or AI behavior if player isn't found
            // this.enabled = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"EnemyAI [{gameObject.name}]: Start - Setting initial health and NavMeshAgent properties.");
        currentHealth = maxHealth;
        Debug.Log($"EnemyAI [{gameObject.name}]: Initial Health: {currentHealth}/{maxHealth}");

        if (navAgent != null)
        {
            navAgent.stoppingDistance = attackRange; // Set how close the agent stops from the destination
            Debug.Log($"EnemyAI [{gameObject.name}]: NavMeshAgent stopping distance set to: {navAgent.stoppingDistance}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Don't do anything if dead or player isn't found
        if (isDead || playerTransform == null || navAgent == null)
        {
            // Optional log if needed for debugging state issues
            // Debug.Log($"EnemyAI [{gameObject.name}]: Update skipped (isDead={isDead}, playerFound={playerTransform != null}, navAgentExists={navAgent != null}).");
            return;
        }

        // Debug.Log($"EnemyAI [{gameObject.name}]: Update frame check."); // Can be spammy

        // --- Detection and Following Logic ---
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        // Debug.Log($"EnemyAI [{gameObject.name}]: Distance to player: {distanceToPlayer.ToString("F2")}");

        if (distanceToPlayer <= detectionRadius)
        {
            if (!isPlayerDetected)
            {
                 Debug.Log($"EnemyAI [{gameObject.name}]: Player detected within {detectionRadius} units.");
                 isPlayerDetected = true;
            }
            // Player is detected, set destination to player's position
            navAgent.SetDestination(playerTransform.position);
            // Debug.Log($"EnemyAI [{gameObject.name}]: Setting destination to player position: {playerTransform.position.ToString("F3")}");

            // --- Optional: Attack Logic Placeholder ---
            if (distanceToPlayer <= navAgent.stoppingDistance) // Check if we are in attack range (close enough)
            {
                // Check if the agent actually reached the destination (or is very close)
                if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
                {
                     Debug.Log($"EnemyAI [{gameObject.name}]: Reached player within attack range ({attackRange} units). (Attack logic would go here)");
                     // Face the player while attacking (optional)
                     FaceTarget();
                     // TODO: Implement attack behavior (e.g., deal damage to player)
                }
            }
        }
        else
        {
            if (isPlayerDetected)
            {
                 Debug.Log($"EnemyAI [{gameObject.name}]: Player lost (out of {detectionRadius} units range).");
                 isPlayerDetected = false;
                 // Optional: Stop moving if player is lost
                 if (navAgent.hasPath)
                 {
                     navAgent.ResetPath(); // Clear the current path
                     Debug.Log($"EnemyAI [{gameObject.name}]: Path reset.");
                 }
            }
            // Optional: Implement idle or patrol behavior here when player is not detected
        }
    }

    // Rotates the enemy to face the player (call when attacking or engaging)
    void FaceTarget()
    {
        if(playerTransform == null) return;

        Vector3 direction = (playerTransform.position - transform.position).normalized;
        // We only want to rotate around the Y axis
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        // Slerp for smooth rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * navAgent.angularSpeed / 30); // Adjust divisor for rotation speed
        // Debug.Log($"EnemyAI [{gameObject.name}]: Facing player."); // Can be spammy
    }


    // Public method for projectiles (or other sources) to deal damage
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return; // Already dead, do nothing

        currentHealth -= damageAmount;
        Debug.Log($"EnemyAI [{gameObject.name}]: Took {damageAmount} damage. Current Health: {currentHealth}/{maxHealth}");

        // --- Health Check ---
        if (currentHealth <= 0)
        {
            Die();
        }
        // Optional: Add hit reaction logic here (e.g., play sound, brief stun)
    }

    // Handles the enemy's death
    private void Die()
    {
        isDead = true; // Set flag to stop other behaviors
        Debug.Log($"EnemyAI [{gameObject.name}]: Health reached zero. Dying.");

        // Disable components that interfere with death sequence (like movement)
        if (navAgent != null)
        {
            navAgent.enabled = false;
            Debug.Log($"EnemyAI [{gameObject.name}]: NavMeshAgent disabled.");
        }
        GetComponent<Collider>().enabled = false; // Prevent further collisions
        Debug.Log($"EnemyAI [{gameObject.name}]: Collider disabled.");

        // --- Death Effects Placeholder ---
        // TODO: Add death animation, sound effects, particle effects, loot drops etc.
        Debug.Log($"EnemyAI [{gameObject.name}]: Playing death effects (Placeholder).");


        // Destroy the GameObject after a short delay (e.g., to allow animation/effects to play)
        float deathDelay = 2.0f; // Adjust as needed
        Debug.Log($"EnemyAI [{gameObject.name}]: Scheduling GameObject destruction in {deathDelay} seconds.");
        Destroy(gameObject, deathDelay);
    }
}