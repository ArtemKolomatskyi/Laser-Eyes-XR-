/*using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Events;

public enum CurrentWave
{
    Wave1, Wave2, Wave3
}

enum GameOutcome
{
    None, Win, Lose
}

public class GameManager : MonoBehaviour
{
    #region Fields

    [Header("Managers")]
    [SerializeField] private FindSpawnPositions _findSpawnPositions;
    [SerializeField] private PassthroughManager passthroughManager;
    [SerializeField] private EnemySpawnManager _enemySpawnManager;
    [SerializeField] private AlienPortalManager _alienPortalManager;
    [SerializeField] private Hovl_DemoLasers leftEyeLaser;
    [SerializeField] private Hovl_DemoLasers rightEyeLaser;

    [Header("UI Panels")]
    private PanelTransitionSequenceManager panelTransitionManager;

    [Header("Wave Settings")]
    [SerializeField] private int _wave1Enemies = 1;
    [SerializeField] private int _wave2Enemies = 2;
    [SerializeField] private int _wave3Enemies = 3;

    [Header("Timing Settings (in seconds)")]
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
    [SerializeField] private GameObject enemyDestroyEffect;

    [Header("Events")]
    public UnityEvent onIntroStart;
    public UnityEvent onIntroCompleted;
    public UnityEvent onGameStarted;
    public UnityEvent onTriggerAreaSpawned;
    public UnityEvent onPowerUpSpawn;
    public UnityEvent<CurrentWave> onNewWaveDisplay;
    public UnityEvent<CurrentWave> onNewWaveStart;
    public UnityEvent<CurrentWave> onCurrentWaveEnd;
    public UnityEvent onGameOver;
    public UnityEvent onGameWon;
    public UnityEvent onEnemyHit;
    public UnityEvent<GameObject> onEnemyDestroyed;

    private bool gameStarted = false;
    private bool _canShootLaser = false;
    private float _wave1Timer = 0f;
    private float _wave2Timer = 0f;
    private float _wave3Timer = 0f;
    private List<GameObject> _enemies = new List<GameObject>();
    public CurrentWave currentWave { get; private set; } = CurrentWave.Wave1;
    public bool waveTimerIsActive { get; private set; } = false;
    public string timeLeft = "0:00";

    #endregion

    #region Initialization

    void Start()
    {
        ResetGameVariables();
        InitializeEvents();
        panelTransitionManager = FindObjectOfType<PanelTransitionSequenceManager>();
        StartCoroutine(SpawnAlienPortal());
    }

    private void ResetGameVariables()
    {
        gameStarted = false;
        currentWave = CurrentWave.Wave1;
        timeLeft = "0:00";
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
        onIntroCompleted.AddListener(OnIntroCompleted);
        onGameStarted.AddListener(HandleGameStarted);
        onPowerUpSpawn.AddListener(HandlePowerUpSpawn);
        onEnemyHit.AddListener(HandleEnemyHit);
        onEnemyDestroyed.AddListener(HandleEnemyDestroyed);
        onGameOver.AddListener(HandleGameOver);
        onGameWon.AddListener(HandleGameWon);
    }

    private IEnumerator SpawnAlienPortal()
    {
        yield return new WaitForSeconds(gameStartDelay);
        _alienPortalManager.OpenPortal();
        Log("Spawning Alien Portal");
    }

    #endregion

    #region Game Start

    private void OnIntroCompleted()
    {
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(gameStartDelay);
        onGameStarted.Invoke();
    }

    private void HandleGameStarted()
    {
        gameStarted = true;
        backgroundMusicSource.Play();
        passthroughManager.SetActiveLayer(0);
        StartCoroutine(SpawnTriggerAreaCoroutine());
        Log("Game Started");
    }

    private IEnumerator SpawnTriggerAreaCoroutine()
    {
        yield return new WaitForSeconds(triggerAreaSpawnDelay);
        SpawnTriggerArea();
        onTriggerAreaSpawned.Invoke();
    }

    #endregion

    #region Trigger Area

    private void SpawnTriggerArea()
    {
        _findSpawnPositions.SpawnAmount = 1;
        _findSpawnPositions.StartSpawn(MRUK.Instance.GetCurrentRoom());
        Log("Spawned a Trigger Area");
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

    private void HandlePowerUpSpawn()
    {
        passthroughManager.SetActiveLayer(2);
        StartCoroutine(StartWave_AfterDelay());
        Log("User Powered Up");
    }

    private IEnumerator StartWave_AfterDelay()
    {
        yield return new WaitForSeconds(powerUpPassthroughDelay);
        StartCoroutine(StartNextWave());
    }

    #endregion

    #region Waves

    private IEnumerator StartNextWave()
    {
        Log("Starting " + currentWave);
        onNewWaveDisplay.Invoke(currentWave);

        int enemiesCount = _wave1Enemies;
        if (currentWave == CurrentWave.Wave2)
        {
            enemiesCount = _wave2Enemies;
            panelTransitionManager.PlayRoundIndicatorAnimation(2);
        }
        else if (currentWave == CurrentWave.Wave3)
        {
            enemiesCount = _wave3Enemies;
            panelTransitionManager.PlayRoundIndicatorAnimation(3);
        }

        yield return new WaitForSeconds(waveStartDelay);

        _canShootLaser = true;
        _enemySpawnManager.SpawnEnemy(enemiesCount);
        ResetWaveVariables();
        waveTimerIsActive = true;
        onNewWaveStart.Invoke(currentWave);
        Log(currentWave + " started");
    }

    private void CurrentWaveEnded()
    {
        onCurrentWaveEnd.Invoke(currentWave);
        waveTimerIsActive = false;
        SetNextWave();
        StartCoroutine(SpawnTriggerAreaCoroutine());
        Log("Wave " + currentWave + " ended");
    }

    private void SetNextWave()
    {
        if (currentWave == CurrentWave.Wave1)
        {
            currentWave = CurrentWave.Wave2;
            panelTransitionManager.PlayRoundIndicatorAnimation(2);
        }
        else if (currentWave == CurrentWave.Wave2)
        {
            currentWave = CurrentWave.Wave3;
            panelTransitionManager.PlayRoundIndicatorAnimation(3);
        }
    }

    #endregion

    #region Lasers

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
    }

    #endregion

    #region Enemies

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
        Instantiate(enemyDestroyEffect, enemyObj.transform.position, Quaternion.identity);
        _enemies.Remove(enemyObj);
        Destroy(enemyObj);
        FindGameOutcome(false);
        Log("Enemy Destroyed");
    }

    #endregion

    #region Game Outcome

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
        panelTransitionManager.PlayPanel(panelTransitionManager.currentPanel, panelTransitionManager.gameOverPanel);
        Log("Game Over!");
    }

    private void HandleGameWon()
    {
        waveTimerIsActive = false;
        panelTransitionManager.PlayPanel(panelTransitionManager.currentPanel, panelTransitionManager.youWinPanel);
        Log("Game Won!");
    }

    #endregion

    #region Utilities

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

    private void Log(string logText)
    {
        string className = this.GetType().Name;
        Debug.Log("[" + className + "] " + logText);
    }

    #endregion
}*/
