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
        ResourceManger.LoadSceneAsync("Scene_NPRscence", null);
    }

    private IEnumerator InitAndStart()
    {
        yield return ResourceManger.Init();
        yield return  ConfigManager.Init();

        GameStart();
    }

    void Update()
    {
        // 游戏循环
        // 建议玩法循环由这里控制
    }
}
