using System;
using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.TTS.Utilities;
using UnityEngine;

public class TTSSpeakerManager : MonoBehaviour
{
    [SerializeField] private TTSSpeaker _ttsSpeaker;
    public static TTSSpeakerManager Instance { get; internal set; }     //Or private set
    private GameManager _gameManager;

    public string introText1 =
        "Hello, human! My name is Experiment 369. I’ve just escaped from a lab run by evil scientists who plan to unleash a terrifying weapon upon the world: powerful Evil Kamikaze Nuke Drones. I need your help to stop them!";
    public string introText2 = "Oh no, they’ve found me! They’ll be here soon!";
    public string introText3 = "Listen closely, as time is short. I can’t fight, but I can grant you a special ability—laser eyes!";
    public string introText4 = "Whenever you find a blue-lit area in your room, run to it. Standing in the blue light will give you a 15-second superpower. Use your laser eyes to fight off the invading aliens and protect our world.";
    public string introText5 = "Remember, your superpower only affects the aliens; your home remains safe from harm.";
    public string introText6 = "Prepare yourself, brave human. The fate of the world is in your hands!";
    
    public string wave1Text = "Wave 1";
    public string wave2Text = "Wave 2";
    public string wave3Text = "Wave 3";
    
    private bool _isSpeaking = false;

    private void Awake()
    {
        if (Instance != null)
        {
            // As long as you aren't creating multiple NetworkManager instances, throw an exception.
            // (***the current position of the callstack will stop here***)
            throw new Exception($"Detected more than one instance of {nameof(TTSSpeakerManager)} on {nameof(gameObject)}!");
        }
        Instance = this;
    }
    
    private void Start(){
        if (Instance != this){
            return; // so things don't get even more broken if this is a duplicate
        }
        
        _gameManager = FindObjectOfType<GameManager>();
        _gameManager.onIntroStart.AddListener(Speak_IntroText);
        _gameManager.onIntroCompleted.AddListener(StopIntro);
        _gameManager.onNewWaveDisplay.AddListener(Speak_CurrentWave);
    }

    public void Speak_CurrentWave(CurrentWave currentWave)
    {
        log("TTSSpeaker Current_Wave Start");
        if(currentWave == CurrentWave.Wave1) _ttsSpeaker.Speak(wave1Text);
        else if(currentWave == CurrentWave.Wave2) _ttsSpeaker.Speak(wave2Text);
        else _ttsSpeaker.Speak(wave3Text);
    }
    
    public void Speak_IntroText()
    {
        log("TTSSpeaker Intro Start");
        _ttsSpeaker.SpeakQueued(introText1);
        _ttsSpeaker.SpeakQueued(introText2);
        _ttsSpeaker.SpeakQueued(introText3);
        _ttsSpeaker.SpeakQueued(introText4);
        _ttsSpeaker.SpeakQueued(introText5);
        _ttsSpeaker.SpeakQueued(introText6);
        _ttsSpeaker.Events.OnPlaybackQueueComplete.AddListener(IntroductionCompleted);
    }

    private void StopIntro()
    {
        _ttsSpeaker.Stop();
        _ttsSpeaker.QueuedClips.Clear();
        log("Stopping TTSSpeaker and clearing Queued Clips");
    }
    
    private void IntroductionCompleted()
    {
        _gameManager.onIntroCompleted.Invoke();
        log("TTSSpeaker Completed Queue");
    }
    
    private void log(string logText){
        string className = this.GetType().Name;
        Debug.Log("["+className+"]  " +logText);
    }


}
