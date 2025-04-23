using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI����
/// </summary>
public class UIBase : MonoBehaviour
{
    /// <summary>
    /// ��Ԥ��������һ��
    /// </summary>
    public string Name;

    /// <summary>
    /// UI�㼶
    /// </summary>
    public EUILayer layer; 

    /// <summary>
    /// UIչʾ
    /// ˢ����ʾ����
    /// </summary>
    public virtual void OnShow(object param = null)
    {
        
    }

    /// <summary>
    /// UI�ر�ʱ
    /// ����״̬���رն�ʱ��֮���
    /// </summary>
    public virtual void OnClose()
    {

    }

    /// <summary>
    /// �¼�
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="param"></param>
    public virtual void OnEvent(EUIEvent eventType, object param)
    {
        
    }

    /// <summary>
    /// �ر�
    /// </summary>
    public void Close()
    {
        UIManager.Instance.Close(this);
    }
}
