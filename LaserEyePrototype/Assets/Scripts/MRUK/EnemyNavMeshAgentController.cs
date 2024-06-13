using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Utilities.XR;

/// <summary>
/// Script component on the Enemy gameobject that sets the destination point of the NavMesh Agent component on it
/// </summary>
public class EnemyNavMeshAgentController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private float _distOffsetB2nEnemyNDestinationPoint = 0.1f;
    [SerializeField] private float _maxDistanceFromPlayer = 3.0f;
    private Vector3? _nullableTargetDestination = null;
    private Vector3 _targetDestination;
    private bool _destinationIsValid = false;
    private Vector3 _previousEnemyPosition;
    private Transform _player;
    private GameManager _gameManager;
    
    void Start()
    {
        if (!_navMeshAgent) _navMeshAgent = GetComponent<NavMeshAgent>();
        InitNavMeshAgent();
        _player = GameObject.FindGameObjectWithTag("MainCamera").transform;
        _gameManager = FindObjectOfType<GameManager>();
    }

    // This method is called on Start method because the script component is on Enemy gameobject which is spawned later on in game
    // Allow the NavMeshAgent to be placed on top of the NavMeshSurface by setting 'agentTypeID' of the NavMeshAgent to
    // be same as that of the NavMeshSurface
    private void InitNavMeshAgent()
    {
        SceneNavigation sceneNavigation = FindObjectOfType<SceneNavigation>();
        NavMeshSurface navMeshSurface = sceneNavigation.transform.GetComponent<NavMeshSurface>();
        
        _navMeshAgent.agentTypeID = navMeshSurface.GetBuildSettings().agentTypeID;
        StartCoroutine(SetNavAgentDestination_AfterDelay());
    }
    
    // A delay before the NavAgent is properly configured
    IEnumerator SetNavAgentDestination_AfterDelay()
    {
        yield return new WaitForSeconds(1.5f); //To allow enemy to come out of the portal
        // yield return new WaitForFixedUpdate();
        SetNewNavAgentDestination();
    }

    private void SetNewNavAgentDestination()
    {
        _nullableTargetDestination = null;
        while (_nullableTargetDestination == null)
        {
            _nullableTargetDestination = FindNavAgentNextDestination();
            if(_nullableTargetDestination == null) Debug.LogError("TargetDestination inside MRUK Current Room is NULL");
            else
            {
                Vector3 dest = _nullableTargetDestination ?? Vector3.zero;
                if (!DestinationPointIsReachableByNavAgent(dest))
                {
                    // log("Destination Point generated is unreachable by the NavMeshAgent. Finding another one...");
                    _nullableTargetDestination = null;
                }
                if(Vector3.Distance(dest, _player.position) > _maxDistanceFromPlayer)
                {
                    // log("Destination Point is far away from player. Finding another one...");
                    _nullableTargetDestination = null;
                }
            }
        }
        _targetDestination = _nullableTargetDestination ?? Vector3.zero;
        _navMeshAgent.SetDestination(_targetDestination);
        _previousEnemyPosition = _navMeshAgent.transform.position;
        _destinationIsValid = true;
        log("New NavMesh TargetDestination for "+gameObject+" = "+_targetDestination);
    }

    private Vector3? FindNavAgentNextDestination()
    {
        MRUKRoom room = MRUK.Instance?.GetCurrentRoom();
        if (!room)
        {
            Debug.LogError("Cannot spawn enemy because MRUKRoom is absent");
            return null;
        }
        return room.GenerateRandomPositionInRoom(_navMeshAgent.radius*2f, true);
    }
    
    bool DestinationPointIsReachableByNavAgent(Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();
        _navMeshAgent.CalculatePath(targetPosition, path);
        return path.status == NavMeshPathStatus.PathComplete;
    }
    
    //// This is costly and makes Unity Editor to crash
    public bool IsValidNavMeshPosition(Vector3 position, float validationRadius)
    {
        NavMeshHit hit;
        bool hasHit = NavMesh.SamplePosition(position, out hit, validationRadius, NavMesh.AllAreas);
        if(!hasHit) Debug.LogError("Destination Point is not on top of the NavMeshSurface");
        return hasHit;
    }

    
    void Update()
    {
        if (!_gameManager.waveTimerIsActive) return;
        
        // Set height of the flying enemy to approach the destination point, as NavMeshAgent doesn't do this
        float distFromDestination = Vector3.Distance(_navMeshAgent.transform.position, _targetDestination);
        // log("distFromDestination: "+distFromDestination);
        float lerpFactor = 1f - (Vector3.Distance(_navMeshAgent.transform.position, _targetDestination) /
                            Vector3.Distance(_previousEnemyPosition, _targetDestination));
        lerpFactor = Mathf.Abs(lerpFactor);
        
        Vector3 lerpedVector3 = Vector3.Lerp(_previousEnemyPosition, _targetDestination, lerpFactor);
        // log("Lerpfactor: "+lerpFactor);
        // If 1 metre == 1.5 unit of baseOffset, then to find x unit of baseOffset = (x metres * 1.5)/1
        _navMeshAgent.baseOffset = (lerpedVector3.y * 1.5f);

        // Set a new destination if enemy has reached destination
        if (distFromDestination <= _distOffsetB2nEnemyNDestinationPoint)
        {
            SetNewNavAgentDestination();
        }
        // else log("Remaining Dist: "+distFromDestination);
        
        #if UNITY_EDITOR
            XRGizmos.DrawSphere(_targetDestination, _navMeshAgent.radius, Color.green);
        #endif
    }
    
 
    /// Finding if a DestinationPoint is reachable by a NavAgent doesn't 100% work. so these variables and functions are
    /// used as an alternative
    private Vector3 _prevNavAgentPosition = Vector3.zero;
    private int enemyStuckInSamePositionCount = 0;

    private void FixedUpdate()
    {
        CheckIfEnemyIsStuckInSamePosition(_navMeshAgent.transform.position);
    }

    private void CheckIfEnemyIsStuckInSamePosition(Vector3 currentPos)
    {
        if (IsWithinSphere(_prevNavAgentPosition, currentPos, 0.3f))
        {
            Debug.Log("The object is within the sphere radius.");
            enemyStuckInSamePositionCount++;
            if (enemyStuckInSamePositionCount > 10)
            {
                log("Enemy Was Stuck At Position "+currentPos+". Correcting this");
                enemyStuckInSamePositionCount = 0;
                SetNewNavAgentDestination();
            }
        }
        else
        {
            _prevNavAgentPosition = currentPos;
            enemyStuckInSamePositionCount = 0;
        }
        
        // if (_prevNavAgentPosition == currentPos) enemyStuckInSamePositionCount++;
        // else enemyStuckInSamePositionCount = 0;
        //
        // if (enemyStuckInSamePositionCount > 10)
        // {
        //     log("Enemy Was Stuck At Position "+currentPos+". Correcting this");
        //     enemyStuckInSamePositionCount = 0;
        //     SetNewNavAgentDestination();
        // }
        // _prevNavAgentPosition = currentPos;

    }
    
    // Function to check if a position is within a sphere radius
    bool IsWithinSphere(Vector3 position, Vector3 center, float radius)
    {
        float distanceSquared = (position - center).sqrMagnitude;
        float radiusSquared = radius * radius;
        return distanceSquared <= radiusSquared;
    }
    
    
    private void log(string logText){
        string className = this.GetType().Name;
        Debug.Log("["+className+"]  " +logText);
    }

}
