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
    [Header("触发事件的条件")]
    public List<WorldStateKV> Condition;
    [Header("触发事件后改变状态")]
    public List<WorldStateKV> Effect;

    [Header("碰撞进入事件")]
    public UnityEvent CollisionEnterEvent;
    [Header("碰撞离开事件")]
    public UnityEvent CollisionExitEvent;
    [Header("碰撞持续事件")]
    public UnityEvent CollisionStayEvent;
    [Header("触发器进入事件")]
    public UnityEvent TriggerEnterEvent;
    [Header("触发器离开事件")]
    public UnityEvent TriggerExitEvent;
    [Header("触发器持续事件")]
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
