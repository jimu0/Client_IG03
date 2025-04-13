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

    [Header("��ײ�����¼�")]
    public UnityEvent CollisionEnterEvent;
    [Header("��ײ�뿪�¼�")]
    public UnityEvent CollisionExitEvent;
    [Header("��ײ�����¼�")]
    public UnityEvent CollisionStayEvent;
    [Header("�����������¼�")]
    public UnityEvent TriggerEnterEvent;
    [Header("�������뿪�¼�")]
    public UnityEvent TriggerExitEvent;
    [Header("�����������¼�")]
    public UnityEvent TriggerStayEvent;

    void OnCollisionEnter(Collision collision)
    {
        if (CollisionEnterEvent == null || CollisionEnterEvent.GetPersistentEventCount() == 0)
            return;

        if (!WorldStateManager.Check(Condition))
            return;
        CollisionEnterEvent?.Invoke();
        WorldStateManager.SetValues(Condition);
    }

    void OnCollisionExit(Collision collision)
    {
        if (CollisionExitEvent == null || CollisionEnterEvent.GetPersistentEventCount() == 0)
            return;

        if (!WorldStateManager.Check(Condition))
            return;
        CollisionExitEvent?.Invoke();
        WorldStateManager.SetValues(Condition);
    }

    void OnCollisionStay(Collision collision)
    {
        if (CollisionStayEvent == null || CollisionEnterEvent.GetPersistentEventCount() == 0)
            return;

        if (!WorldStateManager.Check(Condition))
            return;
        CollisionStayEvent?.Invoke();
        WorldStateManager.SetValues(Condition);
    }

    void OnTriggerEnter(Collider other)
    {
        if (TriggerEnterEvent == null || CollisionEnterEvent.GetPersistentEventCount() == 0)
            return;

        if (!WorldStateManager.Check(Condition))
            return;
        TriggerEnterEvent?.Invoke();
        WorldStateManager.SetValues(Condition);
    }

    void OnTriggerExit(Collider other)
    {
        if (TriggerExitEvent == null || CollisionEnterEvent.GetPersistentEventCount() == 0)
            return;

        if (!WorldStateManager.Check(Condition))
            return;
        TriggerExitEvent?.Invoke();
        WorldStateManager.SetValues(Condition);
    }

    void OnTriggerStay(Collider other)
    {
        if (TriggerStayEvent == null || CollisionEnterEvent.GetPersistentEventCount() == 0)
            return;

        if (!WorldStateManager.Check(Condition))
            return;
        TriggerStayEvent?.Invoke();
        WorldStateManager.SetValues(Condition);
    }
}
