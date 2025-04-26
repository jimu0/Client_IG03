using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cinemachine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    [Header("全局")]
    public ControlSetting playerControlSetting;
    public DamageSetting damageSetting;
    public LayerMaskSetting layerMaskSetting;
    public float GravityValue = -9.81f;

    [Header("角色控制相关")]
    public PlayerController player;
    public float PlayerMoveSpeed;
    public float PlayerJumpHeight;

    [Header("伙伴控制相关")]
    public PartnerController partner;
    public float PartnerFlySpeed;
    public float PartnerFlyMaxDistance;
    public float PartnerGoBackTime;
    public AnimationCurve PartnerGoBackCurve;
    public AnimationCurve PartnerFlyAndGoBackCurve;

    [Header("箱子控制相关")]
    public float BoxMoveSpeed;
    public float BoxMoveDistance;

    public float playerMaxHP;
    public float playerHp;

    public float score;

    [Header("相机控制相关"), Range(0, 1)]
    public CinemachineVirtualCamera camera;
    public float cameraScreenYDown;
    [Range(0, 1)]
    public float cameraScreenYUp;
    public float cameraDistanceWhenLookUpDown;
    private float m_cameraDefaultScreenY;
    private float m_cameraDefaultDistance;
    private CinemachineFramingTransposer m_framingTransposer;

    private IController m_playerControler;
    private ControlSettingItem m_playerJumpSetting; // 跳跃控制特殊处理
    private Dictionary<EControlType, float> m_lastActionTime = new Dictionary<EControlType, float>(); 
    private Dictionary<int, float> m_lastActionGroupTime = new Dictionary<int, float>();

    private Dictionary<ELayerMaskUsage, int> m_layerMaskUsage = new Dictionary<ELayerMaskUsage, int>();
    private Dictionary<EDamageType, float> m_damageCfg = new Dictionary<EDamageType, float>();

    void Awake()
    {
        instance = this;
        //HidePlayer();
        m_playerControler = player as IController;
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(player.gameObject);
        DontDestroyOnLoad(partner.gameObject);

        m_framingTransposer = camera.GetCinemachineComponent<CinemachineFramingTransposer>();
        m_cameraDefaultScreenY = m_framingTransposer.m_ScreenY;
        m_cameraDefaultDistance = m_framingTransposer.m_CameraDistance;

        foreach (var item in playerControlSetting.settingList)
        {
            if(!m_lastActionTime.ContainsKey(item.type))
                m_lastActionTime.Add(item.type, 0f);

            if (item.commonCdAffected)
                if(!m_lastActionGroupTime.ContainsKey(item.commonCdGroup))
                    m_lastActionGroupTime.Add(item.commonCdGroup, 0f);

            if (item.type == EControlType.Jump)
                player.JumpSetting = item;
        }

        foreach (var item in layerMaskSetting.settingList)
        {       
            m_layerMaskUsage.Add(item.usage, LayerMask.GetMask(item.layerNames));
            //Debug.Log($"m_layerMaskUsage {item.usage} {LayerMask.GetMask(item.layerNames)} " );
        }

        foreach (var item in damageSetting.settingList)
        {
            m_damageCfg.Add(item.type, item.value);
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
        CameraControl();
    }

    public int GetLayerMask(ELayerMaskUsage usage)
    {
        return m_layerMaskUsage[usage];
    }

    public void Damage(EDamageType type, float customValue = 0f)
    {
        var value = 0f;
        if (type == EDamageType.CustomValue)
            value = customValue;
        else
            value = m_damageCfg.GetValueOrDefault(type, 0f);
        playerHp = Mathf.Clamp(playerHp + value, 0, playerMaxHP);

        UIManager.Instance.SendEvent(EUIEvent.PlayerHpChange);

        Debug.Log($"Damage  type { type} value {value} hp {playerHp}");
        if (playerHp <= 0)
        {
            TmpGameManager.instance.GameOver();
        }
        else
        {
            // todo 受伤表现
        }
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
            if (Input.GetKeyDown(item.keyCode) || Input.GetKeyDown(item.keyCode2))
            {
                if (!CheckCD(item))
                {
                    Debug.Log($"CheckCD {item.type} cd{item.cd}");
                    continue;
                }
                
                if (!CheckCommonCD(item))
                {
                    Debug.Log($"CheckCommonCD {item.type} cd{item.commonCd}");
                    continue;
                }

                if (!m_playerControler.TryDoAction(item.type))
                {
                    Debug.Log($"TryDoAction {item.type}");
                    continue;
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


    void CameraControl()
    {
        var value = Input.GetAxis("Vertical");
        if (value != 0)
        {
            m_framingTransposer.m_ScreenY = value < 0 ? cameraScreenYUp : cameraScreenYDown;
            m_framingTransposer.m_CameraDistance = cameraDistanceWhenLookUpDown;
        }
        else
        {
            // 复位
            m_framingTransposer.m_ScreenY = m_cameraDefaultScreenY;
            m_framingTransposer.m_CameraDistance = m_cameraDefaultDistance;
        }
    }
}
