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

    public override void OnShow(object param)
    {
        BtnPause.onClick.AddListener(() =>
        {
            TmpGameManager.instance.Pause();
        });
    }

    public override void OnClose()
    {
        BtnPause.onClick.RemoveAllListeners();
    }
}
