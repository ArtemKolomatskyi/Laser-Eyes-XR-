using UnityEngine;

public class DroneEnemy : MonoBehaviour
{
    #region Fields
    public Transform target; // The target the drone will attack
    public string targetLayerName = "Enemy"; // The layer name for the target

    public delegate void DroneDestroyedHandler(DroneEnemy drone);
    public event DroneDestroyedHandler OnDroneDestroyed;
    #endregion

    #region Unity Methods
    void Start()
    {
        Debug.Log("DroneEnemy Start method called.");
        if (target == null)
        {
            target = FindTargetInLayer(targetLayerName);
            if (target == null)
            {
                Debug.LogWarning("Target not found in the specified layer.");
                return;
            }
        }

        DroneBehavior droneBehavior = GetComponent<DroneBehavior>();
        if (droneBehavior != null)
        {
            droneBehavior.Initialize(1); // Initialize with default round number
            droneBehavior.TriggerAttackPattern(); // Trigger the default attack pattern
        }
    }

    private void OnDestroy()
    {
        Debug.Log("DroneEnemy destroyed.");
        if (OnDroneDestroyed != null)
        {
            OnDroneDestroyed(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Collision with player.");
        }
    }
    #endregion

    #region Private Methods
    private Transform FindTargetInLayer(string layerName)
    {
        Debug.Log($"Finding target in layer: {layerName}");
        int layer = LayerMask.NameToLayer(layerName);
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject go in gameObjects)
        {
            if (go.layer == layer)
            {
                return go.transform;
            }
        }
        return null;
    }
    #endregion
}
