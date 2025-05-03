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
/// �ҵ�����������gameObject��
/// </summary>
public class TmpGameManager : MonoBehaviour
{
    public static TmpGameManager instance;
    public EGameState gameState;

    void Start()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

        // ����ģ��ĳ�ʼ��
        StartCoroutine("InitAndStart");
    }

    /// <summary>
    /// ��Ϸ���
    /// </summary>
    private void GameStart()
    {
        Debug.Log("Game Start!");
        // ��Ϸ��ʼ���߼� ��ʾ���˵���  

        //test
        //AudioManager.PlayOneShot(AudioConst.Test);
        //TimerManager.Register(1f, 
        //    () => 
        //    {
        //Debug.Log("Change Scene");
        //AudioManager.StopAllSounds();//�ر���������

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
    /// ������Ϸ
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
    /// ����
    /// </summary>
    public void GameOver()
    {
        UIManager.Instance.Show("UIGameOver");
        gameState = EGameState.Pause;
    }

    /// <summary>
    /// ���ùؿ�
    /// </summary>
    public void ResetLevel()
    {
        UIManager.Instance.CloseByName("UIGamePause");
        PlayerManager.instance.Damage(EDamageType.CustomValue, Mathf.Infinity);
        LevelManager.instance.ResetLevel();
        gameState = EGameState.Play;
    }

    /// <summary>
    /// ����������
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
    /// ������ͣ���ص���Ϸ
    /// </summary>
    public void Resume()
    {
        UIManager.Instance.CloseByName("UIGamePause");
        gameState = EGameState.Play;
    }

    /// <summary>
    /// ��Ϸ��ͣ
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

        // ��Ϸѭ��
        PlayerManager.instance.PlayerUpdate();
    }

    void FixedUpdate()
    {
        if (gameState != EGameState.Play)
            return;

        // ��Ϸѭ��
        PlayerManager.instance.PlayerFixedUpdate();
    }
}
