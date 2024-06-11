using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private float _spawnDelay = 1.5f;
    private MRUKRoom _room;
    private GameManager _gameManager;
    
    void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    /// <summary>
    /// Enemy should be spawned after MRUK Scene has been loaded and the NavMesh has been created by created by <see cref="SceneNavigation"/> script
    /// </summary>
    public void SpawnEnemy(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            StartCoroutine(_SpawnEnemy());
        }
    }

    private IEnumerator _SpawnEnemy()
    {
        yield return new WaitForSeconds(_spawnDelay);
        
        Debug.Log("MRUK Room Nav Mesh Has Been Created");
        if (!_room) _room = MRUK.Instance?.GetCurrentRoom();
        if (!_room)
        {
            Debug.LogError("Cannot spawn enemy because MRUKRoom is absent");
            yield return null;
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

        GameObject enemy = Instantiate(_enemyPrefab, position, Quaternion.identity);
        _gameManager.EnemySpawned(enemy);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
