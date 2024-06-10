using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    private MRUKRoom _room;
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    
    /// <summary>
    /// Enemy is spawned after MRUK Scene has been loaded to allow for the NavMesh created by <see cref="SceneNavigation"/> to be loaded
    /// </summary>
    public void OnMRUKRoomNavMeshCreated()
    {
        Debug.Log("MRUK Room Nav Mesh Has Been Created");
        if (!_room) _room = MRUK.Instance?.GetCurrentRoom();
        if (!_room)
        {
            Debug.LogError("Cannot spawn enemy because MRUKRoom is absent");
            return;
        }

        NavMeshAgent navMeshAgent = _enemyPrefab.GetComponent<NavMeshAgent>();
        
        Vector3? nullablePosition = null;
        while (nullablePosition == null)
        {
            nullablePosition = 
                _room.GenerateRandomPositionInRoom(navMeshAgent.radius*2f, true);
            if(nullablePosition == null) Debug.LogError("TargetDestination inside MRUK Current Room is NULL");
        }
        Vector3 position = nullablePosition ?? Vector3.zero;
        position.y = 0;

        Instantiate(_enemyPrefab, position, Quaternion.identity);
    }
}
