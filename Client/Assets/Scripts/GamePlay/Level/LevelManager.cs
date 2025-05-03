using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Numerics;
using Scripts.TimelineControl.SceneFX;

[Serializable]
public struct LevelCfg
{
    /// <summary>
    /// 关卡场景名
    /// </summary>
    [Header("关卡场景名")]
    public string sceneName;
    /// <summary>
    /// 关卡解锁所需积分
    /// </summary>
    [Header("关卡解锁所需积分, -1表示不能通过积分解锁")]
    public int scoreNeed;
    /// <summary>
    /// 关卡名
    /// </summary>
    [Header("关卡名")]
    public string levelName;
    /// <summary>
    /// 是否展示在选关界面
    /// </summary>
    [Header("是否展示在选关界面")]
    public bool showInLevelUI;

    [Header("不播放bgm（cg关卡打开）")]
    public bool notPlayBGM;

    [Header("不展示关卡标题（cg关卡打开）")]
    public bool notShowTitle;
}

/// <summary>
/// 备注：
/// levelCfg：在inspector 按顺序配置 所有关卡的场景名
/// 如果要做无缝关卡表现 preLoadLevelCount 不能为0，并且关卡之间的地形坐标需要衔接好
/// 每个关卡场景内需要挂载 
///     1.LevelDirector 脚本，配置好出生点和箱子的初始位置（如有复杂机关也需要加上初始状态配置）
///     2.关卡终点设置 LevelCheckPoint（ECheckPointType.LevelComplete）
///     3.如果要做无缝关卡，还需要在关卡入口设置 LevelCheckPoint （ECheckPointType.LevelStart）
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    /// <summary>
    /// 关卡设置
    /// </summary>
    public LevelCfg[] levelCfg;

    /// <summary>
    /// 预加载关卡数量 n
    /// 当前关卡的前n关和后n关都会预先加载
    /// </summary>
    public int preLoadLevelCount;

    private int m_curSceneIndex = -1;
    private HashSet<int> m_loadedScene = new HashSet<int>();
    private Dictionary<int, LevelDirector> m_dicLevelDirector = new Dictionary<int, LevelDirector>();
    public SceneFXController SetSceneFx;// 场景切换效果


    void Start()
    {
        instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    public void EnterLevel(int index)
    {
        SetSceneIndex(index);
    }

    public void EnterLevel(string sceneName)
    {
        int index = GetLevelIndex(sceneName);
        if (index < 0)
            return;

        SetSceneIndex(index);
    }

    public void NextLevel()
    {
        SetSceneIndex(m_curSceneIndex + 1);
    }

    /// <summary>
    /// 最近未通关的关卡
    /// </summary>
    /// <returns></returns>
    public int GetPlayLevelIndex()
    {
        int data = WorldStateManager.State.GetInt(WorldStateConst.LevelComplete, 0);
        if (data == 0)
            return 0;

        string bitStr = Convert.ToString(data, 2);
        char[] charArray = bitStr.ToCharArray();
        int levelIndex = 0;
        bool find = false;
        for (int i = 0; i < charArray.Length; i++)
        {
            if (charArray[i] == 0)
            {
                levelIndex = i;
                find = true;
                break;
            }
        }
        if (!find)
            levelIndex = charArray.Length;

        return Mathf.Clamp(levelIndex, 0, levelCfg.Length - 1);
    }

    /// <summary>
    /// 关卡配置
    /// </summary>
    /// <param name="index"></param>
    /// <param name="cfg"></param>
    /// <returns></returns>
    public bool TryGetLevelCfg(int index, out LevelCfg cfg)
    {
        cfg = default;
        if (index < 0)
            return false;

        if (index >= levelCfg.Length)
            return false;

        cfg = levelCfg[index];
        return true;
    }

    /// <summary>
    /// 关卡积分需求
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    public int GetNeedScore(string sceneName)
    {
        int index = GetLevelIndex(sceneName);
        LevelCfg cfg;
        if (!TryGetLevelCfg(index, out cfg))
            return 0;

        return cfg.scoreNeed;
    }

    /// <summary>
    /// 关卡名
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    public string GetLevelName(string sceneName)
    {
        int index = GetLevelIndex(sceneName);
        LevelCfg cfg;
        if (!TryGetLevelCfg(index, out cfg))
            return sceneName;

        return cfg.levelName;
    }

    /// <summary>
    /// 关卡是否解锁
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    public bool IsLevelUnlock(string sceneName)
    {
        int index = GetLevelIndex(sceneName);
        if (index < 0)
            return false;

        int playLevelIndex = GetPlayLevelIndex();
        if (index <= playLevelIndex)
            return true;

        int needScore = GetNeedScore(sceneName);
        if (needScore < 0)
            return false;

        return PlayerManager.instance.GetScore() >= needScore;
    }

    /// <summary>
    /// 关卡完成
    /// </summary>
    /// <param name="levelSceneName"></param>
    public void CompleteLevel(string levelSceneName)
    {
        var index = GetLevelIndex(levelSceneName);
        var data = WorldStateManager.State.GetInt(WorldStateConst.LevelComplete);
        data |= 1 << index;
        WorldStateManager.State.SetValue(WorldStateConst.LevelComplete, data);
        WorldStateManager.SaveToFile();
    }

    /// <summary>
    /// 收集物
    /// </summary>
    /// <param name="levelIndex"></param>
    /// <param name="number"></param>
    public void GatherCollect(int levelIndex, int number)
    {
        string key = $"{WorldStateConst.LevelCollect}_{levelIndex}";
        int value = WorldStateManager.State.GetInt(key, 0);
        value |= 1 << number;
        WorldStateManager.State.SetValue(key, value.ToString());
    }

    /// <summary>
    /// 收集物是否展示
    /// </summary>
    /// <param name="levelIndex"></param>
    /// <param name="number"></param>
    /// <returns></returns>
    public bool GetCollectShow(int levelIndex, int number)
    {
        string key = $"{WorldStateConst.LevelCollect}_{levelIndex}";
        int value = WorldStateManager.State.GetInt(key, 0);
        return (value & (1 << number)) == 0;
    }

    public void RegistLevelDirector(LevelDirector director)
    {
        m_dicLevelDirector.TryAdd(GetLevelIndex(director.levelSceneName), director);
    }

    public void UnregistLevelDirector(LevelDirector director)
    {
        m_dicLevelDirector.Remove(GetLevelIndex(director.levelSceneName));
    }

    public void ResetLevel()
    {
        if (m_dicLevelDirector.ContainsKey(m_curSceneIndex))
        {
            m_dicLevelDirector[m_curSceneIndex].ResetLevel();
            m_dicLevelDirector[m_curSceneIndex].LevelStart();
        }
    }

    public void UnloadAllLevel()
    {
        foreach (var index in m_loadedScene)
        {
            SceneManager.UnloadSceneAsync(levelCfg[index].sceneName);
        }
        m_loadedScene.Clear();
        m_curSceneIndex = -1;
    }

    public int GetLevelIndex(string sceneName)
    {
        int index = -1;
        for (int i = 0; i < levelCfg.Length; i++)
        {
            if (levelCfg[i].sceneName.Equals(sceneName))
                return i;
        }
        return index;
    }

    private void SetSceneIndex(int index)
    {
        if (m_curSceneIndex == index)
            return;
        
        m_curSceneIndex = index;
        TryLoadScene(m_curSceneIndex, ()=> { ResetLevel(); });
        for (int i = 1; i <= preLoadLevelCount; i++)
        {
            TryLoadScene(m_curSceneIndex + i);
            TryLoadScene(m_curSceneIndex - i);
        }
        TryUnloadScene();
    }

    private void TryLoadScene(int index, System.Action callback = null)
    {
        if (index < 0 || index >= levelCfg.Length)
            return;

        if (m_loadedScene.Contains(index))
            return;

        LoadScene(index, callback);
    }

    void TryUnloadScene()
    {
        List<int> unloadIndexes = new List<int>();
        foreach (var index in m_loadedScene)
        {
            if (index > m_curSceneIndex + preLoadLevelCount || index < m_curSceneIndex - preLoadLevelCount)
            {         
                unloadIndexes.Add(index);           
            }
        }

        foreach (var index in unloadIndexes)
        {
            SceneManager.UnloadSceneAsync(levelCfg[index].sceneName);
            m_loadedScene.Remove(index);
        }
    }

    void LoadScene(int index, System.Action callback = null)
    {   
        if (m_loadedScene.Contains(index))
            return;

        var op = SceneManager.LoadSceneAsync(levelCfg[index].sceneName, LoadSceneMode.Additive);
        // op.allowSceneActivation = true;
        op.completed += (AsyncOperation _op) =>
        {
            callback?.Invoke();
            m_loadedScene.Add(index);

            if (index == 1)
            {
                UIManager.Instance.Show("UITutorial");
            }

            LevelCfg cfg;
            if (TryGetLevelCfg(index, out cfg))
            {
                if (cfg.notPlayBGM)
                    AudioManager.StopLevelBGM();
                else
                    AudioManager.PlayLevelBGM();

                if (!cfg.notShowTitle)
                    UIManager.Instance.Show("UITitle", cfg.levelName);
            }
        };
    }
    
}
