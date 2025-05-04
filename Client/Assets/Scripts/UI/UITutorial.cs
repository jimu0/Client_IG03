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
public class UITutorial : UIBase
{
    public Button BtnClose;

    public Transform[] TranPartnerControl;

    public override void OnShow(object param)
    {
        BtnClose.onClick.AddListener(() =>
        {
            AudioManager.PlayBtnClick();
            Close();
        });

        foreach (var item in TranPartnerControl)
        {
            item.gameObject.SetActive(WorldStateManager.State.GetBool(WorldStateConst.HavePartner));
        }
    }

    public override void OnClose()
    {
        BtnClose.onClick.RemoveAllListeners();
    }
}
