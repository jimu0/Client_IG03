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
    /// �ؿ�������
    /// </summary>
    [Header("�ؿ�������")]
    public string sceneName;
    /// <summary>
    /// �ؿ������������
    /// </summary>
    [Header("�ؿ������������, -1��ʾ����ͨ�����ֽ���")]
    public int scoreNeed;
    /// <summary>
    /// �ؿ���
    /// </summary>
    [Header("�ؿ���")]
    public string levelName;
    /// <summary>
    /// �Ƿ�չʾ��ѡ�ؽ���
    /// </summary>
    [Header("�Ƿ�չʾ��ѡ�ؽ���")]
    public bool showInLevelUI;

    [Header("������bgm��cg�ؿ��򿪣�")]
    public bool notPlayBGM;

    [Header("��չʾ�ؿ����⣨cg�ؿ��򿪣�")]
    public bool notShowTitle;
}

/// <summary>
/// ��ע��
/// levelCfg����inspector ��˳������ ���йؿ��ĳ�����
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
    /// �ؿ�����
    /// </summary>
    public LevelCfg[] levelCfg;

    /// <summary>
    /// Ԥ���عؿ����� n
    /// ��ǰ�ؿ���ǰn�غͺ�n�ض���Ԥ�ȼ���
    /// </summary>
    public int preLoadLevelCount;

    private int m_curSceneIndex = -1;
    private HashSet<int> m_loadedScene = new HashSet<int>();
    private Dictionary<int, LevelDirector> m_dicLevelDirector = new Dictionary<int, LevelDirector>();
    public SceneFXController SetSceneFx;// �����л�Ч��


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
    /// ���δͨ�صĹؿ�
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
    /// �ؿ�����
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
    /// �ؿ���������
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
    /// �ؿ���
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
    /// �ؿ��Ƿ����
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
    /// �ؿ����
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
    /// �ռ���
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
    /// �ռ����Ƿ�չʾ
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
