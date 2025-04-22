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

    [Header("触发器或碰撞进入事件")]
    public UnityEvent CollisionOrTriggerEnterEvent;
    [Header("触发器或碰撞离开事件")]
    public UnityEvent CollisionOrTriggerExitEvent;
    [Header("触发器或碰撞持续事件")]
    public UnityEvent CollisionOrTriggerStayEvent;


    [Header("弃用 碰撞进入事件")]
    public UnityEvent CollisionEnterEvent;
    [Header("弃用 碰撞离开事件")]
    public UnityEvent CollisionExitEvent;
    [Header("弃用碰撞持续事件")]
    public UnityEvent CollisionStayEvent;
    [Header("弃用触发器进入事件")]
    public UnityEvent TriggerEnterEvent;
    [Header("弃用触发器离开事件")]
    public UnityEvent TriggerExitEvent;
    [Header("弃用触发器持续事件")]
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
