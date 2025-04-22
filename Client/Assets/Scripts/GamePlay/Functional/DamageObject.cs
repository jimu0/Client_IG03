using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DamageObject : MonoBehaviour 
{
    [Header("伤害类型")]
    public EDamageType type;
    [Header("自定义伤害值 仅在type = custom时有效")]
    public float customValue;

    private void OnTriggerEnter(Collider other)
    {
        if ((1 << other.gameObject.layer & PlayerManager.instance.GetLayerMask(ELayerMaskUsage.TriggerForPlayer)) == 0)
            return;

        PlayerManager.instance.Damage(type, customValue);
    }
}
