using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// ��ע��
/// sceneNameArray����inspector ��˳������ ���йؿ��ĳ�����
/// ���Ҫ���޷�ؿ����� preLoadLevelCount ����Ϊ0�����ҹؿ�֮��ĵ���������Ҫ�νӺ�
/// ÿ���ؿ���������Ҫ���� 
///     1.LevelDirector �ű������úó���������ӵĳ�ʼλ�ã����и��ӻ���Ҳ��Ҫ���ϳ�ʼ״̬���ã�
///     2.�ؿ��յ����� LevelCheckPoint��ECheckPointType.LevelComplete��
///     3.���Ҫ���޷�ؿ�������Ҫ�ڹؿ�������� LevelCheckPoint ��ECheckPointType.LevelStart��
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    /// <summary>
    /// �������йؿ�����
    /// </summary>
    public string[] sceneNameArray;

    /// <summary>
    /// Ԥ���عؿ����� n
    /// ��ǰ�ؿ���ǰn�غͺ�n�ض���Ԥ�ȼ���
    /// </summary>
    public int preLoadLevelCount;

    private int m_curSceneIndex = -1;
    private HashSet<int> m_loadedScene = new HashSet<int>();
    private Dictionary<int, LevelDirector> m_dicLevelDirector = new Dictionary<int, LevelDirector>();

    void Start()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void EnterLevel(int index)
    {
        SetSceneIndex(index);
    }

    public void EnterLevel(string levelName)
    {
        int index = GetSceneIndex(levelName);
        if (index < 0)
            return;

        SetSceneIndex(index);
    }

    public void NextLevel()
    {
        SetSceneIndex(m_curSceneIndex + 1);
    }

    public int GetPlayLevelIndex()
    {
        int levelIndex = WorldStateManager.State.GetInt(WorldStateConst.LevelComplete, -1);
        return Mathf.Clamp(levelIndex + 1, 0, sceneNameArray.Length - 1);
    }

    public void CompleteLevel(string levelSceneName)
    {
        WorldStateManager.SetValue(WorldStateConst.LevelComplete, GetSceneIndex(levelSceneName).ToString());
    }

    public void RegistLevelDirector(LevelDirector director)
    {
        m_dicLevelDirector.TryAdd(GetSceneIndex(director.levelSceneName), director);
    }

    public void UnregistLevelDirector(LevelDirector director)
    {
        m_dicLevelDirector.Remove(GetSceneIndex(director.levelSceneName));
    }

    public void ResetLevel()
    {
        if (m_dicLevelDirector.ContainsKey(m_curSceneIndex))
        {
            m_dicLevelDirector[m_curSceneIndex].ResetLevel();
            m_dicLevelDirector[m_curSceneIndex].LevelStart();
        }
    }

    private int GetSceneIndex(string levelName)
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
            //  todo ������Դ�ֶ����ػ��bug
            ResourceManger.LoadResAsync<Material>("Materials_unit_Box_mat", null);
            ResourceManger.LoadResAsync<Material>("Materials_unit_wall_mat", null);
            callback?.Invoke();
            m_loadedScene.Add(index);
        };
    }
}
