using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class DroneBehavior : MonoBehaviour
{
    #region Enums
    public enum AttackPattern
    {
        SeekAndDestroy,
        HomingCharge,
        CircularEncirclement
    }
    #endregion

    #region Fields
    [Header("Drone Settings")]
    public AttackPattern attackPattern; // Current attack pattern
    public GameObject permissibleAttackArea; // Plane to constrain the drone's movement
    public Transform target; // The target the drone will attack
    public int health = 100; // Health of the drone
    public float searchRadius = 10f; // Radius within which the drone will search for the player
    public float searchInterval = 1f; // Interval between search attempts

    private Transform spawnPoint; // The spawn point of the drone
    private Transform regroupPoint; // The regroup point of the drone
    private List<DroneBehavior> otherDrones = new List<DroneBehavior>();
    private Bounds attackAreaBounds;
    private bool isReturningToRegroup = false;
    private bool continueAttacking = true;
    private Rigidbody rb;
    private float minSeparationDistance = 2.0f; // Minimum distance between drones
    private bool isSearchingForPlayer = true; // Flag to indicate if the drone is searching for the player
    private float fallbackDuration = 0.5f; // Duration for fallback after collision
    private float fallbackDistance = 2f; // Distance for fallback after collision
    private Vector3 forceDirection; // Direction of the force applied during collision
    private bool isDefensive = false; // Flag to indicate if the drone is in defensive mode
    private bool isPatrolling = false; // Flag to indicate if the drone is patrolling
    #endregion

    #region Unity Methods
    private void Start()
    {
        Debug.Log("DroneBehavior Start method called.");
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Make the Rigidbody kinematic to prevent physics interactions
        }

        InitializeTarget();
        InitializeAttackArea();
        StartCoroutine(CheckProximity());
        StartCoroutine(SearchForPlayer());
    }

    private void Update()
    {
        ApplySeparation();
    }
    #endregion

    #region Public Methods
    public void Initialize()
    {
        Debug.Log("DroneBehavior Initialize called without parameters.");
        ExecuteAttackPattern(); // Start the attack pattern once initialized
    }

    public void Initialize(int round)
    {
        Debug.Log($"DroneBehavior Initialize called with round parameter: {round}");
        // Overload to take round argument
        ExecuteAttackPattern(); // Start the attack pattern once initialized
    }

    public void SetSpawnPoint(Transform point)
    {
        spawnPoint = point;
    }

    public Transform GetSpawnPoint()
    {
        return spawnPoint;
    }

    public void SetRegroupPoint(Transform point)
    {
        regroupPoint = point;
    }

    public void SetOtherDrones(List<DroneBehavior> drones)
    {
        otherDrones = new List<DroneBehavior>(drones);
        otherDrones.Remove(this); // Ensure the list does not contain itself
    }

    public void TriggerAttackPattern()
    {
        if (!isReturningToRegroup && !isDefensive && !isPatrolling)
        {
            continueAttacking = true;
            ExecuteAttackPattern();
        }
    }

    public void ReturnToRegroup()
    {
        Debug.Log("Drone is returning to regroup.");
        isReturningToRegroup = true;
        continueAttacking = false; // Stop attacking when returning to regroup
        transform.DOMove(GetConstrainedPosition(regroupPoint.position), 3f).SetEase(Ease.Linear).OnComplete(() =>
        {
            isReturningToRegroup = false;
            ExecuteAttackPattern(); // Start the attack pattern once regrouped
        });
    }
    #endregion

    #region Private Methods
    private void InitializeTarget()
    {
        if (target == null)
        {
            target = FindTargetInLayer("Player");
            if (target == null)
            {
                Debug.LogWarning("Player target not found in the specified layer.");
                return;
            }
        }
    }

    private void InitializeAttackArea()
    {
        if (permissibleAttackArea != null)
        {
            attackAreaBounds = permissibleAttackArea.GetComponent<Renderer>().bounds;
        }
        else
        {
            Debug.LogWarning("Permissible attack area not assigned. Attempting to find in scene...");
            permissibleAttackArea = GameObject.Find("PermissibleAttackArea"); // Default name to search
            if (permissibleAttackArea != null)
            {
                attackAreaBounds = permissibleAttackArea.GetComponent<Renderer>().bounds;
            }
            else
            {
                Debug.LogWarning("Permissible attack area not found in the scene.");
            }
        }
    }

    private Transform FindTargetInLayer(string layerName)
    {
        Debug.Log($"Finding target in layer: {layerName}");
        int layer = LayerMask.NameToLayer(layerName);
        foreach (GameObject go in FindObjectsOfType<GameObject>())
        {
            if (go.layer == layer)
            {
                return go.transform;
            }
        }
        return null;
    }
    private void ExecuteAttackPattern()
    {
        if (!continueAttacking)
            return;

        Debug.Log($"Executing attack pattern: {attackPattern}");
        switch (attackPattern)
        {
            case AttackPattern.SeekAndDestroy:
                SeekAndDestroy();
                break;
            case AttackPattern.HomingCharge:
                HomingCharge();
                break;
            case AttackPattern.CircularEncirclement:
                CircularEncirclement();
                break;
        }
    }

    private void SeekAndDestroy()
    {
        Debug.Log("Executing SeekAndDestroy pattern.");
        Vector3 targetPosition = new Vector3(target.position.x, attackAreaBounds.center.y, target.position.z); // Constrain to the plane

        Vector3[] path = new Vector3[2];
        path[0] = transform.position;
        path[1] = GetConstrainedPosition(targetPosition);

        transform.DOPath(path, 1f, PathType.Linear, PathMode.TopDown2D)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (isDefensive)
                {
                    // If the drone is in defensive mode, it will fall back
                    FallBack();
                }
                else
                {
                    ExecuteAttackPattern(); // Resume attack pattern if not defensive
                }
            });
    }

    private void HomingCharge()
    {
        Debug.Log("Executing HomingCharge pattern.");
        Vector3 targetPosition = new Vector3(target.position.x, attackAreaBounds.center.y, target.position.z); // Constrain to the plane

        Vector3[] path = new Vector3[2];
        path[0] = transform.position;
        path[1] = GetConstrainedPosition(targetPosition);

        transform.DOPath(path, 0.5f, PathType.Linear, PathMode.TopDown2D)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                if (isDefensive)
                {
                    // If the drone is in defensive mode, it will fall back
                    FallBack();
                }
                else
                {
                    ExecuteAttackPattern(); // Resume attack pattern if not defensive
                }
            });
    }

    private void CircularEncirclement()
    {
        Debug.Log("Executing CircularEncirclement pattern.");
        Vector3 centerPoint = target.position;
        float radius = 5f;
        int resolution = 36; // Number of points in the circle path

        Vector3[] path = new Vector3[resolution];
        for (int i = 0; i < resolution; i++)
        {
            float angle = i * Mathf.PI * 2f / resolution;
            path[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius + centerPoint;
        }

        transform.DOPath(path, 5f, PathType.Linear, PathMode.TopDown2D)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart)
            .OnWaypointChange(index =>
            {
                // Ensure drone stays within the attack area bounds
                transform.position = GetConstrainedPosition(transform.position);
            });
    }

    private void MoveToTarget(Ease ease, float duration, TweenCallback onComplete)
    {
        Debug.Log("Moving to target position.");
        Vector3 targetPosition = new Vector3(target.position.x, attackAreaBounds.center.y, target.position.z); // Constrain to the plane
        transform.DOMove(GetConstrainedPosition(targetPosition), duration).SetEase(ease).OnComplete(onComplete);
    }

    private Vector3 GetConstrainedPosition(Vector3 position)
    {
        position.y = attackAreaBounds.center.y; // Constrain to the plane's y-coordinate
        if (attackAreaBounds.Contains(position))
        {
            return position;
        }
        else
        {
            Vector3 constrainedPosition = attackAreaBounds.ClosestPoint(position);
            constrainedPosition.y = attackAreaBounds.center.y; // Keep constrained to the plane
            return constrainedPosition;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HandleCollisionWithPlayer(other);
        }
        else if (other.CompareTag("Drone"))
        {
            Debug.Log("Collision with another drone.");
        }
        else if (other.CompareTag("Laser") && other.gameObject.layer == LayerMask.NameToLayer("Lasers"))
        {
            HandleLaserCollision();
        }
    }

    private void HandleCollisionWithPlayer(Collider playerCollider)
    {
        Debug.Log("Drone collided with player, executing collision logic.");

        // Apply force to the player
        Rigidbody playerRigidbody = playerCollider.GetComponent<Rigidbody>();
        if (playerRigidbody != null)
        {
            forceDirection = playerCollider.transform.position - transform.position;
            forceDirection.y = 0; // Keep the force horizontal
            playerRigidbody.AddForce(forceDirection.normalized * 500f); // Adjust the force magnitude as needed
        }

        // Reduce player's health
        PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(10); // Adjust the damage amount as needed
        }

        // Reduce drone health
        health -= 10;
        if (health <= 0)
        {
            Debug.Log("Drone health depleted, destroying drone.");
            Destroy(gameObject); // Destroy drone if health is depleted
        }
        else
        {
            // Move back the drone a bit and then stay on the defensive
            FallBack();
        }
    }

    private void FallBack()
    {
        Vector3 fallbackDirection = -forceDirection.normalized * fallbackDistance;
        Vector3 fallbackPosition = GetConstrainedPosition(transform.position + fallbackDirection);
        Vector3[] fallbackPath = new Vector3[2];
        fallbackPath[0] = transform.position;
        fallbackPath[1] = fallbackPosition;

        isDefensive = true;
        transform.DOPath(fallbackPath, fallbackDuration, PathType.Linear, PathMode.TopDown2D)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // Stay on the defensive for a while
                StartCoroutine(StayDefensive());
            });
    }

    private IEnumerator StayDefensive()
    {
        Debug.Log("Drone is staying on the defensive.");
        yield return new WaitForSeconds(2f); // Stay defensive for 2 seconds

        isDefensive = false;
        Debug.Log("Drone starting patrol.");
        StartPatrol(); // Start patrolling after defensive period
    }

    private void StartPatrol()
    {
        isPatrolling = true;
        Vector3[] patrolPath = new Vector3[4];

        patrolPath[0] = GetRandomPositionWithinBounds();
        patrolPath[1] = GetRandomPositionWithinBounds();
        patrolPath[2] = GetRandomPositionWithinBounds();
        patrolPath[3] = GetRandomPositionWithinBounds();

        transform.DOPath(patrolPath, 10f, PathType.Linear, PathMode.TopDown2D)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                isPatrolling = false;
                Debug.Log("Patrol complete. Resuming attack pattern.");
                ExecuteAttackPattern(); // Resume attack pattern after patrol
            });
    }

    private Vector3 GetRandomPositionWithinBounds()
    {
        float x = Random.Range(attackAreaBounds.min.x, attackAreaBounds.max.x);
        float z = Random.Range(attackAreaBounds.min.z, attackAreaBounds.max.z);
        return new Vector3(x, attackAreaBounds.center.y, z);
    }

    private void HandleLaserCollision()
    {
        Debug.Log("Drone hit by laser, taking damage.");

        // Reduce drone health
        health -= 50; // Adjust damage as needed
        if (health <= 0)
        {
            Debug.Log("Drone health depleted, destroying drone.");
            Destroy(gameObject); // Destroy drone if health is depleted
        }
    }

    private IEnumerator CheckProximity()
    {
        while (true)
        {
            ApplySeparation();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ApplySeparation()
    {
        foreach (DroneBehavior otherDrone in otherDrones)
        {
            float distance = Vector3.Distance(transform.position, otherDrone.transform.position);
            if (distance < minSeparationDistance)
            {
                Vector3 direction = (transform.position - otherDrone.transform.position).normalized;
                Vector3 separationVector = direction * (minSeparationDistance - distance);
                transform.position += new Vector3(separationVector.x, 0, separationVector.z); // Adjust position on the horizontal plane
            }
        }
    }

    private IEnumerator SearchForPlayer()
    {
        while (isSearchingForPlayer)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    Debug.Log("Player found within search radius.");
                    target = hit.transform;
                    isSearchingForPlayer = false;
                    transform.DOMove(GetConstrainedPosition(regroupPoint.position), 1f).OnComplete(() =>
                    {
                        ExecuteAttackPattern();
                    });
                    yield break;
                }
            }
            yield return new WaitForSeconds(searchInterval);
        }
    }
    #endregion
}
