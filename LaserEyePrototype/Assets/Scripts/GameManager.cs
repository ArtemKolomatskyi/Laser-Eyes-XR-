using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PassthroughManager passthroughManager;
    [SerializeField] private GameObject enemyObj;
    [SerializeField] private GameObject leftEyeLaser;
    [SerializeField] private GameObject rightEyeLaser;
    [SerializeField] private GameObject alienPortal;
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private float gameStartDelay = 3.0f;
    [SerializeField] private Collider gameStartTriggerArea;

    public UnityEvent onGameStarted;
    public UnityEvent onGameOver;
    public UnityEvent onPowerUpSpawn;

    private bool gameStarted = false;

    void Start()
    {
        // Initialize the game manager
        backgroundMusic.Play();
        onGameStarted.AddListener(HandleGameStarted);
        onGameOver.AddListener(HandleGameOver);
        onPowerUpSpawn.AddListener(HandlePowerUpSpawn);
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
        alienPortal.SetActive(true);
        passthroughManager.SetActiveLayer(0);
        // Additional logic for starting the game
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