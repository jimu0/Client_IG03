using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPushable
{
    void DoMove(Vector3 direction);
    void SetLink(IPushable other);
    IPushable GetLink();
    void TryMoveNearby();
    bool IsCanMove(Vector3 direction);
    bool IsLinkedBoxCanMove(Vector3 direction);
}
