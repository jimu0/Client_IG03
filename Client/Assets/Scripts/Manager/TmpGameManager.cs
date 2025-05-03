using System;
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
    public static TmpGameManager instance;
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
        //AudioManager.PlayOneShot(AudioConst.Test);
        //TimerManager.Register(1f, 
        //    () => 
        //    {
        //Debug.Log("Change Scene");
        //AudioManager.StopAllSounds();//关闭声音总线

        //WorldStateManager.State.SetValue("test", "2");
        //WorldStateManager.SaveToFile();
        
        ResourceManger.LoadSceneAsync("Scene_GamePlayScene", () =>
        {
            UIManager.Instance.Show("UIMainMenu");
        });
                //Debug.Log("Start Story");
                //StoryManager.StartStory(10000);
                //StoryManager.StartStory(10002);
            //});
    }

    public void Exit()
    {
        Application.Quit();
    }

    /// <summary>
    /// 进入游戏
    /// </summary>
    public void StartPlay()
    {
        gameState = EGameState.Play;

        UIManager.Instance.CloseByName("UIMainMenu");
        UIManager.Instance.Show("UIGame");
      
        LevelManager.instance.EnterLevel(LevelManager.instance.GetPlayLevelIndex());
    }

    public void StartPlay(string levelName)
    {
        gameState = EGameState.Play;
        UIManager.Instance.CloseByName("UIMainMenu");
        UIManager.Instance.Show("UIGame");

        LevelManager.instance.EnterLevel(levelName);
    }

    /// <summary>
    /// 死亡
    /// </summary>
    public void GameOver()
    {
        UIManager.Instance.Show("UIGameOver");
        gameState = EGameState.Pause;
    }

    /// <summary>
    /// 重置关卡
    /// </summary>
    public void ResetLevel()
    {
        UIManager.Instance.CloseByName("UIGamePause");
        PlayerManager.instance.Damage(EDamageType.CustomValue, Mathf.Infinity);
        LevelManager.instance.ResetLevel();
        gameState = EGameState.Play;
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    public void BackToMenu()
    {
        AudioManager.StopLevelBGM();
        UIManager.Instance.CloseByName("UIGame");
        UIManager.Instance.Show("UIMainMenu");
        LevelManager.instance.UnloadAllLevel();
        gameState = EGameState.Menu;
    }

    /// <summary>
    /// （从暂停）回到游戏
    /// </summary>
    public void Resume()
    {
        UIManager.Instance.CloseByName("UIGamePause");
        gameState = EGameState.Play;
    }

    /// <summary>
    /// 游戏暂停
    /// </summary>
    public void Pause()
    {
        UIManager.Instance.Show("UIGamePause");
        gameState = EGameState.Pause;
    }

    public void StartTimeLine()
    {
        gameState = EGameState.Pause;
    }

    public void EndTimeLine()
    {
        gameState = EGameState.Play;
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

        if (Input.GetKeyDown(KeyCode.Escape))
            Pause();

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
