using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float jumpForce = 10f;
    public float gravity = -9.81f;
    private Vector3 velocity;
    private bool isGrounded;
    private Transform wallCheck; // Store the transform of the wall check object

    private void Awake()
    {
        // Assign the transform of the child object named "WallCheck" to wallCheck
        wallCheck =transform.Find("WallCheck");
        if (wallCheck == null)
        {
            Debug.LogError("WallCheck object not found in the scene.");
        }
    }

    private void Update()
    {
        Debug.Log("Update called.");

        // Calculate movement based on input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;
        transform.position += moveDirection * Time.deltaTime * moveSpeed;

        // Handle wall jumping
        if (Input.GetButtonDown("Jump") && CanWallJump())
        {
            Debug.Log("Can perform wall jump.");
            velocity.y = jumpForce;
            isGrounded = false;
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        transform.Translate(velocity * Time.deltaTime);

        // Check if the player is grounded again
        if (isGrounded == false)
        {
            Debug.Log("Checking ground status...");
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -transform.up, out hit))
            {
                Debug.Log("Player is now grounded.");
                isGrounded = true;
            }
        }

        // Check for wall jumping
        if (!isGrounded && CanWallJump())
        {
            RaycastHit wallHit;
            if (Physics.Raycast(wallCheck.position, transform.TransformDirection(-Vector3.up), out wallHit, 1f))
            {
                Debug.Log("Wall detected.");
                isGrounded = true; // Set grounded to true for wall jumping
            }
        }

        // Debug logs for player movement
        Debug.Log($"Player position: {transform.position}");
        Debug.Log($"Velocity: {velocity}");
        Debug.Log($"Is Grounded: {isGrounded}");
    }

    private bool CanWallJump()
    {
        RaycastHit wallHit;
        if (Physics.Raycast(wallCheck.position, transform.TransformDirection(-Vector3.up), out wallHit, 1f))
        {
            return true;
        }
        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected.");

        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Player is grounded.");
            isGrounded = true;
        }
    }
}
