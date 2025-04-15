using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float jumpForce = 10f;
    public float gravity = -9.81f;
    private Vector3 velocity;
    private bool isGrounded;

    private void Update()
    {
        Debug.Log("Update called.");

        // Calculate movement based on input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;
        transform.position += moveDirection * Time.deltaTime * moveSpeed;

        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Debug.Log("Jump button pressed and player is grounded.");
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

        // Debug logs for player movement
        Debug.Log($"Player position: {transform.position}");
        Debug.Log($"Velocity: {velocity}");
        Debug.Log($"Is Grounded: {isGrounded}");
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
