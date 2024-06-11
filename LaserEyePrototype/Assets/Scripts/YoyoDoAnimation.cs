using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class YoyoDoAnimation : MonoBehaviour
{
    [SerializeField] private float _moveDistance = 0.3f; // Distance to move up and down
    [SerializeField] private float _duration = 1f; // Duration for one up and down cycle
    private float _moveDist;
    private float _originalLocalYPos;
    private bool _animate = false;

    void OnEnable()
    {
        _moveDist = _moveDistance;
        _originalLocalYPos = transform.localPosition.y;
        _animate = true;
        DoAnimation();
    }
    
    // Loop infinitely with a Yoyo loop type
    void DoAnimation()
    {
        if(!_animate) return;
        _moveDist = -_moveDist;
        transform.DOLocalMoveY((_originalLocalYPos + _moveDist), _duration).OnComplete(DoAnimation); 
    }

    /// <summary>
    /// This y position is altered by <see cref="Experiment369Intro"/> when setting this gameObject to active
    /// However, we have to manually update <see cref="Experiment369Intro"/> as it new y position doesn't reflect
    /// when <see cref="OnEnable"/> is called
    /// </summary>
    public void UpdateOriginalLocalYPos()
    {
        _originalLocalYPos = transform.localPosition.y;
    }

    private void OnDisable()
    {
        _animate = false;
    }
}
