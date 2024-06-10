using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneSpawnManager : MonoBehaviour
{
    #region Fields
    [Header("Drone Settings")]
    [SerializeField] private GameObject dronePrefab; // Reference to the Drone prefab
    [SerializeField] private Transform[] spawnPoints; // Array of spawn points for the Drones
    [SerializeField] private Transform[] regroupPoints; // Array of regroup points for the Drones
    [SerializeField] private float spawnInterval = 0.1f; // Time interval between spawns in seconds
    [SerializeField] private GameObject permissibleAttackArea; // Plane to constrain the drones' movement
    [SerializeField] private bool autoStartAttack = false; // Option to start attack automatically

    private int roundNumber = 0; // Current round number
    private bool isSpawning = false; // Check if drones are currently spawning
    private bool attackTriggered = false; // Ensure attack pattern can only be triggered once per round
    private List<GameObject> activeDrones = new List<GameObject>(); // List to keep track of active drones
    #endregion

    #region Unity Methods
    private void Update()
    {
        if (!isSpawning)
        {
            HandleRoundInput();
        }

        if (!attackTriggered && !autoStartAttack)
        {
            TriggerAttackPatterns();
            attackTriggered = true;
        }
    }
    #endregion

    #region Private Methods
    private void HandleRoundInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) StartRound(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) StartRound(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) StartRound(3);
    }

    private void StartRound(int round)
    {
        Debug.Log($"Starting round: {round}");
        roundNumber = round;
        attackTriggered = false; // Reset attack trigger for new round
        int dronesToSpawn = round - activeDrones.Count;
        StartCoroutine(SpawnDrones(dronesToSpawn));
    }

    private IEnumerator SpawnDrones(int count)
    {
        Debug.Log($"Spawning {count} drones.");
        isSpawning = true;
        for (int i = 0; i < count; i++)
        {
            SpawnDrone();
            yield return new WaitForSeconds(spawnInterval);
        }
        isSpawning = false;

        if (autoStartAttack)
        {
            TriggerAttackPatterns();
        }
    }

    private void SpawnDrone()
    {
        Transform spawnPoint = GetUniqueSpawnPoint();
        GameObject drone = Instantiate(dronePrefab, spawnPoint.position, spawnPoint.rotation); // Instantiate drone
        activeDrones.Add(drone);
        Debug.Log("Drone spawned.");

        DroneBehavior droneBehavior = drone.GetComponent<DroneBehavior>();
        if (droneBehavior != null)
        {
            droneBehavior.Initialize();
            droneBehavior.SetSpawnPoint(spawnPoint);
            droneBehavior.SetOtherDrones(GetAllDroneBehaviors());
            droneBehavior.permissibleAttackArea = permissibleAttackArea; // Set permissible attack area
            droneBehavior.SetRegroupPoint(GetUniqueRegroupPoint());

            // Set the attack pattern based on the round number
            switch (roundNumber)
            {
                case 1:
                    droneBehavior.attackPattern = DroneBehavior.AttackPattern.SeekAndDestroy;
                    break;
                case 2:
                    droneBehavior.attackPattern = DroneBehavior.AttackPattern.HomingCharge;
                    break;
                case 3:
                    droneBehavior.attackPattern = DroneBehavior.AttackPattern.CircularEncirclement;
                    break;
            }
            Debug.Log($"Drone attack pattern set to: {droneBehavior.attackPattern}");
        }

        DroneEnemy droneEnemy = drone.GetComponent<DroneEnemy>();
        if (droneEnemy != null)
        {
            droneEnemy.OnDroneDestroyed += OnDroneDestroyed;
        }
    }

    private List<DroneBehavior> GetAllDroneBehaviors()
    {
        List<DroneBehavior> droneBehaviors = new List<DroneBehavior>();
        foreach (GameObject drone in activeDrones)
        {
            DroneBehavior behavior = drone.GetComponent<DroneBehavior>();
            if (behavior != null)
            {
                droneBehaviors.Add(behavior);
            }
        }
        return droneBehaviors;
    }

    private Transform GetUniqueSpawnPoint()
    {
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);
        foreach (GameObject drone in activeDrones)
        {
            DroneBehavior behavior = drone.GetComponent<DroneBehavior>();
            if (behavior != null && availableSpawnPoints.Contains(behavior.GetSpawnPoint()))
            {
                availableSpawnPoints.Remove(behavior.GetSpawnPoint());
            }
        }
        return availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];
    }

    private Transform GetUniqueRegroupPoint()
    {
        List<Transform> availableRegroupPoints = new List<Transform>(regroupPoints);
        foreach (GameObject drone in activeDrones)
        {
            DroneBehavior behavior = drone.GetComponent<DroneBehavior>();
            if (behavior != null && availableRegroupPoints.Contains(behavior.GetSpawnPoint()))
            {
                availableRegroupPoints.Remove(behavior.GetSpawnPoint());
            }
        }
        return availableRegroupPoints[Random.Range(0, availableRegroupPoints.Count)];
    }

    private void OnDroneDestroyed(DroneEnemy drone)
    {
        Debug.Log("Drone destroyed, removing from active drones.");
        activeDrones.Remove(drone.gameObject);
        Destroy(drone.gameObject); // Destroy the drone instance
    }

    private void TriggerAttackPatterns()
    {
        Debug.Log("Triggering attack patterns for all active drones.");
        foreach (GameObject drone in activeDrones)
        {
            DroneBehavior droneBehavior = drone.GetComponent<DroneBehavior>();
            if (droneBehavior != null)
            {
                droneBehavior.TriggerAttackPattern();
            }
        }
    }
    #endregion
}
