using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using cfg;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// UI»ùÀà
/// </summary>
public class UIGame : UIBase
{
    //public TextMeshProUGUI text_title;
    public Button BtnPause;
    public TextMeshProUGUI TextHp;

    public override void OnShow(object param)
    {
        UIManager.Instance.RegistEvent(this);
        BtnPause.onClick.AddListener(() =>
        {
            TmpGameManager.instance.Pause();
        });

        TextHp.text = $"HP : {PlayerManager.instance.playerHp}/{PlayerManager.instance.playerMaxHP}";
    }

    public override void OnClose()
    {
        BtnPause.onClick.RemoveAllListeners();
    }

    public override void OnEvent(EUIEvent eventType, object param)
    {
        if (eventType != EUIEvent.PlayerHpChange)
            return;

        TextHp.text = $"HP : {PlayerManager.instance.playerHp}/{PlayerManager.instance.playerMaxHP}";
    }
}
