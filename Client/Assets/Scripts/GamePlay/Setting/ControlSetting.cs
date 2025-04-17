using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum EControlType
{
    //Move,
    Jump,
    PushBox,
    ShootPartner,
    BackPartener,
    ActivePartner,
    InacitvePartner,
}

[Serializable]
public struct ControlSettingItem
{
    [Header("动作类型")]
    public EControlType type;
    [Header("控制键盘")]
    public KeyCode keyCode;
    [Header("动作cd")]
    public float cd;
    [Header("此动作是否影响和被影响公共cd")]
    public bool commonCdAffected;
    [Header("使用后增加公共cd")]
    public float commonCd;
    [Header("公共cd组")]
    public int commonCdGroup;
    public ControlSettingItem(EControlType type)
    {
        this.type = type;
        this.keyCode = default;
        this.cd = default;
        this.commonCd = default;
        this.commonCdAffected = default;
        this.commonCdGroup = default;
    }       
}

[CreateAssetMenu(fileName = "ControllSetting", menuName = "ControllSetting", order = 0)]
public class ControlSetting : ScriptableObject 
{
    [SerializeField]
    public List<ControlSettingItem> settingList = new List<ControlSettingItem>();

    ControlSetting()
    {
        foreach (EControlType type in Enum.GetValues(typeof(EControlType)))
        {
            settingList.Add(new ControlSettingItem(type));
        }
    }
}

