using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cfg;
using SimpleJSON;
using System.IO;

public static class StoryManager
{
    static Dictionary<int, List<StoryCfg>> m_dicStorys;

    public static void Init()
    {
        m_dicStorys = new Dictionary<int, List<StoryCfg>>();
        List<StoryCfg> allStory = ConfigManager.GetAllStoryCfg();
        foreach (var item in allStory)
        {
            List<StoryCfg> list;
            if (!m_dicStorys.TryGetValue(item.Id, out list))
            {
                list = new List<StoryCfg>();
                m_dicStorys.Add(item.Id, list);
            }
            list.Add(item);
        }


        foreach (var item in m_dicStorys)
        {
            item.Value.Sort((x, y) =>
            {
                if (x.Index == y.Index)
                    return 0;
                else
                    return x.Index > y.Index ? 1 : -1;
            });
        }
    }

    public static List<StoryCfg> GetStroyCfgById(int id)
    {
        List<StoryCfg> list;
        if (m_dicStorys.TryGetValue(id, out list))
        {
            return list;
        }
        return null;
    }

    /// <summary>
    /// 开始一段剧情
    /// </summary>
    /// <param name="id"></param>
    public static void StartStory(int id)
    {
        List<StoryCfg> list = GetStroyCfgById(id);
        if (list == null || list.Count == 0)
            return;

        switch (list[0].Type)
        {
            case EStoryType.Talk:
                UIManager.Instance.Show("UITalk", list);
                break;
            case EStoryType.Narrator:
                UIManager.Instance.Show("UITalk", list);
                break;
            case EStoryType.Title:
                UIManager.Instance.Show("UITitle", list);
                break;
            default:
                break;
        }
    }

}
