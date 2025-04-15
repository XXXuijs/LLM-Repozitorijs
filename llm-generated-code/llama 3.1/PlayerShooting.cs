using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public Transform firePoint; // Store the transform of the fire point object
    public GameObject projectilePrefab; // Store the reference to the projectile prefab

    private void Awake()
    {
        // Assign the transform of the child object named "FirePoint" to firePoint
        firePoint = transform.Find("FirePoint");
        if (firePoint == null)
        {
            Debug.LogError("FirePoint object not found in the scene.");
        }
    }

    private void OnMouseButtonDown(int button)
    {
        if (button == 0) // Left mouse button click
        {
            ShootProjectile();
        }
    }

    private void ShootProjectile()
    {
        Debug.Log("Shooting projectile.");

        // Instantiate a new projectile at the fire point position and rotation
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Add velocity to the projectile based on the player's forward direction
        Vector3 projectileVelocity = transform.TransformDirection(Vector3.forward) * 10f;
        projectile.GetComponent<Rigidbody>().linearVelocity = projectileVelocity;

        Debug.Log("Projectile instantiated: " + projectile.name);
    }
}
