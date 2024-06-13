using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PanelTransitionSequenceManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    
    [Header("UI Panels")]
    public GameObject laserWelcomePanel;
    public GameObject reverseWelcomePanel;
    public GameObject instructionsPanel;
    public GameObject reverseInstructionsPanel;
    public GameObject laserActivatedPanel;
    public GameObject reverseLaserActivatedPanel;
    public GameObject roundOnePanel;
    public GameObject roundTwoPanel;
    public GameObject roundThreePanel;
    public GameObject gameOverPanel;
    public GameObject youWinPanel;

    [Header("Test Mode")]
    public bool testMode = false;
    public bool integratedReverseMode = false;
    public bool consolidatedTestMode = false;
    public KeyCode youWinKey = KeyCode.Y;
    public KeyCode gameOverKey = KeyCode.B;
    public KeyCode reverseWelcomeKey = KeyCode.G;
    public KeyCode reverseInstructionsKey = KeyCode.O;
    public KeyCode reverseLaserActivatedKey = KeyCode.K;
    public KeyCode laserWelcomeKey = KeyCode.H;
    public KeyCode instructionsKey = KeyCode.I;
    public KeyCode laserActivatedKey = KeyCode.L;
    public KeyCode customTransitionKey1 = KeyCode.X;
    public KeyCode customTransitionKey2 = KeyCode.C;
    public KeyCode nextPanelKey = KeyCode.V;

    private GameObject currentPanel;
    private bool awaitingInput = false;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        DeactivateAllPanels();
        laserWelcomePanel.SetActive(true); // Start with LaserWelcome panel active
        currentPanel = laserWelcomePanel;
    }

    void Start()
    {
        gameManager.onGameWon.AddListener(() => PlayPanel(currentPanel, youWinPanel));
        gameManager.onGameOver.AddListener(() =>PlayPanel(currentPanel, gameOverPanel));
        gameManager.onIntroCompleted.AddListener(() =>PlayPanel(currentPanel, laserWelcomePanel));
        gameManager.onGameStarted.AddListener(() =>PlayPanel(currentPanel, instructionsPanel));
        gameManager.onTriggerAreaSpawned.AddListener(() =>PlayPanel(currentPanel, laserActivatedPanel));
    }

    private void Update()
    {
        if (testMode)
        {
            HandleTestModeInput();
        }

        if (awaitingInput && Input.GetKeyDown(nextPanelKey))
        {
            awaitingInput = false;
            AdvancePanelSequence();
        }

        // Check for custom transitions from non-reverse panels to their reverse counterparts
        if (Input.GetKeyDown(customTransitionKey1))
        {
            HandleCustomTransitionKey1();
        }

        // Check for transition from ReverseLaserActivated to the rounds
        if (currentPanel == reverseLaserActivatedPanel && Input.GetKeyDown(customTransitionKey2))
        {
            PlayPanel(currentPanel, roundOnePanel);
            awaitingInput = false; // End of sequence, no longer awaiting input
        }
    }

    private void HandleTestModeInput()
    {
        if (Input.GetKeyDown(youWinKey)) PlayPanel(currentPanel, youWinPanel);
        if (Input.GetKeyDown(gameOverKey)) PlayPanel(currentPanel, gameOverPanel);
        if (Input.GetKeyDown(reverseWelcomeKey)) PlayPanel(currentPanel, reverseWelcomePanel);
        if (Input.GetKeyDown(reverseInstructionsKey)) PlayPanel(currentPanel, reverseInstructionsPanel);
        if (Input.GetKeyDown(reverseLaserActivatedKey)) PlayPanel(currentPanel, reverseLaserActivatedPanel);
        if (Input.GetKeyDown(laserWelcomeKey)) PlayPanel(currentPanel, laserWelcomePanel);
        if (Input.GetKeyDown(instructionsKey)) PlayPanel(currentPanel, instructionsPanel);
        if (Input.GetKeyDown(laserActivatedKey)) PlayPanel(currentPanel, laserActivatedPanel);
        if (Input.GetKeyDown(customTransitionKey1)) HandleCustomTransitionKey1();
        if (Input.GetKeyDown(customTransitionKey2)) HandleCustomTransitionKey2();
    }

    private void HandleCustomTransitionKey1()
    {
        if (currentPanel == laserWelcomePanel)
        {
            PlayPanel(currentPanel, reverseWelcomePanel);
            StartCoroutine(WaitAndAdvance(reverseWelcomePanel, instructionsPanel));
        }
        else if (currentPanel == instructionsPanel)
        {
            PlayPanel(currentPanel, reverseInstructionsPanel);
            StartCoroutine(WaitAndAdvance(reverseInstructionsPanel, laserActivatedPanel));
        }
        else if (currentPanel == laserActivatedPanel)
        {
            PlayPanel(currentPanel, reverseLaserActivatedPanel);
            awaitingInput = true; // Wait for customTransitionKey2 to be pressed to move to the rounds
        }
    }

    private void HandleCustomTransitionKey2()
    {
        // Only handle this transition if currently in reverseLaserActivatedPanel
        if (currentPanel == reverseLaserActivatedPanel)
        {
            PlayPanel(currentPanel, roundOnePanel);
            awaitingInput = false; // End of sequence, no longer awaiting input
        }
    }

    private void AdvancePanelSequence()
    {
        if (currentPanel == laserWelcomePanel)
        {
            PlayPanel(currentPanel, reverseWelcomePanel);
            StartCoroutine(WaitAndAdvance(reverseWelcomePanel, instructionsPanel));
        }
        else if (currentPanel == reverseWelcomePanel)
        {
            PlayPanel(currentPanel, instructionsPanel);
            awaitingInput = true; // Wait for input to move to ReverseInstructions
        }
        else if (currentPanel == instructionsPanel)
        {
            awaitingInput = true; // Wait for customTransitionKey1 to be pressed
        }
        else if (currentPanel == reverseInstructionsPanel)
        {
            PlayPanel(currentPanel, laserActivatedPanel);
        }
        else if (currentPanel == laserActivatedPanel)
        {
            awaitingInput = true; // Wait for customTransitionKey1 to be pressed
        }
        else if (currentPanel == reverseLaserActivatedPanel)
        {
            // Wait for customTransitionKey2 to be pressed
            awaitingInput = true;
        }
    }

    private IEnumerator WaitAndAdvance(GameObject fromPanel, GameObject toPanel)
    {
        yield return new WaitForSeconds(1f); // Adjust delay as needed for animation completion
        if (currentPanel == fromPanel)
        {
            PlayPanel(currentPanel, toPanel);
        }
    }

    public void PlayRoundIndicatorAnimation(int roundNumber)
    {
        switch (roundNumber)
        {
            case 1:
                PlayPanel(currentPanel, roundOnePanel);
                break;
            case 2:
                PlayPanel(currentPanel, roundTwoPanel);
                break;
            case 3:
                PlayPanel(currentPanel, roundThreePanel);
                break;
            default:
                Debug.LogWarning("Invalid round number.");
                break;
        }
    }

    private IEnumerator PlaySequentialPanels(List<GameObject> panels, bool startRoundsAfter = false)
    {
        foreach (var panel in panels)
        {
            PlayPanel(currentPanel, panel);
            yield return new WaitForSeconds(1f); // Adjust delay as needed for animation completion
        }

        if (startRoundsAfter)
        {
            FindObjectOfType<GameManager>().OnIntroCompleted(); // Start the round after the sequence
        }
    }

    public void PlayPanel(GameObject currentPanel, GameObject nextPanel)
    {
        StartCoroutine(SwitchPanels(currentPanel, nextPanel));
    }

    private IEnumerator SwitchPanels(GameObject currentPanel, GameObject nextPanel)
    {
        if (nextPanel != null)
        {
            nextPanel.SetActive(true);
            yield return new WaitForSeconds(0.1f); // Adjust timing if necessary
        }

        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }

        this.currentPanel = nextPanel;
    }

    private void DeactivateAllPanels()
    {
        laserWelcomePanel.SetActive(false);
        reverseWelcomePanel.SetActive(false);
        instructionsPanel.SetActive(false);
        reverseInstructionsPanel.SetActive(false);
        laserActivatedPanel.SetActive(false);
        reverseLaserActivatedPanel.SetActive(false);
        roundOnePanel.SetActive(false);
        roundTwoPanel.SetActive(false);
        roundThreePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        youWinPanel.SetActive(false);
    }
}
