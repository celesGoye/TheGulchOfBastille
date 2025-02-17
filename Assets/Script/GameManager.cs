﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public UIHandler uiHandler{get; private set;}

    public PlayerCinemaMovement playerCinemaMovement { get; private set; }
    public bool IsDebug = true;

    public float demonSceneCodeInterval = 10f;

    public SceneCode[] demonScenes = {SceneCode.Gulch_Main, SceneCode.Gulch_SunTower, SceneCode.Gulch_Goye};

#region REFERNECES
    public Player player;
#endregion

#region INTERNAL VARIABLES
    private string gameScene;
    public SceneCode currentSceneCode{get; private set;}
    public GameSaver gameSaver{get; private set;}
    public float elapsedSeconds{get; private set;}
#endregion

    public void ReloadGame()
    {
        SceneManager.LoadScene(gameScene);
    }
    void OnEnable(){
        uiHandler = GetComponent<UIHandler>();
        playerCinemaMovement = GetComponent<PlayerCinemaMovement>();

        player = GameObject.Find("/Player").transform.Find("Player").GetComponent<Player>();
        
        gameScene = SceneManager.GetActiveScene().name;

        gameSaver = GetComponent<GameSaver>();

        currentSceneCode = SceneCode.Gulch_Main;

        StartCoroutine(DemonRandomSceneCode());
        elapsedSeconds = 0.0f;
        // DemonSceneCode(demonScenes[Mathf.FloorToInt(Random.Range(0, demonScenes.Length))]);
    }

    public void StartGame() => StartCoroutine(StartGame(gameSaver.isNewGame));

    public void LoadSceneCode(){
        LoadSceneCode(currentSceneCode);
    }

    public void ExitSceneCode(SceneCode sceneCode){
        GameObject sceneGO = GameObject.Find("/Scenes").transform.Find(sceneCode.ToString()).gameObject;
        sceneGO?.SetActive(false);
    }

    public void EnterSceneCode(SceneCode sceneCode){
        GameObject sceneGO = GameObject.Find("/Scenes").transform.Find(sceneCode.ToString()).gameObject;
        sceneGO?.SetActive(true);

        Camera.main.GetComponent<BasicFollower>().cameraClamp = sceneGO.GetComponent<SceneCodeUtil>().CameraClamp;
    }

    public void LoadSceneCode(SceneCode sceneCode){
        ExitSceneCode(currentSceneCode);
        EnterSceneCode(sceneCode);
        currentSceneCode = sceneCode;

        player.playerRuntimeData.currentSceneCode = sceneCode;
    }

    public void OnApplicationQuit(){
        // gameSaver.SaveAll();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ExitGame(){
        gameSaver.SaveAll();
        player.gameObject.SetActive(false);

        ExitSceneCode(currentSceneCode);
        LoadSceneCode(SceneCode.Gulch_SunTower);

        uiHandler.uiFSM.InitStateMachine(uiHandler.uiMainState);
        
    }

    public bool CanPlayerAction()
    {
        if (uiHandler != null)
        {
            return uiHandler.uiFSM.PeekState() == uiHandler.uiPlayState;
        }

        return true;
    }
    IEnumerator StartGame(bool isNewGame){
        StopAllCoroutines();
        ResetAllSceneCode();

        // uiHandler.uiEffectHandler.OnPlayUIEffect(UIEffect.Transition_CrossFade, UIEffectAnimationClip.dark);

        if(isNewGame){
            player.gameObject.SetActive(true);
            var startTrans = GameObject.Find("/Scenes").transform.Find("Gulch_Main/GameStartPoint");
            if(startTrans){
                player.SetPosition(startTrans.position);
            }
            currentSceneCode = SceneCode.Gulch_Main;
        }
        else{
            player.gameObject.SetActive(true);
            gameSaver.LoadAll();
            currentSceneCode = player.playerRuntimeData.currentSceneCode;
            elapsedSeconds = gameSaver.GetSaveSlotMeta(gameSaver.currentSaveSlot).elapsedSeconds;
        }

        Camera.main.GetComponent<BasicFollower>().ClampCamera(player.transform.position);
        Camera.main.GetComponent<BasicFollower>().UpdateCameraFollowing(player.transform);
        LoadSceneCode();
        
        uiHandler.StartGame();

        player.InputHandler.ResetAll();
        yield return new WaitForSeconds(uiHandler.uiEffectHandler.OnPlayUIEffect(UIEffect.Transition_CrossFade, UIEffectAnimationClip.end));
    }

    IEnumerator DemonRandomSceneCode(){
        int index = Random.Range(0, demonScenes.Length);
        while(true){
            yield return new WaitForSeconds(uiHandler.uiEffectHandler.OnPlayUIEffect(UIEffect.Transition_CrossFade, UIEffectAnimationClip.start));
            index = (index + 1) % demonScenes.Length;
            DemonSceneCode(demonScenes[index]);
            yield return new WaitForSeconds(uiHandler.uiEffectHandler.OnPlayUIEffect(UIEffect.Transition_CrossFade, UIEffectAnimationClip.end));
            yield return new WaitForSeconds(demonSceneCodeInterval);
        }
    }

    private void DemonSceneCode(SceneCode sceneCode){
        ResetAllSceneCode();
        Transform centerPoint = GameObject.Find("/Scenes").transform.Find(sceneCode.ToString()).Find("CenterPoint");
        if(centerPoint == null){
            centerPoint = new GameObject("CenterPoint").transform;
            centerPoint.position = Vector3.zero;
            centerPoint.transform.parent = GameObject.Find("/Scenes").transform.Find(sceneCode.ToString());
        }

        EnterSceneCode(sceneCode);
        //Debug.Log(demonScenes[index].ToString());
        Camera.main.transform.position = new Vector3(centerPoint.position.x, centerPoint.position.y, Camera.main.transform.position.z);
        Camera.main.GetComponent<BasicFollower>().UpdateCameraFollowing(centerPoint);
    }

    public void ResetAllSceneCode(){
        GameObject sceneGO = GameObject.Find("/Scenes");
        foreach(SceneCodeUtil util in sceneGO.GetComponentsInChildren<SceneCodeUtil>()){
            util.gameObject.SetActive(false);
        }
    }

    void Update(){
        if(CanPlayerAction()){
            elapsedSeconds += Time.deltaTime;
        }
    }
}
