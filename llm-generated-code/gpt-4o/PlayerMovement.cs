using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float gravity = -9.81f;
    public CharacterController controller;

    [Header("Wall Jump Settings")]
    public float wallJumpForce = 8f;
    public float wallDetectionDistance = 1f;
    public LayerMask wallLayer;

    [Header("Mouse Look Settings")]
    public Transform playerBody;
    public Transform cameraTransform;
    public float mouseSensitivity = 100f;
    private float xRotation = 0f;

    [Header("Shooting Settings")]
    public GameObject projectilePrefab;
    public Transform shootPoint;

    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Start(): Player initialized. Cursor locked.");
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleJump();
        HandleWallJump();
        HandleShooting();
    }

    // ----------------- Mouse Look -----------------
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);

        Debug.Log($"HandleMouseLook(): MouseX = {mouseX}, MouseY = {mouseY}, xRotation = {xRotation}");
    }

    // ----------------- Movement -----------------
    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * moveSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        Debug.Log($"HandleMovement(): moveX = {moveX}, moveZ = {moveZ}, velocity.y = {velocity.y}");
    }

    // ----------------- Jumping -----------------
    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            Debug.Log("HandleJump(): Jumped!");
        }
    }

    // ----------------- Wall Jump -----------------
    void HandleWallJump()
    {
        if (Input.GetButtonDown("Jump") && !isGrounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.right, out hit, wallDetectionDistance, wallLayer) ||
                Physics.Raycast(transform.position, -transform.right, out hit, wallDetectionDistance, wallLayer))
            {
                velocity.y = Mathf.Sqrt(wallJumpForce * -2f * gravity);
                Debug.Log("HandleWallJump(): Wall jumped!");
            }
        }
    }

    // ----------------- Shooting -----------------
    void HandleShooting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (projectilePrefab && shootPoint)
            {
                GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
                Debug.Log("HandleShooting(): Projectile fired.");
            }
            else
            {
                Debug.LogWarning("HandleShooting(): projectilePrefab or shootPoint not assigned.");
            }
        }
    }
}
