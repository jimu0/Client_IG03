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
public class UIGamePause : UIBase
{
    //public TextMeshProUGUI text_title;
    public Button BtnResume;
    public Button BtnRestart;
    public Button BtnExit;


    public override void OnShow(object param)
    {
        BtnResume.onClick.AddListener(() =>
        {
            TmpGameManager.instance.Resume();
        });

        BtnRestart.onClick.AddListener(() =>
        {
            LevelManager.instance.ResetLevel();
            Close();
        });

        BtnExit.onClick.AddListener(() =>
        {
            TmpGameManager.instance.BackToMenu();
        });
    }

    public override void OnClose()
    {
        BtnResume.onClick.RemoveAllListeners();
        BtnRestart.onClick.RemoveAllListeners();
        BtnExit.onClick.RemoveAllListeners();
    }
}
