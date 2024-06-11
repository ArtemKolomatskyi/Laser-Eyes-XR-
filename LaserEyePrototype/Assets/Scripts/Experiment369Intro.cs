using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experiment369Intro : MonoBehaviour
{
    [SerializeField] private GameObject _experiment369;
    private GameManager _gameManager;

    void Start()
    {
        _experiment369.gameObject.SetActive(false);
        _gameManager = FindObjectOfType<GameManager>();
        _gameManager.onIntroCompleted.AddListener(HideExperiment369);
    }

    public void ShowExperiment369(Vector3 pos, Quaternion rot)
    {
        _experiment369.SetActive(true);
        _experiment369.transform.position = pos;
        _experiment369.transform.rotation = rot;
        _experiment369.GetComponent<YoyoDoAnimation>().UpdateOriginalLocalYPos();
    }
    
    private void HideExperiment369()
    {
        _experiment369.SetActive(false);
    }

    void Update()
    {

    }
}
