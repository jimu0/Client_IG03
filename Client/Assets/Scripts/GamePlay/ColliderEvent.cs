using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

public enum EValueType
{
    Int,
    String,
    Bool,
}

[Serializable]
public struct WorldStateKV
{
    public string key;
    public string value;
    public EValueType valueType;
    public EOperation op;
}

public class ColliderEvent : MonoBehaviour
{
    [Header("�����¼�������")]
    public List<WorldStateKV> Condition;
    [Header("�����¼���ı�״̬")]
    public List<WorldStateKV> Effect;

    [Header("����������ײ�����¼�")]
    public UnityEvent CollisionOrTriggerEnterEvent;
    [Header("����������ײ�뿪�¼�")]
    public UnityEvent CollisionOrTriggerExitEvent;
    [Header("����������ײ�����¼�")]
    public UnityEvent CollisionOrTriggerStayEvent;


    [Header("���� ��ײ�����¼�")]
    public UnityEvent CollisionEnterEvent;
    [Header("���� ��ײ�뿪�¼�")]
    public UnityEvent CollisionExitEvent;
    [Header("������ײ�����¼�")]
    public UnityEvent CollisionStayEvent;
    [Header("���ô����������¼�")]
    public UnityEvent TriggerEnterEvent;
    [Header("���ô������뿪�¼�")]
    public UnityEvent TriggerExitEvent;
    [Header("���ô����������¼�")]
    public UnityEvent TriggerStayEvent;

    void TryInvoke(ref UnityEvent unityEvent)
    {
        if (unityEvent == null || unityEvent.GetPersistentEventCount() == 0)
            return;

        if (!WorldStateManager.Check(Condition))
            return;

        unityEvent?.Invoke();
        WorldStateManager.SetValues(Condition);
    }

    void OnCollisionEnter(Collision collision)
    {
        TryInvoke(ref CollisionOrTriggerEnterEvent);
        //if (CollisionEnterEvent == null || CollisionEnterEvent.GetPersistentEventCount() == 0)
        //    return;

        //if (!WorldStateManager.Check(Condition))
        //    return;
        //CollisionEnterEvent?.Invoke();
        //WorldStateManager.SetValues(Condition);
    }

    void OnCollisionExit(Collision collision)
    {
        TryInvoke(ref CollisionOrTriggerExitEvent);
        //if (CollisionExitEvent == null || CollisionExitEvent.GetPersistentEventCount() == 0)
        //    return;

        //if (!WorldStateManager.Check(Condition))
        //    return;
        //CollisionExitEvent?.Invoke();
        //WorldStateManager.SetValues(Condition);
    }

    void OnCollisionStay(Collision collision)
    {
        TryInvoke(ref CollisionOrTriggerStayEvent);
        //if (CollisionStayEvent == null || CollisionStayEvent.GetPersistentEventCount() == 0)
        //    return;

        //if (!WorldStateManager.Check(Condition))
        //    return;
        //CollisionStayEvent?.Invoke();
        //WorldStateManager.SetValues(Condition);
    }

    void OnTriggerEnter(Collider other)
    {
        TryInvoke(ref CollisionOrTriggerEnterEvent);
        //if (TriggerEnterEvent == null || TriggerEnterEvent.GetPersistentEventCount() == 0)
        //    return;

        //if (!WorldStateManager.Check(Condition))
        //    return;
        //TriggerEnterEvent?.Invoke();
        //WorldStateManager.SetValues(Condition);
    }

    void OnTriggerExit(Collider other)
    {
        TryInvoke(ref CollisionOrTriggerExitEvent);
        //if (TriggerExitEvent == null || TriggerExitEvent.GetPersistentEventCount() == 0)
        //    return;

        //if (!WorldStateManager.Check(Condition))
        //    return;
        //TriggerExitEvent?.Invoke();
        //WorldStateManager.SetValues(Condition);
    }

    void OnTriggerStay(Collider other)
    {
        TryInvoke(ref CollisionOrTriggerStayEvent);
        //if (TriggerStayEvent == null || TriggerStayEvent.GetPersistentEventCount() == 0)
        //    return;

        //if (!WorldStateManager.Check(Condition))
        //    return;
        //TriggerStayEvent?.Invoke();
        //WorldStateManager.SetValues(Condition);
    }
}
