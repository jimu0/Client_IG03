using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ELayerMaskUsage
{
    /// <summary>
    /// ���ƶ�������
    /// </summary>
    Pushable,

    /// <summary>
    /// ���ƶ�����+ǽ
    /// </summary>
    MoveSpaceCheck,

    /// <summary>
    /// ��ײ
    /// </summary>
    PlayerCollition,
    PartnerCollition,
    BoxCollition,
    
    /// <summary>
    /// ������������
    /// </summary>
    PartnerLink,

    /// <summary>
    /// ��Ҵ�����
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

