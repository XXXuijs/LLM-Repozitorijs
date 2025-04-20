using UnityEngine;

public class ShootingController : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;     // The projectile object to spawn (Assign Prefab in Inspector)
    [SerializeField] private Transform projectileSpawnPoint; // Where the projectile originates (Assign Transform in Inspector)
    // Optional: Add fire rate later if needed
    // [SerializeField] private float fireRate = 0.2f; // Time between shots
    // private float nextFireTime = 0f;

    private Camera playerCamera; // Reference to the player's camera for potential aiming logic

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        Debug.Log($"ShootingController [{gameObject.name}]: Awake - Initializing.");
        // Find the main camera in the scene - Ensure your player camera has the "MainCamera" tag!
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError($"ShootingController [{gameObject.name}]: Could not find Main Camera! Ensure camera has 'MainCamera' tag.");
        }
         else
        {
            Debug.Log($"ShootingController [{gameObject.name}]: Main Camera retrieved.");
        }

        // Error checking for required assignments
        if (projectilePrefab == null)
        {
            Debug.LogError($"ShootingController [{gameObject.name}]: Projectile Prefab is not assigned in the Inspector!");
        }
        if (projectileSpawnPoint == null)
        {
             Debug.LogError($"ShootingController [{gameObject.name}]: Projectile Spawn Point is not assigned in the Inspector!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log($"ShootingController [{gameObject.name}]: Update frame check."); // Can be spammy

        // Check for Left Mouse Button Click (Fire1 is the default mapping)
        // Use GetMouseButtonDown for single shot per click
        // Use GetMouseButton for automatic fire (combine with fire rate timer)
        if (Input.GetButtonDown("Fire1")) // "Fire1" is typically Left Ctrl/Left Mouse
        {
            Debug.Log($"ShootingController [{gameObject.name}]: Fire1 input detected!");
            Shoot();
        }

        // --- Optional: Automatic Fire Logic ---
        // if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        // {
        //     Debug.Log($"ShootingController [{gameObject.name}]: Fire1 input detected (Automatic Fire Check)!");
        //     nextFireTime = Time.time + fireRate; // Update next allowed fire time
        //     Shoot();
        // }
    }

    // Handles the spawning of the projectile
    void Shoot()
    {
        Debug.Log($"ShootingController [{gameObject.name}]: Shoot() method called.");

        // Double-check if references are valid before attempting to shoot
        if (projectilePrefab == null || projectileSpawnPoint == null || playerCamera == null)
        {
            Debug.LogError($"ShootingController [{gameObject.name}]: Cannot shoot because projectilePrefab, projectileSpawnPoint, or playerCamera is missing!");
            return;
        }

        Debug.Log($"ShootingController [{gameObject.name}]: Instantiating projectile '{projectilePrefab.name}' at position {projectileSpawnPoint.position.ToString("F3")} with rotation {projectileSpawnPoint.rotation.eulerAngles.ToString("F3")}.");

        // Instantiate the projectile prefab at the spawn point's position and rotation
        // Using the spawn point's rotation ensures the projectile initially faces the correct direction
        GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

        // The Projectile.cs script on the instance will handle applying velocity and lifetime automatically in its Start() method.

        Debug.Log($"ShootingController [{gameObject.name}]: Projectile '{projectileInstance.name}' instantiated successfully.");
    }
}