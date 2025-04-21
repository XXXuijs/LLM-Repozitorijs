using UnityEngine;
using System.Collections.Generic;

public class FirstPersonController : MonoBehaviour
{
    // Movement parameters
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform groundCheck;

    // Wall jump parameters
    [Header("Wall Jump Settings")]
    [SerializeField] private float wallJumpForce = 10f;
    [SerializeField] private float wallJumpUpwardForce = 5f;
    [SerializeField] private float wallJumpCooldown = 0.5f;
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float wallSlidingSpeed = 2f;

    // Camera control
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private Transform playerCamera;

    // Weapon & Shooting parameters
    [Header("Weapon Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootForce = 30f;
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private float reloadTime = 1.5f;
    [SerializeField] private bool infiniteAmmo = false;

    // Internal variables
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;
    
    // Wall jump variables
    private bool isTouchingWall;
    private bool isWallJumping;
    private float wallJumpTimer;
    private Vector3 wallNormal;
    private Vector3 lastWallNormal;
    
    // Weapon variables
    private float nextFireTime = 0f;
    private int currentAmmo;
    private bool isReloading = false;
    private float reloadTimer = 0f;
    private List<GameObject> spawnedProjectiles = new List<GameObject>();

    private void Start()
    {
        Debug.Log("FirstPersonController: Start function called");
        
        // Get the CharacterController component
        controller = GetComponent<CharacterController>();
        
        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        
        // Check if camera reference is set
        if (playerCamera == null)
        {
            Debug.LogError("FirstPersonController: Player camera reference is not set!");
            playerCamera = Camera.main.transform;
        }
        
        // Check if ground check is set
        if (groundCheck == null)
        {
            Debug.LogWarning("FirstPersonController: Ground check transform is not set! Creating one.");
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.parent = transform;
            groundCheckObj.transform.localPosition = new Vector3(0, -1, 0);
            groundCheck = groundCheckObj.transform;
        }
        
        // Check if shoot point is set
        if (shootPoint == null)
        {
            Debug.LogWarning("FirstPersonController: Shoot point transform is not set! Using camera transform.");
            shootPoint = playerCamera;
        }
        
        // Check if projectile prefab is assigned
        if (projectilePrefab == null)
        {
            Debug.LogError("FirstPersonController: Projectile prefab is not assigned!");
        }
        
        // Initialize weapon
        currentAmmo = maxAmmo;
        
        wallJumpTimer = 0f;
        Debug.Log("FirstPersonController: Initialization complete");
        Debug.Log($"FirstPersonController: Weapon initialized with {currentAmmo} ammo");
    }

    private void Update()
    {
        Debug.Log("FirstPersonController: Update function called");
        
        HandleGroundCheck();
        HandleWallCheck();
        HandleMouseLook();
        HandleMovementInput();
        HandleJumpInput();
        HandleWallJump();
        UpdateWallJumpCooldown();
        HandleShooting();
        HandleReloading();
        ApplyMovement();
    }

    private void HandleGroundCheck()
    {
        Debug.Log("FirstPersonController: HandleGroundCheck function called");
        
        // Check if player is grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        // Apply gravity and reset velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            Debug.Log("FirstPersonController: Player is grounded, resetting Y velocity");
            velocity.y = -2f; // Small negative value to keep player grounded
            
            // Reset wall jumping state when grounded
            if (isWallJumping)
            {
                Debug.Log("FirstPersonController: Wall jump ended - player touched ground");
                isWallJumping = false;
            }
        }
    }

    private void HandleWallCheck()
    {
        Debug.Log("FirstPersonController: HandleWallCheck function called");
        
        // Previous wall touch state
        bool wasWallTouching = isTouchingWall;
        isTouchingWall = false;
        
        // Check for walls in multiple directions (forward, left, right, and slightly angled directions)
        Vector3[] checkDirections = {
            transform.forward, 
            -transform.forward, 
            transform.right, 
            -transform.right,
            (transform.forward + transform.right).normalized,
            (transform.forward - transform.right).normalized,
            (-transform.forward + transform.right).normalized,
            (-transform.forward - transform.right).normalized
        };
        
        foreach (Vector3 direction in checkDirections)
        {
            Debug.DrawRay(transform.position, direction * wallCheckDistance, Color.red);
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, wallCheckDistance, wallMask))
            {
                // Only consider walls if not grounded
                if (!isGrounded)
                {
                    wallNormal = hit.normal;
                    isTouchingWall = true;
                    Debug.Log($"FirstPersonController: Wall detected in direction {direction}, normal: {wallNormal}");
                    break;
                }
            }
        }
        
        // Log when wall touch state changes
        if (wasWallTouching != isTouchingWall)
        {
            if (isTouchingWall)
            {
                Debug.Log("FirstPersonController: Player started touching wall");
                lastWallNormal = wallNormal;
            }
            else
            {
                Debug.Log("FirstPersonController: Player stopped touching wall");
            }
        }
    }

    private void HandleMouseLook()
    {
        Debug.Log("FirstPersonController: HandleMouseLook function called");
        
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        // Calculate vertical rotation (looking up/down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevent looking too far up/down
        
        // Apply rotations
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
        
        Debug.Log($"FirstPersonController: Mouse look updated - X: {mouseX}, Y: {mouseY}, xRotation: {xRotation}");
    }

    private void HandleMovementInput()
    {
        Debug.Log("FirstPersonController: HandleMovementInput function called");
        
        // Get input axes
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        // Create movement vector relative to player's orientation
        Vector3 move = transform.right * x + transform.forward * z;
        
        // Limit movement during wall jump
        if (isWallJumping)
        {
            // Reduce player control during wall jump
            move *= 0.5f;
            Debug.Log("FirstPersonController: Movement reduced due to wall jumping");
        }
        
        // Apply movement to controller
        controller.Move(move * walkSpeed * Time.deltaTime);
        
        Debug.Log($"FirstPersonController: Movement input - X: {x}, Z: {z}, Vector: {move}");
    }

    private void HandleJumpInput()
    {
        Debug.Log("FirstPersonController: HandleJumpInput function called");
        
        // Check for jump input when grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Debug.Log("FirstPersonController: Standard jump triggered");
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }
    
    private void HandleWallJump()
    {
        Debug.Log("FirstPersonController: HandleWallJump function called");
        
        // Check for wall jump input when touching wall, not grounded, and cooldown finished
        if (Input.GetButtonDown("Jump") && isTouchingWall && !isGrounded && wallJumpTimer <= 0)
        {
            Debug.Log("FirstPersonController: Wall jump initiated");
            isWallJumping = true;
            wallJumpTimer = wallJumpCooldown;
            
            // Store the wall normal for the jump direction
            lastWallNormal = wallNormal;
            
            // Calculate jump direction (away from wall)
            Vector3 jumpDirection = lastWallNormal * wallJumpForce;
            
            // Set the velocity for wall jump (horizontal direction + upward force)
            velocity.x = jumpDirection.x;
            velocity.z = jumpDirection.z;
            velocity.y = Mathf.Sqrt(wallJumpUpwardForce * -2f * gravity);
            
            Debug.Log($"FirstPersonController: Wall jump executed - Direction: {jumpDirection}, Velocity: {velocity}");
        }
        
        // Handle wall sliding - slow the fall when touching a wall
        if (isTouchingWall && !isGrounded && velocity.y < 0)
        {
            Debug.Log("FirstPersonController: Wall sliding active");
            velocity.y = Mathf.Max(velocity.y, -wallSlidingSpeed);
        }
    }
    
    private void UpdateWallJumpCooldown()
    {
        Debug.Log("FirstPersonController: UpdateWallJumpCooldown function called");
        
        // Update wall jump cooldown timer
        if (wallJumpTimer > 0)
        {
            wallJumpTimer -= Time.deltaTime;
            Debug.Log($"FirstPersonController: Wall jump cooldown: {wallJumpTimer}");
        }
        
        // End wall jumping state after a brief period
        if (isWallJumping && wallJumpTimer <= wallJumpCooldown * 0.5f)
        {
            Debug.Log("FirstPersonController: Wall jump movement phase ended");
            isWallJumping = false;
        }
    }
    
    private void HandleShooting()
    {
        Debug.Log("FirstPersonController: HandleShooting function called");
        
        // Check if we can fire (not reloading, has ammo, and fire rate allows)
        if (!isReloading && (infiniteAmmo || currentAmmo > 0) && Time.time >= nextFireTime)
        {
            // Check for fire input (left mouse button)
            if (Input.GetMouseButtonDown(0))
            {
                ShootProjectile();
                nextFireTime = Time.time + fireRate;
                
                if (!infiniteAmmo)
                {
                    currentAmmo--;
                    Debug.Log($"FirstPersonController: Ammo reduced to {currentAmmo}");
                }
            }
        }
        
        // Clean up destroyed projectiles from list
        for (int i = spawnedProjectiles.Count - 1; i >= 0; i--)
        {
            if (spawnedProjectiles[i] == null)
            {
                spawnedProjectiles.RemoveAt(i);
            }
        }
    }
    
    private void ShootProjectile()
    {
        Debug.Log("FirstPersonController: ShootProjectile function called");
        
        // Check if prefab is assigned
        if (projectilePrefab == null)
        {
            Debug.LogError("FirstPersonController: Cannot shoot - projectilePrefab is not assigned!");
            return;
        }
        
        // Create projectile at shoot point
        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
        spawnedProjectiles.Add(projectile);
        
        // Add the projectile component if not already there
        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent == null)
        {
            projectileComponent = projectile.AddComponent<Projectile>();
            Debug.Log("FirstPersonController: Added Projectile component to instantiated projectile");
        }
        
        // Initialize the projectile
        projectileComponent.Initialize(shootPoint.forward * shootForce);
        
        Debug.Log($"FirstPersonController: Projectile fired - Direction: {shootPoint.forward}, Force: {shootForce}");
        
        // Add rigidbody if not present
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Debug.Log("FirstPersonController: Added Rigidbody component to projectile");
        }
        
        // Add collider if not present
        Collider collider = projectile.GetComponent<Collider>();
        if (collider == null)
        {
            SphereCollider sphereCollider = projectile.AddComponent<SphereCollider>();
            sphereCollider.radius = 0.25f;
            Debug.Log("FirstPersonController: Added SphereCollider component to projectile");
        }
        
        // Apply force to the projectile
        rb.AddForce(shootPoint.forward * shootForce, ForceMode.Impulse);
        
        // Destroy the projectile after some time to prevent memory issues
        Destroy(projectile, 5f);
    }
    
    private void HandleReloading()
    {
        Debug.Log("FirstPersonController: HandleReloading function called");
        
        // Check for reload input (R key)
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartReload();
        }
        
        // Process reload timer
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            
            if (reloadTimer <= 0)
            {
                FinishReload();
            }
        }
    }
    
    private void StartReload()
    {
        Debug.Log("FirstPersonController: StartReload function called");
        isReloading = true;
        reloadTimer = reloadTime;
        Debug.Log($"FirstPersonController: Reload started - Duration: {reloadTime}s");
    }
    
    private void FinishReload()
    {
        Debug.Log("FirstPersonController: FinishReload function called");
        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log($"FirstPersonController: Reload completed - Ammo restored to {currentAmmo}");
    }

    private void ApplyMovement()
    {
        Debug.Log("FirstPersonController: ApplyMovement function called");
        
        // Apply gravity unless wall sliding
        if (!(isTouchingWall && !isGrounded && velocity.y < 0))
        {
            velocity.y += gravity * Time.deltaTime;
        }
        
        // Apply vertical movement
        controller.Move(velocity * Time.deltaTime);
        
        Debug.Log($"FirstPersonController: Applied gravity. Current velocity: {velocity}");
    }

    // Function to change movement speed at runtime
    public void SetMovementSpeed(float newSpeed)
    {
        Debug.Log($"FirstPersonController: SetMovementSpeed called - Old: {walkSpeed}, New: {newSpeed}");
        walkSpeed = newSpeed;
    }

    // Function to toggle cursor lock state
    public void ToggleCursorLock(bool lockCursor)
    {
        Debug.Log($"FirstPersonController: ToggleCursorLock called - Lock cursor: {lockCursor}");
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
    }
    
    // Function to adjust wall jump settings at runtime
    public void AdjustWallJumpSettings(float force, float upwardForce, float cooldown)
    {
        Debug.Log($"FirstPersonController: AdjustWallJumpSettings called - Force: {force}, Upward: {upwardForce}, Cooldown: {cooldown}");
        wallJumpForce = force;
        wallJumpUpwardForce = upwardForce;
        wallJumpCooldown = cooldown;
    }
    
    // Function to adjust weapon settings at runtime
    public void AdjustWeaponSettings(float newShootForce, float newFireRate)
    {
        Debug.Log($"FirstPersonController: AdjustWeaponSettings called - Force: {newShootForce}, Rate: {newFireRate}");
        shootForce = newShootForce;
        fireRate = newFireRate;
    }
    
    // Function to get current ammo amount
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }
    
    // Function to add ammo
    public void AddAmmo(int amount)
    {
        Debug.Log($"FirstPersonController: AddAmmo called - Amount: {amount}");
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        Debug.Log($"FirstPersonController: Current ammo: {currentAmmo}/{maxAmmo}");
    }
    
    // Debug visualization
    private void OnDrawGizmos()
    {
        // Draw ground check sphere
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
        
        // Draw wall check directions
        if (Application.isPlaying)
        {
            Gizmos.color = isTouchingWall ? Color.blue : Color.yellow;
            Vector3[] checkDirections = {
                transform.forward, 
                -transform.forward, 
                transform.right, 
                -transform.right,
                (transform.forward + transform.right).normalized,
                (transform.forward - transform.right).normalized,
                (-transform.forward + transform.right).normalized,
                (-transform.forward - transform.right).normalized
            };
            
            foreach (Vector3 direction in checkDirections)
            {
                Gizmos.DrawRay(transform.position, direction * wallCheckDistance);
            }
            
            // Draw last wall normal
            if (isTouchingWall)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(transform.position, wallNormal * 2);
            }
            
            // Draw shoot direction
            if (shootPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(shootPoint.position, shootPoint.forward * 3);
            }
        }
    }
}