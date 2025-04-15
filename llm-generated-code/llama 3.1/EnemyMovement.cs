using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform player; // Store the transform of the player object

    private Vector3 moveDirection;
    private float moveSpeed = 5f;

    private void Update()
    {
        MoveTowardsPlayer();
    }

    private void MoveTowardsPlayer()
    {
        Debug.Log("Moving towards player.");

        // Calculate the direction from the enemy to the player
        moveDirection = (player.position - transform.position).normalized;
        transform.velocity = moveDirection * moveSpeed;

        // Rotate the enemy to face the player
        Quaternion rotation = Quaternion.LookRotation(-moveDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 5f);
    }
}
