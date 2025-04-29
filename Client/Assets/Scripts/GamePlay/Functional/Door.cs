using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EDoorState
{
    Close,
    Open,
}

public class Door : MonoBehaviour
{
    public BoxCollider collider;

    public EDoorState defaultState;

    public Animator[] DoorAnimators;

    public string AnimOpenName = "";
    public string AnimCloseName = "";

    public int StateChangeDelayFrame = 0;

    private EDoorState m_shouldBeState;
    private EDoorState m_realState;

    private Vector3 m_colliderCenter;
    private Vector3 m_colliderExtents;

    public int m_stateChangeFrame;

    void Start()
    {
        m_colliderCenter = collider.bounds.center;
        m_colliderExtents = collider.bounds.extents;
        m_shouldBeState = defaultState;
    }

    public void OpenDoor()
    {
        m_stateChangeFrame = 0;
        m_shouldBeState = EDoorState.Open;
        Debug.Log("OpenDoor");
    }

    public void CloseDoor()
    {
        m_stateChangeFrame = 0;
        m_shouldBeState = EDoorState.Close;
        Debug.Log("CloseDoor");
    }

    private void Update() 
    {
        UpdateState();
    }

    private void UpdateState()
    {
        if (m_shouldBeState == m_realState)
            return;

        m_stateChangeFrame++;
        if (m_stateChangeFrame < StateChangeDelayFrame)
            return;

        if (m_shouldBeState == EDoorState.Close)
        {
            if (IsCanClose())
                BeClose();
        }
        else
            BeOpen();
    }

    private bool IsCanClose()
    {
        var results = Physics.OverlapBox(m_colliderCenter, m_colliderExtents*0.9f, Quaternion.identity, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.BoxCollition), QueryTriggerInteraction.Collide);
        return results.Length == 0;
    }

    private void BeClose()
    {
        m_realState = EDoorState.Close;
        collider.enabled = true;
        // 动画
        foreach (var item in DoorAnimators)
        {
            item.Play(AnimCloseName, 0, 0);
        }
    }

    private void BeOpen()
    {
        m_realState = EDoorState.Open;
        collider.enabled = false;
        // 动画
        foreach (var item in DoorAnimators)
        {
            item.Play(AnimOpenName, 0, 0);
        }
    }
}
