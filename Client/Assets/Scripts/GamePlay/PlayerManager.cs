using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    public PlayerController player;
    public PartnerController partner;

    public ControlSetting playerControlSetting;
    public DamageSetting damageSetting;
    public LayerMaskSetting layerMaskSetting;

    public float GravityValue = -9.81f;
    public float PlayerMoveSpeed;
    public float PlayerJumpHeight;

    public float PartnerFlySpeed;
    public float PartnerFlyMaxDistance;
    public float PartnerGoBackTime;
    
    public float BoxMoveSpeed;
    public float BoxMoveDistance;

    public float playerMaxHP;
    public float playerHp;
    
    private IController playerControler;
    private Dictionary<EControlType, float> m_lastActionTime = new Dictionary<EControlType, float>(); 
    private Dictionary<int, float> m_lastActionGroupTime = new Dictionary<int, float>();

    private Dictionary<ELayerMaskUsage, int> m_layerMaskUsage = new Dictionary<ELayerMaskUsage, int>();

    void Awake()
    {
        instance = this;
        //HidePlayer();
        playerControler = player as IController;
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(player.gameObject);
        DontDestroyOnLoad(partner.gameObject);

        foreach (var item in playerControlSetting.settingList)
        {
            if(!m_lastActionTime.ContainsKey(item.type))
                m_lastActionTime.Add(item.type, 0f);

            if (item.commonCdAffected)
                if(!m_lastActionGroupTime.ContainsKey(item.commonCdGroup))
                    m_lastActionGroupTime.Add(item.commonCdGroup, 0f);
        }

        foreach (var item in layerMaskSetting.settingList)
        {       
            m_layerMaskUsage.Add(item.usage, LayerMask.GetMask(item.layerNames));
        }
    }

    public void PlayerFixedUpdate()
    {
        //player.DoUpdate();
        //partner.DoUpdate();
    }

    public void PlayerUpdate()
    {
        player.DoUpdate();
        partner.DoUpdate();
        PlayerControl();
    }

    public int GetLayerMask(ELayerMaskUsage usage)
    {
        return m_layerMaskUsage[usage];
    }

    public void TeleportPlayerAndReset(Vector3 position, RigidbodyConstraints constraints)
    {
        player.gameObject.SetActive(true);
        partner.gameObject.SetActive(true);

        player.SetRigidConstraints(constraints);
        partner.rigidbody.constraints = constraints;

        player.SetPosition(position);
        partner.ForceDoFollow();
    }

    public void HidePlayer()
    {
        player.gameObject.SetActive(false);
        partner.gameObject.SetActive(false);
    }

    void PlayerControl()
    {
        foreach (var item in playerControlSetting.settingList)
        {
            if (Input.GetKeyDown(item.keyCode))
            {
                if (!CheckCD(item))
                {
                    Debug.Log($"CheckCD {item.type} cd{item.cd}");
                    return;
                }
                
                if (!CheckCommonCD(item))
                {
                    Debug.Log($"CheckCommonCD {item.type} cd{item.commonCd}");
                    return;
                }

                if (!playerControler.TryDoAction(item.type))
                {
                    Debug.Log($"TryDoAction {item.type}");
                    return;
                }
                
                RecordCD(item);
            }
        }
    }

    bool CheckCD(ControlSettingItem settingItem)
    {
        return Time.time - m_lastActionTime[settingItem.type] > settingItem.cd;
    }

    bool CheckCommonCD(ControlSettingItem settingItem)
    {
        if (!settingItem.commonCdAffected)
            return true;
        return Time.time - m_lastActionGroupTime[settingItem.commonCdGroup] > settingItem.commonCd;
    }

    void RecordCD(ControlSettingItem settingItem)
    {
        m_lastActionTime[settingItem.type] = Time.time;
        if (settingItem.commonCdAffected)
            m_lastActionGroupTime[settingItem.commonCdGroup] = Time.time;
    }
}
