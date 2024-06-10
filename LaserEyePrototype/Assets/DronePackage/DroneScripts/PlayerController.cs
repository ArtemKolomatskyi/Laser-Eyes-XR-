using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 2f; // Sensitivity for looking around
    public Camera playerCamera; // Reference to the player's camera
    public GameObject hmdRotationAnchor; // Reference to the HMD Rotation Anchor game object
    public GameObject collisionEffectPrefab; // Reference to the particle system prefab for collisions

    private float pitch = 0f; // Rotation around the X axis
    private float yaw = 0f; // Rotation around the Y axis
    private Quaternion initialCameraRotation; // Initial rotation of the first-person camera

    void Start()
    {
        // Cache the initial rotation of the first-person camera
        initialCameraRotation = playerCamera.transform.localRotation;
    }

    void Update()
    {
        MovePlayer();
        LookAround();
    }

    private void MovePlayer()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0, moveVertical) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.Self);
    }

    private void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        // Update the player's rotation around the Y axis
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f); // Clamp the pitch to avoid flipping

        // Apply rotation to the HMD Rotation Anchor
        hmdRotationAnchor.transform.localRotation = Quaternion.Euler(pitch, yaw, 0) * initialCameraRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Debug.Log("Player collided with an enemy.");
            CreateCollisionEffect(other);
        }
    }

    private void CreateCollisionEffect(Collider enemy)
    {
        // Instantiate the particle effect at the collision point
        Vector3 collisionPoint = enemy.ClosestPoint(transform.position);
        GameObject effect = Instantiate(collisionEffectPrefab, collisionPoint, Quaternion.identity);

        // Adjust the rotation of the effect to match the direction of the impact
        Vector3 direction = collisionPoint - transform.position;
        effect.transform.rotation = Quaternion.LookRotation(direction);

        // Optional: Destroy the effect after a certain time to clean up
        Destroy(effect, 2f); // Adjust the time as needed
    }
}
