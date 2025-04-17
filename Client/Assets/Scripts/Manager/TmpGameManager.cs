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
    public TmpGameManager instance;
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
        AudioManager.PlayOneShot(AudioConst.Test);
        TimerManager.Register(1f, 
            () => 
            {
                Debug.Log("Change Scene");
                AudioManager.StopAllSounds();//�ر���������
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
