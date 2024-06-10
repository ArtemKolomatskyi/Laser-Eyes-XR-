#region DronePool
using System.Collections.Generic;
using UnityEngine;

public class DronePool : MonoBehaviour
{
    public static DronePool Instance;

    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private int poolSize = 10;
    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject drone = Instantiate(dronePrefab);
            drone.SetActive(false);
            pool.Enqueue(drone);
        }
    }

    public GameObject GetDrone()
    {
        if (pool.Count > 0)
        {
            GameObject drone = pool.Dequeue();
            drone.SetActive(true);
            return drone;
        }
        else
        {
            GameObject drone = Instantiate(dronePrefab);
            return drone;
        }
    }

    public void ReturnDrone(GameObject drone)
    {
        drone.SetActive(false);
        pool.Enqueue(drone);
    }
}
#endregion
