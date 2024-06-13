using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAreaDetector : MonoBehaviour
{
    private GameManager _gameManager;
    [SerializeField] private Collider _gameStartTriggerArea;

    void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collider Trigger entered");
        
        if (other == _gameStartTriggerArea)
        {
            _gameManager.OnUserEnteredTriggerArea(other.gameObject);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))  _gameManager.OnUserEnteredTriggerArea();
    }
}
