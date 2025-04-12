using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonShooter : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private LayerMask groundMask;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private int maxAirJumps = 1;

    [Header("Wall Jump Settings")]
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private float wallJumpVerticalForce = 7f;
    [SerializeField] private float wallJumpHorizontalForce = 10f;
    [SerializeField] private float wallJumpCooldown = 0.5f;
    [SerializeField] private LayerMask wallMask;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform weaponMuzzle;
    [SerializeField] private float projectileSpeed = 30f;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private float reloadTime = 1.5f;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool invertY = false;

    private CharacterController characterController;
    private AudioSource audioSource;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isTouchingWall;
    private RaycastHit wallHit;
    private float xRotation = 0f;
    private float currentSpeed;
    private int airJumpCount;
    private float lastWallJumpTime;
    private Vector3 wallNormal;
    private int currentAmmo;
    private float nextFireTime;
    private bool isReloading;

    private void Awake()
    {
        Debug.Log("[FirstPersonShooter] Initializing controller and weapon system");
        
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        
        if (characterController == null) Debug.LogError("[FirstPersonShooter] Missing CharacterController!");
        if (cameraTransform == null) Debug.LogError("[FirstPersonShooter] Missing camera reference!");
        if (weaponMuzzle == null) Debug.LogError("[FirstPersonShooter] Missing weapon muzzle reference!");
        if (projectilePrefab == null) Debug.LogError("[FirstPersonShooter] Missing projectile prefab!");

        Cursor.lockState = CursorLockMode.Locked;
        currentAmmo = maxAmmo;
    }

    private void Start()
    {
        Debug.Log($"[FirstPersonShooter] Ready with {currentAmmo}/{maxAmmo} ammo");
        currentSpeed = walkSpeed;
        airJumpCount = maxAirJumps;
    }

    private void Update()
    {
        if (isReloading) return;

        HandleGroundCheck();
        HandleWallCheck();
        HandleMovement();
        HandleJump();
        HandleWallJump();
        HandleLook();
        HandleShooting();
        HandleReload();
        ApplyGravity();
    }

    private void HandleGroundCheck()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundMask);

        if (isGrounded && !wasGrounded)
        {
            Debug.Log("[FirstPersonShooter] Player landed");
            airJumpCount = maxAirJumps;
            velocity.y = -2f;
        }
    }

    private void HandleWallCheck()
    {
        bool wasTouchingWall = isTouchingWall;
        isTouchingWall = Physics.SphereCast(transform.position, characterController.radius, transform.forward, 
                                          out wallHit, wallCheckDistance, wallMask);

        if (isTouchingWall && !wasTouchingWall)
        {
            wallNormal = wallHit.normal;
            Debug.Log($"[FirstPersonShooter] Touching wall: {wallHit.collider.name}");
        }
    }

    private void HandleMovement()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            currentSpeed = runSpeed;
            Debug.Log("[FirstPersonShooter] Sprint activated");
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            currentSpeed = walkSpeed;
        }

        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (input.magnitude > 0.1f) Debug.Log($"[FirstPersonShooter] Movement: {input}");

        Vector3 moveDirection = transform.right * input.x + transform.forward * input.y;
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                PerformJump(jumpForce, "ground jump");
            }
            else if (airJumpCount > 0)
            {
                PerformJump(jumpForce * 0.8f, $"air jump ({maxAirJumps - airJumpCount + 1}/{maxAirJumps})");
                airJumpCount--;
            }
        }
    }

    private void HandleWallJump()
    {
        if (isTouchingWall && Input.GetButtonDown("Jump") && Time.time > lastWallJumpTime + wallJumpCooldown)
        {
            Vector3 jumpDirection = (wallNormal + Vector3.up).normalized;
            velocity.y = Mathf.Sqrt(wallJumpVerticalForce * -2f * gravity);
            characterController.Move(jumpDirection * wallJumpHorizontalForce * Time.deltaTime);
            
            lastWallJumpTime = Time.time;
            airJumpCount = maxAirJumps;
            
            Debug.Log($"[FirstPersonShooter] Wall jump! Force: {jumpDirection * wallJumpHorizontalForce}");
        }
    }

    private void HandleShooting()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime && currentAmmo > 0)
        {
            nextFireTime = Time.time + fireRate;
            FireProjectile();
        }
        else if (Input.GetButtonDown("Fire1") && currentAmmo <= 0)
        {
            Debug.Log("[FirstPersonShooter] Out of ammo!");
        }
    }

    private void HandleReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            Debug.Log($"[FirstPersonShooter] Reloading...");
            isReloading = true;
            PlaySound(reloadSound);
            Invoke("FinishReload", reloadTime);
        }
    }

    private void FinishReload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log($"[FirstPersonShooter] Reload complete. Ammo: {currentAmmo}/{maxAmmo}");
    }

    private void FireProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, weaponMuzzle.position, weaponMuzzle.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            rb.linearVelocity = weaponMuzzle.forward * projectileSpeed;
            Debug.Log($"[FirstPersonShooter] Fired projectile at {rb.linearVelocity.magnitude}m/s");
        }
        else
        {
            Debug.LogError("[FirstPersonShooter] Projectile missing Rigidbody!");
        }

        currentAmmo--;
        PlaySound(shootSound);
        Debug.Log($"[FirstPersonShooter] Ammo: {currentAmmo}/{maxAmmo}");
    }

    private void PerformJump(float force, string jumpType)
    {
        velocity.y = Mathf.Sqrt(force * -2f * gravity);
        Debug.Log($"[FirstPersonShooter] Executed {jumpType} with force {force}");
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void HandleLook()
    {
        Vector2 mouseInput = new Vector2(
            Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime,
            Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime
        );

        if (mouseInput.magnitude > 0.01f)
        {
            Debug.Log($"[FirstPersonShooter] Look input: {mouseInput}");
        }

        xRotation += mouseInput.y * (invertY ? 1 : -1);
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseInput.x);
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, groundCheckDistance);

        Gizmos.color = isTouchingWall ? Color.blue : Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * wallCheckDistance, characterController.radius);

        if (weaponMuzzle != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(weaponMuzzle.position, weaponMuzzle.position + weaponMuzzle.forward * 2f);
        }
    }
}