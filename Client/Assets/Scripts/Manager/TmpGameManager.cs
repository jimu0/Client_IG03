using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂到启动场景内gameObject上
/// </summary>
public class TmpGameManager : MonoBehaviour
{
    void Start()
    {
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
                //Debug.Log("Change Scene");
                //AudioManager.StopAllSounds();//关闭声音总线
                //ResourceManger.LoadSceneAsync("Scene_UIScene", null);

                Debug.Log("Start Story");
                StoryManager.StartStory(10000);
                StoryManager.StartStory(10002);
            });
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
        // 游戏循环
        // 建议玩法循环由这里控制
    }
}
