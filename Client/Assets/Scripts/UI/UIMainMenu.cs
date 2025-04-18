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
public class UIMainMenu : UIBase
{
    //public TextMeshProUGUI text_title;
    public Button BtnStart;
    public Button BtnExit;

    public override void OnShow(object param)
    {
        BtnStart.onClick.AddListener(() =>
        {
            TmpGameManager.instance.StartPlay();
        });

        BtnExit.onClick.AddListener(() =>
        {
            TmpGameManager.instance.Exit();
        });
    }

    public override void OnClose()
    {
        BtnStart.onClick.RemoveAllListeners();
        BtnExit.onClick.RemoveAllListeners();
    }
}
