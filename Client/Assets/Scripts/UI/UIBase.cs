using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI基类
/// </summary>
public class UIBase : MonoBehaviour
{
    /// <summary>
    /// 与预制体名称一致
    /// </summary>
    public string Name;

    /// <summary>
    /// UI层级
    /// </summary>
    public EUILayer layer; 

    /// <summary>
    /// UI展示
    /// 刷新显示数据
    /// </summary>
    public virtual void OnShow(object param = null)
    {
        
    }

    /// <summary>
    /// UI关闭时
    /// 清理状态，关闭定时器之类的
    /// </summary>
    public virtual void OnClose()
    {

    }

    /// <summary>
    /// 事件
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="param"></param>
    public virtual void OnEvent(EUIEvent eventType, object param)
    {
        
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public void Close()
    {
        UIManager.Instance.Close(this);
    }
}
