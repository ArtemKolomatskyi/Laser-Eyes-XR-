using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PassthroughManager passthroughManager;
    [SerializeField] private GameObject enemyObj;
    [SerializeField] private GameObject enemyDestroyEffect;
    [SerializeField] private GameObject leftEyeLaser;
    [SerializeField] private GameObject rightEyeLaser;
    [SerializeField] private GameObject alienPortal;
    
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioSource enemyHitSound;

    [SerializeField] private AudioClip enemyTakingDamage;
    
    [SerializeField] private float gameStartDelay = 3.0f;
    [SerializeField] private Collider gameStartTriggerArea;

    public UnityEvent onGameStarted;
    public UnityEvent onGameOver;
    public UnityEvent onPowerUpSpawn;

    public UnityEvent onEnemyHit;
    public UnityEvent onEnemyDestroyed;

    private bool gameStarted = false;

    void Start()
    {
        // Initialize the game manager
        onGameStarted.AddListener(HandleGameStarted);
        onGameOver.AddListener(HandleGameOver);
        onPowerUpSpawn.AddListener(HandlePowerUpSpawn);
        onEnemyHit.AddListener(HandleEnemyHit);
        onEnemyDestroyed.AddListener(HandleEnemyDestroyed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!gameStarted && other == gameStartTriggerArea)
        {
            StartCoroutine(StartGameCoroutine());
        }
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
        backgroundMusic.Play();
        //alienPortal.SetActive(true);
        
        // Add event listeners
        passthroughManager.SetActiveLayer(0);
        
        // Additional logic for starting the game
    }

    private void HandleEnemyHit()
    {
        enemyHitSound.Play();
    }

    private void HandleEnemyDestroyed()
    {
        // Instantiate the enemy destroy effect at the enemy's position
        Instantiate(enemyDestroyEffect, enemyObj.transform.position, Quaternion.identity);
        
        // Play destruction sfx
        // Display next wave counter
    }

    private void HandleGameOver()
    {
        passthroughManager.SetActiveLayer(1);
        // Additional logic for game over
    }

    private void HandlePowerUpSpawn()
    {
        passthroughManager.SetActiveLayer(2);
        // Additional logic for powerup spawn
    }
}