using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public enum CurrentWave
{
    Wave1, Wave2, Wave3
}
enum GameOutcome
{
    None, Win, Lose
}

/// <summary>
/// NOTE: Currently, a power up is spawned before a wave
/// </summary>

public class GameManager : MonoBehaviour
{
    [SerializeField] private FindSpawnPositions _findSpawnPositions;
    [SerializeField] private PassthroughManager passthroughManager;
    [SerializeField] private EnemySpawnManager _enemySpawnManager;
    [SerializeField] private AlienPortalManager _alienPortalManager;
    [SerializeField] private Hovl_DemoLasers leftEyeLaser;
    [SerializeField] private Hovl_DemoLasers rightEyeLaser;
    [SerializeField] private int _wave1Enemies = 1;
    [SerializeField] private int _wave2Enemies = 2;
    [SerializeField] private int _wave3Enemies = 3;
    [SerializeField] private GameObject enemyDestroyEffect;

    public string timeLeft = "0:00";
    
    [Header("Time Delays (in seconds)")]
    [SerializeField] private float gameStartDelay = 3.0f;
    [SerializeField] private float triggerAreaSpawnDelay = 1.0f;
    [SerializeField] private float powerUpPassthroughDelay = 2.0f;
    [SerializeField] private float waveStartDelay = 2.0f;
    [SerializeField] private float _wave1Time = 3f;
    [SerializeField] private float _wave2Time = 5f;
    [SerializeField] private float _wave3Time = 7f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource backgroundMusicSource;
    public AudioSource enemyHitSoundSource;
    [SerializeField] private AudioClip enemyTakingDamage_AudioClip;


    [Header("Events")]
    public UnityEvent onIntroStart;
    // public UnityEvent onIntroSkipped;
    public UnityEvent onIntroCompleted;
    public UnityEvent onGameStarted;
    public UnityEvent onTriggerAreaSpawned;
    public UnityEvent onPowerUpSpawn;
    [Tooltip("When user is informed that a new wave is starting")]
    public UnityEvent<CurrentWave> onNewWaveDisplay; 
    [Tooltip("When the new wave actually starts")]
    public UnityEvent<CurrentWave> onNewWaveStart; 
    [Tooltip("When the wave ends")]
    public UnityEvent<CurrentWave> onCurrentWaveEnd; 
    public UnityEvent onGameOver;
    public UnityEvent onGameWon;
    public UnityEvent onEnemyHit;
    public UnityEvent<GameObject> onEnemyDestroyed;

    private bool gameStarted = false;
    private bool _canShootLaser = false;
    public CurrentWave currentWave { get; private set; } = CurrentWave.Wave1;
    private float _wave1Timer = 0f;
    private float _wave2Timer = 0f;
    private float _wave3Timer = 0f;
    public bool waveTimerIsActive { get; private set; } = false;
    private List<GameObject> _enemies = new List<GameObject>();

    private bool _introCompleted = false;
    // public CurrentWave currentWave = CurrentWave.Wave1;


    
    #region INIT
    void Start()
    {
        ResetGameVariables();
        InitializeEvents();
        currentWave = CurrentWave.Wave1;
        StartCoroutine(SpawnAlienPortal());
    }
    
    private void ResetGameVariables()
    {
        gameStarted = false;
        currentWave = CurrentWave.Wave1;
        timeLeft = "0:00";
        _introCompleted = false;
    }
    
    private void ResetWaveVariables()
    {
        _wave1Timer = 0f;
        _wave2Timer = 0f;
        _wave3Timer = 0f;
        waveTimerIsActive = false;
    }

    private void InitializeEvents()
    {
        // Initialize the game manager
        onIntroCompleted.AddListener(OnIntroCompleted);
        onGameStarted.AddListener(HandleGameStarted);
        onPowerUpSpawn.AddListener(HandlePowerUpSpawn);
        onEnemyHit.AddListener(HandleEnemyHit);
        onEnemyDestroyed.AddListener(HandleEnemyDestroyed);
        onGameOver.AddListener(HandleGameOver);
        onGameWon.AddListener(HandleGameWon);
    }

    //Todo: spawn on vertical surface
    private IEnumerator SpawnAlienPortal()
    {
        // Countdown before spawning the alien Portal
        yield return new WaitForSeconds(gameStartDelay);
        _alienPortalManager.OpenPortal();
        log("Spawning Alien Portal");
    }

    public void SkipIntro()
    {
        onIntroCompleted.Invoke();
    }
    #endregion

    
    #region GAME START
    public void OnIntroCompleted()
    {
        if(_introCompleted) return;
        _introCompleted = true;
        StartCoroutine(StartGameCoroutine());
    }
    
    private IEnumerator StartGameCoroutine()
    {
        // Countdown before starting the game
        yield return new WaitForSeconds(gameStartDelay);
        onGameStarted.Invoke();
    }

    private void HandleGameStarted()
    {
        gameStarted = true;
        backgroundMusicSource.Play();
        passthroughManager.SetActiveLayer(0);
        // StartCoroutine(SpawnTriggerAreaCoroutine());
        StartCoroutine(StartNextWave());
        log("Game Started");
    }
    
    private IEnumerator SpawnTriggerAreaCoroutine()
    {
        yield return new WaitForSeconds(triggerAreaSpawnDelay);
        SpawnTriggerArea();
        onTriggerAreaSpawned.Invoke();
    }
    #endregion

    

    #region TRIGGER AREA
    private void SpawnTriggerArea()
    {
        _findSpawnPositions.SpawnAmount = 1;
        _findSpawnPositions.StartSpawn(MRUK.Instance.GetCurrentRoom()); 
        log("Spawned a Trigger Area");
    }
    
    public void OnUserEnteredTriggerArea(GameObject gameObj)
    {
        Destroy(gameObj);
        OnUserEnteredTriggerArea();
    }
    
    public void OnUserEnteredTriggerArea()
    {
        onPowerUpSpawn.Invoke();
    }

    // NOTE: Currently, a power up is spawned before a wave
    private void HandlePowerUpSpawn()
    {
        passthroughManager.SetActiveLayer(2);
        
        StartCoroutine(StartWave_AfterDelay());
        log("User Powered Up");
        // Additional logic for powerup spawn
    }
    
    private IEnumerator StartWave_AfterDelay()
    {
        yield return new WaitForSeconds(powerUpPassthroughDelay);
        StartCoroutine(StartNextWave());
    }
    #endregion

    
    
    #region WAVES
    private IEnumerator StartNextWave()
    {
        log("Starting "+currentWave);
        onNewWaveDisplay.Invoke(currentWave);
        int enemiesCount = _wave1Enemies;
        if (currentWave == CurrentWave.Wave2)
        {
            enemiesCount = _wave2Enemies;
        }
        else if (currentWave == CurrentWave.Wave3)
        {
            enemiesCount = _wave3Enemies;
        }
        
        yield return new WaitForSeconds(waveStartDelay);
        
        _canShootLaser = true;
        _enemySpawnManager.SpawnEnemy(enemiesCount);
        ResetWaveVariables();
        waveTimerIsActive = true;
        onNewWaveStart.Invoke(currentWave);
        log(currentWave+" started");
    }

    private void CurrentWaveEnded()
    {
        onCurrentWaveEnd.Invoke(currentWave);
        waveTimerIsActive = false;
        SetNextWave();
        // StartCoroutine(SpawnTriggerAreaCoroutine());
        StartCoroutine(StartNextWave());
        log("Wave "+currentWave+" ended");
    }

    private void SetNextWave()
    {
        if (currentWave == CurrentWave.Wave1) currentWave = CurrentWave.Wave2;
        else if (currentWave == CurrentWave.Wave2) currentWave = CurrentWave.Wave3;
    }
    #endregion


    #region LASERS
    public void StartShootingLaser()
    {
        if (_canShootLaser)
        {
            leftEyeLaser.StartShootLaser();
            rightEyeLaser.StartShootLaser();
        }
    }
    
    public void StopShootingLaser()
    {
        leftEyeLaser.StopShootingLaser();
        rightEyeLaser.StopShootingLaser();
        enemyHitSoundSource.Stop();
    }
    #endregion


    #region ENEMIES
    private void HandleEnemyHit()
    {
        enemyHitSoundSource.Play();
    }

    public void EnemySpawned(GameObject enemy)
    {
        _enemies.Add(enemy);
    }
    
    private void HandleEnemyDestroyed(GameObject enemyObj)
    {
        // Instantiate the enemy destroy effect at the enemy's position
        Instantiate(enemyDestroyEffect, enemyObj.transform.position, Quaternion.identity);
        _enemies.Remove(enemyObj.gameObject);
        Destroy(enemyObj.gameObject);
        FindGameOutcome(false);
        log("Enemy Destroyed");
  
        // Play destruction sfx
        // Display next wave counter
    }
    #endregion


    #region GAME OUTCOME
    private GameOutcome FindGameOutcome(bool timeIsUp)
    {
        if (_enemies.Count == 0)
        {
            if (currentWave == CurrentWave.Wave3)
            {
                onGameWon.Invoke();
                return GameOutcome.Win;
            }
            else
            {
                CurrentWaveEnded();
            }
        }  
        
        // _enemies Count > 0
        if (timeIsUp)
        {
            onGameOver.Invoke();
            return GameOutcome.Lose;
        }

        return GameOutcome.None;
    }
    
    private void HandleGameOver()
    {
        passthroughManager.SetActiveLayer(1);
        waveTimerIsActive = false;
        log("Game Over!");
        // Additional logic for game over
    }
    
    private void HandleGameWon()
    {
        //todo: passthrough for game Won
        
        waveTimerIsActive = false;
        log("Game Won!");
        // Additional logic for game Won
    }
    #endregion


    private void ReplayGame()
    {
        log("Replaying Game");
        ResetGameVariables();
    }



    private void Update()
    {
        if (!waveTimerIsActive) return;
        if (currentWave == CurrentWave.Wave1)
        {
            _wave1Timer += Time.deltaTime;
            FormatDisplayTime(_wave1Time - _wave1Timer);
            if (_wave1Timer > _wave1Time)
            {
                FindGameOutcome(true);
            }
        }
        else if (currentWave == CurrentWave.Wave2)
        {
            _wave2Timer += Time.deltaTime;
            FormatDisplayTime(_wave2Time - _wave2Timer);
            if (_wave2Timer > _wave2Time)
            {
                FindGameOutcome(true);
            }
        }
        else if (currentWave == CurrentWave.Wave3)
        {
            _wave3Timer += Time.deltaTime;
            FormatDisplayTime(_wave3Time - _wave3Timer);
            if (_wave3Timer > _wave3Time)
            {
                FindGameOutcome(true);
            }
        }
    }

    private void FormatDisplayTime(float seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.Abs(seconds));
        timeLeft = string.Format("{0}:{1:D2}", (int)timeSpan.TotalMinutes, timeSpan.Seconds);
    }

    private void log(string logText){
        string className = this.GetType().Name;
        Debug.Log("["+className+"]  " +logText);
    }


}
