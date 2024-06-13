using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Experiment369Intro : MonoBehaviour
{
    [SerializeField] private GameObject _experiment369;
    public float experiment369OutOfPortalDelay = 2f;
    private GameManager _gameManager;
    private YoyoDoAnimation _yoyoDoAnimation;

    void Start()
    {
        _experiment369.gameObject.SetActive(false);
        _gameManager = FindObjectOfType<GameManager>();
        _gameManager.onIntroCompleted.AddListener(HideExperiment369);
        _yoyoDoAnimation =  _experiment369.GetComponent<YoyoDoAnimation>();
    }

    public void ShowExperiment369(Vector3 startPos, Vector3 targetPos, Quaternion targetRot)
    {
        _experiment369.SetActive(true);
        _experiment369.transform.position = startPos;
        _experiment369.transform.rotation = targetRot;

        _experiment369.transform.DOMove(targetPos, experiment369OutOfPortalDelay).OnComplete(StartYoyoAnimation);
    }

    private void StartYoyoAnimation()
    {
        _yoyoDoAnimation.UpdateOriginalLocalYPos();
        _yoyoDoAnimation.DoYoyoAnimation();
    }


    private void HideExperiment369()
    {
        _experiment369.SetActive(false);
    }

    void Update()
    {

    }
}
