using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IController
{
    bool TryDoAction(EControlType type);
}
