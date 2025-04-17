using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum EDamageType
{
    CustomValue,
    Trap,
    Heal,
    
}

[Serializable]
public struct DamameSettingItem
{
    [Header("伤害类型")]
    public EDamageType type;
    [Header("值")]
    public float value;
    public DamameSettingItem(EDamageType type)
    {
        this.type = type;
        this.value = default;
    }       
}

[CreateAssetMenu(fileName = "DamageSetting", menuName = "DamageSetting", order = 0)]
public class DamageSetting : ScriptableObject 
{
    [SerializeField]
    public List<DamameSettingItem> settingList = new List<DamameSettingItem>();

    DamageSetting()
    {
        foreach (EDamageType type in Enum.GetValues(typeof(EDamageType)))
        {
            settingList.Add(new DamameSettingItem(type));
        }
    }
}
