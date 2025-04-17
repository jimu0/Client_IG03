using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EGameState
{
    Menu,
    Play,
    Pause,
}

/// <summary>
/// 挂到启动场景内gameObject上
/// </summary>
public class TmpGameManager : MonoBehaviour
{
    public TmpGameManager instance;
    public EGameState gameState;

    void Start()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

        // 各个模块的初始化
        StartCoroutine("InitAndStart");
    }

    /// <summary>
    /// 游戏入口
    /// </summary>
    private void GameStart()
    {
        Debug.Log("Game Start!");
        // 游戏开始的逻辑 显示主菜单等  

        //test
        AudioManager.PlayOneShot(AudioConst.Test);
        TimerManager.Register(1f, 
            () => 
            {
                Debug.Log("Change Scene");
                AudioManager.StopAllSounds();//关闭声音总线
                StartPlay();

                //Debug.Log("Start Story");
                //StoryManager.StartStory(10000);
                //StoryManager.StartStory(10002);
            });
    }

    public void StartPlay()
    {
        gameState = EGameState.Play;
        ResourceManger.LoadSceneAsync("Scene_GamePlayScene", () =>
        {
            LevelManager.instance.EnterLevel("Level_1");
        });
    }

    public void Resume()
    {
        gameState = EGameState.Play;
    }

    public void PauseOrNot()
    {
        gameState = EGameState.Pause;
    }

    private IEnumerator InitAndStart()
    {
        yield return ResourceManger.Init();
        yield return  ConfigManager.Init();

        AudioManager.Init();
        TimerManager.Init();
        StoryManager.Init();
        WorldStateManager.Init();

        GameStart();
    }

    void Update()
    {
        if (gameState != EGameState.Play)
            return;

        // 游戏循环
        PlayerManager.instance.PlayerUpdate();
    }

    void FixedUpdate()
    {
        if (gameState != EGameState.Play)
            return;

        // 游戏循环
        PlayerManager.instance.PlayerFixedUpdate();
    }
}
