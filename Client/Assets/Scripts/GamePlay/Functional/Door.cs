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

    public EDoorState shouldBeState;
    public EDoorState realState;

    public Animator[] DoorAnimators;

    public string AnimOpenName = "";
    public string AnimCloseName = "";

    void Start()
    {
        collider = GetComponent<BoxCollider>();    
    }

    public void OpenDoor()
    {
        shouldBeState = EDoorState.Open;
        Debug.Log("OpenDoor");
    }

    public void CloseDoor()
    {
        shouldBeState = EDoorState.Close;
        Debug.Log("CloseDoor");
    }

    private void Update() 
    {
        UpdateState();
    }

    private void UpdateState()
    {
        if (shouldBeState == realState)
            return;

        if (shouldBeState == EDoorState.Close)
        {
            if (IsCanClose())
                BeClose();
        }
        else
            BeOpen();
    }

    private bool IsCanClose()
    {
        if(Physics.BoxCast(collider.bounds.center, collider.bounds.extents, Vector3.up, default,  PlayerManager.instance.GetLayerMask(ELayerMaskUsage.BoxCollition)))
            return false;
        
        return true;
    }

    private void BeClose()
    {
        realState = EDoorState.Close;
        collider.enabled = true;
        // 动画
        foreach (var item in DoorAnimators)
        {
            item.Play(AnimCloseName, -1, 0);
        }
    }

    private void BeOpen()
    {
        realState = EDoorState.Open;
        collider.enabled = false;
        // 动画
        foreach (var item in DoorAnimators)
        {
            item.Play(AnimOpenName, -1, 0);
        }
    }
}
