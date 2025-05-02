using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Numerics;

/// <summary>
/// 备注：
/// sceneNameArray：在inspector 按顺序配置 所有关卡的场景名
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
    /// 配置所有关卡场景
    /// </summary>
    public string[] sceneNameArray;

    /// <summary>
    /// 关卡进入需要的积分
    /// </summary>
    public int[] levelScoreNeed;

    /// <summary>
    /// 预加载关卡数量 n
    /// 当前关卡的前n关和后n关都会预先加载
    /// </summary>
    public int preLoadLevelCount;

    private int m_curSceneIndex = -1;
    private HashSet<int> m_loadedScene = new HashSet<int>();
    private Dictionary<int, LevelDirector> m_dicLevelDirector = new Dictionary<int, LevelDirector>();

    void Start()
    {
        instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    public void EnterLevel(int index)
    {
        SetSceneIndex(index);
    }

    public void EnterLevel(string levelName)
    {
        int index = GetLevelIndex(levelName);
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

        return Mathf.Clamp(levelIndex, 0, sceneNameArray.Length - 1);
    }

    public void CompleteLevel(string levelSceneName)
    {
        var index = GetLevelIndex(levelSceneName);
        var data = WorldStateManager.State.GetInt(WorldStateConst.LevelComplete);
        data |= 1 << index;
        WorldStateManager.State.SetValue(WorldStateConst.LevelComplete, data);
        WorldStateManager.SaveToFile();
    }

    public void GatherCollect(int levelIndex, int number)
    {
        string key = $"{WorldStateConst.LevelCollect}_{levelIndex}";
        int value = WorldStateManager.State.GetInt(key, 0);
        value |= 1 << number;
        WorldStateManager.State.SetValue(key, value.ToString());
    }

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
            SceneManager.UnloadSceneAsync(sceneNameArray[index]);
        }
        m_loadedScene.Clear();
        m_curSceneIndex = -1;
    }

    public int GetLevelIndex(string levelName)
    {
        return Array.IndexOf(sceneNameArray, levelName);
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
        if (index < 0 || index >= sceneNameArray.Length)
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
            SceneManager.UnloadSceneAsync(sceneNameArray[index]);
            m_loadedScene.Remove(index);
        }
    }

    void LoadScene(int index, System.Action callback = null)
    {   
        if (m_loadedScene.Contains(index))
            return;

        var op = SceneManager.LoadSceneAsync(sceneNameArray[index], LoadSceneMode.Additive);
        // op.allowSceneActivation = true;
        op.completed += (AsyncOperation _op) =>
        {
            callback?.Invoke();
            m_loadedScene.Add(index);
        };
    }
}
