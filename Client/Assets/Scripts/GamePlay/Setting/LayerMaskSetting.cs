using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ELayerMaskUsage
{
    /// <summary>
    /// 可推动的物体
    /// </summary>
    Pushable,

    /// <summary>
    /// 可推动物体+墙
    /// </summary>
    MoveSpaceCheck,

    /// <summary>
    /// 碰撞
    /// </summary>
    PlayerCollition,
    PartnerCollition,
    BoxCollition,
    
    /// <summary>
    /// 伙伴可连接物体
    /// </summary>
    PartnerLink,

    /// <summary>
    /// 玩家触发器
    /// </summary>
    TriggerForPlayer,
}

[Serializable]
public struct LayerMaskSettingItem
{
    public ELayerMaskUsage usage;
    public string[] layerNames;

    public LayerMaskSettingItem(ELayerMaskUsage type)
    {
        this.usage = type;
        this.layerNames = default;
    }      
}

[CreateAssetMenu(fileName = "LayerMaskSetting", menuName = "LayerMaskSetting", order = 0)]
public class LayerMaskSetting : ScriptableObject 
{
    [SerializeField]
    public List<LayerMaskSettingItem> settingList = new List<LayerMaskSettingItem>();

    LayerMaskSetting()
    {
        foreach (ELayerMaskUsage type in Enum.GetValues(typeof(ELayerMaskUsage)))
        {
            settingList.Add(new LayerMaskSettingItem(type));
        }
    }
}

