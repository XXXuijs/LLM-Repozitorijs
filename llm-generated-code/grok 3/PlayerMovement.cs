using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // Movement variables
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float wallJumpForce = 4f;
    [SerializeField] private float wallJumpUpwardForce = 4f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private LayerMask wallLayer;

    // Shooting variables
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float projectileLifetime = 3f;
    private float nextFireTime;

    // Player components
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private bool canWallJump;
    private Vector3 wallNormal;

    void Start()
    {
        InitializeComponents();
    }

    void Update()
    {
        CheckGroundStatus();
        CheckWall();
        HandleMovement();
        HandleJump();
        HandleWallJump();
        HandleShooting();
        ApplyGravity();
        MovePlayer();
    }

    private void InitializeComponents()
    {
        controller = GetComponent<CharacterController>();
        if (projectilePrefab == null) Debug.LogWarning("PlayerMovement: Projectile prefab not assigned!");
        if (firePoint == null) Debug.LogWarning("PlayerMovement: Fire point not assigned!");
        Debug.Log("PlayerMovement: CharacterController initialized.");
    }

    private void CheckGroundStatus()
    {
        isGrounded = controller.isGrounded;
        Debug.Log($"PlayerMovement: IsGrounded = {isGrounded}");

        // Reset vertical velocity when grounded to prevent gravity accumulation
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
            Debug.Log("PlayerMovement: Vertical velocity reset when grounded.");
        }
    }

    private void CheckWall()
    {
        canWallJump = false;
        if (!isGrounded)
        {
            RaycastHit hit;
            Vector3 raycastOrigin = transform.position + Vector3.up * 0.5f;
            if (Physics.Raycast(raycastOrigin, transform.forward, out hit, wallCheckDistance, wallLayer) ||
                Physics.Raycast(raycastOrigin, -transform.forward, out hit, wallCheckDistance, wallLayer) ||
                Physics.Raycast(raycastOrigin, transform.right, out hit, wallCheckDistance, wallLayer) ||
                Physics.Raycast(raycastOrigin, -transform.right, out hit, wallCheckDistance, wallLayer))
            {
                canWallJump = true;
                wallNormal = hit.normal;
                Debug.Log($"PlayerMovement: Wall detected, canWallJump = {canWallJump}, Wall Normal: {wallNormal}");
            }
            else
            {
                Debug.Log("PlayerMovement: No wall detected for wall jump.");
            }
        }
        else
        {
            Debug.Log("PlayerMovement: Player grounded, wall jump not checked.");
        }
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move = Vector3.ClampMagnitude(move, 1f) * walkSpeed;

        Debug.Log($"PlayerMovement: Movement input - Horizontal: {moveX}, Vertical: {moveZ}, Move Vector: {move}");

        controller.Move(move * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            Debug.Log($"PlayerMovement: Jump initiated, vertical velocity: {playerVelocity.y}");
        }
    }

    private void HandleWallJump()
    {
        if (Input.GetButtonDown("Jump") && canWallJump && !isGrounded)
        {
            // Apply upward force and push away from wall
            playerVelocity.y = Mathf.Sqrt(wallJumpUpwardForce * -2f * gravity);
            Vector3 wallJumpDirection = (wallNormal + Vector3.up).normalized;
            playerVelocity += wallJumpDirection * wallJumpForce;
            Debug.Log($"PlayerMovement: Wall jump initiated, vertical velocity: {playerVelocity.y}, direction: {wallJumpDirection}");
        }
    }

    private void HandleShooting()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            if (projectilePrefab != null && firePoint != null)
            {
                // Spawn projectile
                GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = firePoint.forward * projectileSpeed;
                    Debug.Log($"PlayerMovement: Projectile spawned at {firePoint.position}, velocity: {rb.linearVelocity}");
                }
                else
                {
                    Debug.LogWarning("PlayerMovement: Projectile prefab missing Rigidbody component!");
                }

                // Set projectile lifetime
                Destroy(projectile, projectileLifetime);
                Debug.Log($"PlayerMovement: Projectile scheduled for destruction after {projectileLifetime} seconds.");

                // Update fire rate
                nextFireTime = Time.time + fireRate;
                Debug.Log($"PlayerMovement: Shooting triggered, next fire time: {nextFireTime}");
            }
            else
            {
                Debug.LogWarning("PlayerMovement: Cannot shoot - projectile prefab or fire point not set!");
            }
        }
    }

    private void ApplyGravity()
    {
        playerVelocity.y += gravity * Time.deltaTime;
        Debug.Log($"PlayerMovement: Applying gravity, vertical velocity: {playerVelocity.y}");
    }

    private void MovePlayer()
    {
        controller.Move(playerVelocity * Time.deltaTime);
        Debug.Log($"PlayerMovement: Player moved with velocity: {playerVelocity}");
    }
}