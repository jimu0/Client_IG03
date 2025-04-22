using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DamageObject : MonoBehaviour 
{
    [Header("�˺�����")]
    public EDamageType type;
    [Header("�Զ����˺�ֵ ����type = customʱ��Ч")]
    public float customValue;

    private void OnTriggerEnter(Collider other)
    {
        if ((1 << other.gameObject.layer & PlayerManager.instance.GetLayerMask(ELayerMaskUsage.TriggerForPlayer)) == 0)
            return;

        PlayerManager.instance.Damage(type, customValue);
    }
}
