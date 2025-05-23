using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cfg;
using SimpleJSON;
using System.IO;

public enum EUILayer
{
    Low,
    Mid,
    Height,
}

/// <summary>
/// 简单UI事件，所有注册的UI能收到所有事件
/// </summary>
public enum EUIEvent
{
    PlayerHpChange,
}

public class UIManager : MonoBehaviour
{ 
    public static UIManager Instance;
    public Canvas canvasLow;
    public Canvas canvasMid;
    public Canvas canvasHeight;
    public Transform Pool;

    private Dictionary<string, List<UIBase>> dicPool;
    private HashSet<UIBase> hashShowingUI;

    private HashSet<UIBase> m_dicRegistEventUI;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        Pool.gameObject.SetActive(false);
        dicPool = new Dictionary<string, List<UIBase>>();
        hashShowingUI = new HashSet<UIBase>();
        m_dicRegistEventUI = new HashSet<UIBase>();
    }

    /// <summary>
    /// 展示UI
    /// </summary>
    /// <param name="UIName">UI预制体名字</param>
    /// <param name="param">参数</param>
    public void Show(string UIName, object param = null)
    {
        var uiBase = TryGetUIFromPool(UIName);
        if (uiBase != null)
        {
            DoShow(uiBase, param);
            return;
        }

        string key = "UI_" + UIName;
        ResourceManger.LoadResAsync<GameObject>(key, (obj) =>
        {
            if (obj == null)
                return;

            var gameObject = GameObject.Instantiate(obj);
            var uiBase2 = gameObject.GetComponent<UIBase>();
            uiBase2.Name = UIName;
            DoShow(uiBase2, param);
        });
    }

    /// <summary>
    /// 关闭所有UI
    /// </summary>
    public void CloseAll()
    {
        CloseByLayer(EUILayer.Low);
        CloseByLayer(EUILayer.Mid);
        CloseByLayer(EUILayer.Height);
    }

    public void CloseByName(string UIName)
    {
        List<UIBase> closeUIList = new List<UIBase>();
        foreach (var item in hashShowingUI)
        {
            if (item.Name.Equals(UIName))
            {
                closeUIList.Add(item);
                break;
            }
        }

        foreach (var item in closeUIList)
        {
            Close(item);
        }
    }

    public void CloseByLayer(EUILayer layer)
    {
        List<UIBase> closeUIList = new List<UIBase>();
        foreach (var item in hashShowingUI)
        {
            if (item.layer == layer)
            {
                closeUIList.Add(item);
            }
        }

        foreach (var item in closeUIList)
        {
            Close(item);
        }
    }

    public void Close(UIBase uiBase)
    {
        DoClose(uiBase);
    }

    public void RegistEvent(UIBase uIBase)
    {
        m_dicRegistEventUI.Add(uIBase);
    }

    public void UnRegistEvent(UIBase uiBase)
    {
        m_dicRegistEventUI.Remove(uiBase);
    }

    public void SendEvent(EUIEvent eventType, object param = null )
    {
        foreach (var item in m_dicRegistEventUI)
        {
            item.OnEvent(eventType, param);
        }
    }

    /// <summary>
    /// 清理池子
    /// </summary>
    public void ClearPool()
    {
        dicPool.Clear();
        for (int i = 0; i < Pool.childCount; i++)
        {
            Destroy(Pool.GetChild(i).gameObject);
        }
    }

    private UIBase TryGetUIFromPool(string uiName)
    {
        List<UIBase> list;
        if (dicPool.TryGetValue(uiName, out list))
        {
            if (list.Count > 0)
            {
                var uiBase = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return uiBase;
            }
        }
        return null;
    }

    private void DoShow(UIBase uiBase, object param)
    {
        if (uiBase == null)
        {
            Debug.LogError("DoShow failed!!");
            return;
        }

        switch (uiBase.layer)
        {
            case EUILayer.Low:
                uiBase.transform.SetParent(canvasLow.transform, false);
                break;
            case EUILayer.Mid:
                uiBase.transform.SetParent(canvasMid.transform, false);
                break;
            case EUILayer.Height:
                uiBase.transform.SetParent(canvasHeight.transform, false);
                break;
            default:
                break;
        }
        hashShowingUI.Add(uiBase);
        uiBase.OnShow(param);
    }

    private void DoClose(UIBase uiBase)
    {
        UnRegistEvent(uiBase);
        uiBase.OnClose();
        uiBase.transform.SetParent(Pool, false);
        List<UIBase> list;
        if(!dicPool.TryGetValue(uiBase.Name, out list))
        {
            list = new List<UIBase>();
            dicPool.Add(uiBase.Name, list);
        }
        list.Add(uiBase);
        hashShowingUI.Remove(uiBase);
    }
}
