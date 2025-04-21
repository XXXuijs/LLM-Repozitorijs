using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int maxEnemiesAlive = 5;
    [SerializeField] private float minSpawnDelay = 2f;
    [SerializeField] private float maxSpawnDelay = 5f;
    [SerializeField] private bool autoStart = true;
    
    [Header("Target Settings")]
    [SerializeField] private Transform playerTarget;
    
    // Internal variables
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool isSpawning = false;
    
    private void Start()
    {
        Debug.Log("EnemySpawner: Start function called");
        
        // Check for valid spawn points
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: No spawn points defined");
            
            // Create a default spawn point if none are specified
            spawnPoints = new Transform[1];
            GameObject defaultPoint = new GameObject("Default Spawn Point");
            defaultPoint.transform.position = transform.position + new Vector3(0, 0, 5);
            defaultPoint.transform.parent = transform;
            spawnPoints[0] = defaultPoint.transform;
        }
        
        // Check for enemy prefab
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: Enemy prefab is not assigned!");
            return;
        }
        
        // Find player if not assigned
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
                Debug.Log("EnemySpawner: Found player automatically");
            }
            else
            {
                Debug.LogWarning("EnemySpawner: Player not found! Enemies won't have a target.");
            }
        }
        
        // Start spawning if autoStart is true
        if (autoStart)
        {
            StartSpawning();
        }
    }
    
    private void Update()
    {
        Debug.Log("EnemySpawner: Update function called");
        
        // Clean up destroyed enemies from our list
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
        }
    }
    
    public void StartSpawning()
    {
        Debug.Log("EnemySpawner: StartSpawning function called");
        
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
        }
    }
    
    public void StopSpawning()
    {
        Debug.Log("EnemySpawner: StopSpawning function called");
        isSpawning = false;
    }
    
    private IEnumerator SpawnRoutine()
    {
        Debug.Log("EnemySpawner: SpawnRoutine coroutine started");
        
        while (isSpawning)
        {
            // Only spawn if we haven't reached the maximum
            if (spawnedEnemies.Count < maxEnemiesAlive)
            {
                SpawnEnemy();
            }
            
            // Wait for next spawn time
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);
        }
    }
    
    private void SpawnEnemy()
    {
        Debug.Log("EnemySpawner: SpawnEnemy function called");
        
        // Choose a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // Instantiate the enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedEnemies.Add(enemy);
        
        // Set its target to the player
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        if (enemyAI != null && playerTarget != null)
        {
            enemyAI.SetPlayerTarget(playerTarget);
        }
        
        Debug.Log($"EnemySpawner: Enemy spawned at {spawnPoint.position}");
    }
    
    // For debugging visualization
    private void OnDrawGizmosSelected()
    {
        // Draw spawn points
        if (spawnPoints != null)
        {
            Gizmos.color = Color.red;
            
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 1f);
                    Gizmos.DrawLine(transform.position, point.position);
                }
            }
        }
    }
}