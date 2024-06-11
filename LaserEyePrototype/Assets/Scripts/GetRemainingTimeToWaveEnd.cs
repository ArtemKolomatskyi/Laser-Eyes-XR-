using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetRemainingTimeToWaveEnd : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tmpPro;
    private GameManager _gameManager;
    
    void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        _tmpPro.text = _gameManager.timeLeft;
    }
}
