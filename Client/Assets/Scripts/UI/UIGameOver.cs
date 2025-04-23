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
public class UIGameOver : UIBase
{
    public Text text_title;
    public Button BtnResume;

    public override void OnShow(object param)
    {
        BtnResume.onClick.AddListener(() =>
        {
            TmpGameManager.instance.Resume();
            Close();
        });
    }

    public override void OnClose()
    {
        
    }
}
