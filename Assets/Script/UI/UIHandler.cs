﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    public enum UIMenu{
        Main, Play, Pause, Load, Save
    };
    public UIFiniteStateMachine uiFSM { get; private set; }

    public UIEffectHandler uiEffectHandler;

    public GameObject uiPlayGO;
    public GameObject uiPauseGO;
    public GameObject uiMainGO;
    public GameObject uiSaveLoadGO;
    public UIPlayState uiPlayState { get; private set; }
    public UIPauseState uiPauseState { get; private set; }
    public UIMainState uiMainState{get; private set;}

    public GameManager GM;

    void Awake()
    {
        uiFSM = new UIFiniteStateMachine();
        uiPauseState = new UIPauseState(this, uiPauseGO);
        uiPlayState = new UIPlayState(this, uiPlayGO);
        uiMainState = new UIMainState(this, uiMainGO, uiMainGO.GetComponentInChildren<ButtonGroup>());

        GM = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Start()
    {
        uiFSM.InitStateMachine(uiMainState);
    }

    void Update()
    {
        uiFSM.Update();
    }

    /// <summary>
    /// This is a callback function for input of pause action or return action
    /// </summary>
    public void OnInteraction(){
        uiFSM.PeekState().OnInteraction();
    }

    /// <summary>
    /// This is a callback function for input of pause action or return action
    /// </summary>
    public void OnPause(){
        if(uiFSM.PeekState() == uiPlayState){
            uiFSM.SwtichState(uiPauseState);
        }
        else if(uiFSM.Count() > 1){
            uiFSM.PopState();
        }
    }
}
