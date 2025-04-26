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

    private EDoorState m_shouldBeState;
    private EDoorState m_realState;

    void Start()
    {
        m_shouldBeState = defaultState;
    }

    public void OpenDoor()
    {
        m_shouldBeState = EDoorState.Open;
        Debug.Log("OpenDoor");
    }

    public void CloseDoor()
    {
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
        if(Physics.BoxCast(collider.bounds.center, collider.bounds.extents, Vector3.up, default, 0f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.BoxCollition)))
            return false;
        
        return true;
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
