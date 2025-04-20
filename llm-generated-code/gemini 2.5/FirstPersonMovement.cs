using UnityEngine;

// Ensure the GameObject has a CharacterController component
[RequireComponent(typeof(CharacterController))]
public class FirstPersonMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f; // Speed for walking
    [SerializeField] private float jumpHeight = 1.5f; // How high the player can jump
    [SerializeField] private float gravity = -19.62f; // Gravity force

    [Header("Wall Jump Settings")]
    [SerializeField] private string wallTag = "Wall"; // Tag assigned to jumpable walls
    [SerializeField] private float wallJumpUpwardForce = 7.0f; // Upward force for wall jump
    [SerializeField] private float wallJumpOutwardForce = 6.0f; // Outward force (away from wall) for wall jump
    // Optional: Add wall slide functionality later if desired
    // [SerializeField] private float wallSlideSpeed = -2.0f; // Max downward speed while sliding

    private CharacterController characterController;
    private Vector3 playerVelocity; // Stores the player's vertical velocity (jumping, gravity) and wall jump force
    private bool isGrounded; // Tracks if the player is touching the ground

    // Wall Jump State
    private bool isTouchingWall = false; // Is the player currently touching a wall suitable for jumping?
    private Vector3 lastWallNormal; // Normal vector of the last wall touched

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        Debug.Log("Awake: Initializing FirstPersonMovement script.");
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("Awake: CharacterController component not found on this GameObject!");
        }
        else
        {
            Debug.Log("Awake: CharacterController component successfully retrieved.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Update: Starting frame update.");

        // Reset wall touch status at the beginning of the frame.
        // It will be set to true in OnControllerColliderHit if a wall collision occurs during characterController.Move
        // bool previousIsTouchingWall = isTouchingWall; // Keep previous state for comparison if needed
        isTouchingWall = false;
        // Note: The actual check happens *during* Move, so isTouchingWall reflects hits from the *previous* Move command by the time we check it here. Let's adjust logic flow.

        // --- Ground Check ---
        CheckIfGrounded(); // Updates isGrounded and potentially resets vertical velocity if landing

        // --- Horizontal Movement ---
        // Calculate horizontal movement intent based on input
        Vector3 horizontalMoveIntent = CalculateHorizontalMovement();
        Debug.Log($"Update: Calculated horizontal move intent: {horizontalMoveIntent.ToString("F3")}");

        // --- Jumping (Handles both ground jump and wall jump) ---
        HandleJumping(); // Modifies playerVelocity.y based on ground or wall jump conditions

        // --- Gravity & Wall Slide ---
        ApplyVerticalForces(); // Applies gravity or wall slide logic

        // --- Combine and Apply Final Movement ---
        // Combine horizontal intent with vertical velocity (gravity/jump/walljump)
        Vector3 finalVelocity = horizontalMoveIntent + playerVelocity; // Horizontal is intent, vertical is accumulated velocity

        Debug.Log($"Update: Applying final movement. Horizontal Intent: {horizontalMoveIntent.ToString("F3")}, Vertical Velocity: {playerVelocity.y.ToString("F3")}, Combined: {finalVelocity.ToString("F3")}");

        // Move the CharacterController
        // OnControllerColliderHit will be called *during* this Move execution if collisions occur
        characterController.Move(finalVelocity * Time.deltaTime);

        // Now, after Move, isTouchingWall and lastWallNormal will be updated if a wall hit occurred.
        Debug.Log($"Update: Frame update finished. isTouchingWall state after Move: {isTouchingWall}");
    }

    // Checks if the character controller is grounded
    void CheckIfGrounded()
    {
        Debug.Log("CheckIfGrounded: Performing ground check.");
        isGrounded = characterController.isGrounded;

        // If grounded and velocity was falling, reset vertical velocity.
        if (isGrounded && playerVelocity.y < 0)
        {
            Debug.Log("CheckIfGrounded: Player landed. Resetting vertical velocity.");
            playerVelocity.y = -2f; // Small negative value to keep grounded
        }
        Debug.Log($"CheckIfGrounded: IsGrounded = {isGrounded}");
    }

    // Calculates desired horizontal movement based on input (DOES NOT apply it yet)
    Vector3 CalculateHorizontalMovement()
    {
        Debug.Log("CalculateHorizontalMovement: Processing WASD/Arrow key input.");
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Debug.Log($"CalculateHorizontalMovement: Input - Horizontal={horizontalInput}, Vertical={verticalInput}");

        Vector3 moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;
        Vector3 horizontalMove = moveDirection * moveSpeed; // Scale by speed

        // Return the calculated horizontal movement vector for this frame
        return horizontalMove;
    }

    // Handles jump input for both ground and wall jumps
    void HandleJumping()
    {
        Debug.Log("HandleJumping: Checking for jump input.");
        bool jumpButtonPressed = Input.GetButtonDown("Jump");

        // --- Ground Jump ---
        if (isGrounded && jumpButtonPressed)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log($"HandleJumping: Ground Jump initiated! Setting vertical velocity to {playerVelocity.y.ToString("F3")}");
        }
        // --- Wall Jump ---
        // Check if airborne, touching a wall (detected in the *last* frame's Move via OnControllerColliderHit), and jump pressed
        else if (!isGrounded && isTouchingWall && jumpButtonPressed)
        {
            Debug.Log($"HandleJumping: Wall Jump initiated! Jumping off wall with normal {lastWallNormal.ToString("F3")}");
            // Apply forces: upward and outward from the wall normal
            playerVelocity.y = wallJumpUpwardForce; // Direct set Y velocity for upward push
            // Add horizontal force away from wall - We modify the horizontal part of playerVelocity here TEMPORARILY for the jump impulse
            // But the main horizontal control comes from CalculateHorizontalMovement. Let's rethink.
            // Best approach: Directly set playerVelocity for the jump impulse.
            playerVelocity = lastWallNormal * wallJumpOutwardForce + Vector3.up * wallJumpUpwardForce;

            Debug.Log($"HandleJumping: Wall jump velocity set to: {playerVelocity.ToString("F3")}");
             // Optional: Add a small cooldown or limit consecutive wall jumps if needed
        }
        else
        {
             Debug.Log($"HandleJumping: No jump action taken. isGrounded={isGrounded}, isTouchingWall={isTouchingWall}, jumpButtonPressed={jumpButtonPressed}");
        }
    }

    // Applies gravity or wall slide logic
    void ApplyVerticalForces()
    {
        Debug.Log("ApplyVerticalForces: Applying gravity or other vertical forces.");

        // --- Wall Sliding (Optional - uncomment and refine if desired) ---
        // if (isTouchingWall && !isGrounded && playerVelocity.y < 0)
        // {
        //     // If touching wall, falling, and not grounded -> potentially slide
        //     playerVelocity.y = Mathf.Max(playerVelocity.y, wallSlideSpeed); // Clamp downward speed
        //     Debug.Log($"ApplyVerticalForces: Wall Sliding active. Vertical velocity clamped at {playerVelocity.y.ToString("F3")}");
        // }

        // --- Gravity ---
        // Always apply gravity if not grounded
        if (!isGrounded)
        {
            playerVelocity.y += gravity * Time.deltaTime;
            Debug.Log($"ApplyVerticalForces: Applying gravity. New vertical velocity: {playerVelocity.y.ToString("F3")}");
        } else {
             Debug.Log("ApplyVerticalForces: Player is grounded, gravity not applied directly (Y velocity managed by CheckIfGrounded).");
        }
    }


    // This is called *during* CharacterController.Move whenever it collides with another collider.
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log($"OnControllerColliderHit: Collision detected with {hit.gameObject.name} (Tag: {hit.collider.tag}) Normal: {hit.normal.ToString("F3")} Flags: {characterController.collisionFlags}");

        // Check if the collided object has the specified wall tag
        if (hit.collider.CompareTag(wallTag))
        {
            // Check if the collision is predominantly horizontal (i.e., a wall, not floor or ceiling)
            // A check on hit.normal.y is good for this. Near 0 means vertical surface.
            if (Mathf.Abs(hit.normal.y) < 0.3f) // Allow slight inclines, adjust threshold as needed
            {
                // Check if the collision happened on the sides of the controller
                // This helps distinguish wall hits from scraping the top/bottom edges against a wall object
                if ((characterController.collisionFlags & CollisionFlags.Sides) != 0)
                {
                    Debug.Log($"OnControllerColliderHit: Valid wall collision detected with {hit.gameObject.name}. Setting isTouchingWall = true.");
                    isTouchingWall = true;
                    lastWallNormal = hit.normal; // Store the normal for the wall jump direction
                }
                else
                {
                    Debug.Log($"OnControllerColliderHit: Hit wall-tagged object {hit.gameObject.name}, but not on the sides (CollisionFlags: {characterController.collisionFlags}). Ignoring for wall jump state.");
                }
            }
            else
            {
                Debug.Log($"OnControllerColliderHit: Hit wall-tagged object {hit.gameObject.name}, but normal {hit.normal.ToString("F3")} is too vertical. Ignoring for wall jump state.");
            }
        }
        else
        {
             Debug.Log($"OnControllerColliderHit: Collision with non-wall object {hit.gameObject.name}.");
        }
    }

    // Optional: Draw gizmos in the Scene view for debugging
    void OnDrawGizmosSelected()
    {
        // Draw wall normal if touching wall
        if (isTouchingWall)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position + characterController.center, lastWallNormal * 2); // Draw ray showing wall normal
        }
        // Draw player velocity
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + characterController.center, playerVelocity); // Draw ray showing current vertical velocity vector
    }
}