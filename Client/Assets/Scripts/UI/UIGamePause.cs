    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using cfg;
using UnityEngine.UI;
using DG.Tweening;
    using Scripts.TimelineControl.SceneFX;

    /// <summary>
/// UI????
/// </summary>
public class UIGamePause : UIBase
{
    //public TextMeshProUGUI text_title;
    public Button BtnResume;
    public Button BtnRestart;
    public Button BtnTutorial;
    public Button BtnExit;


    public override void OnShow(object param)
    {
        BtnResume.onClick.AddListener(() =>
        {
            AudioManager.PlayBtnClick();
            TmpGameManager.instance.Resume();
            Close();
        });

        BtnRestart.onClick.AddListener(() =>
        {
            AudioManager.PlayBtnClick();
            SceneFXController.Instance.FadeOutFX(() =>
            {
                TmpGameManager.instance.ResetLevel();
            });
            Close();
        });

        BtnTutorial.onClick.AddListener(() =>
        {
            AudioManager.PlayBtnClick();
            UIManager.Instance.Show("UITutorial");
        });

        BtnExit.onClick.AddListener(() =>
        {
            AudioManager.PlayBtnClick();
            SceneFXController.Instance.FadeOutFX(() =>
            {
                TmpGameManager.instance.BackToMenu();
            });

            Close();
        });
    }

    public override void OnClose()
    {
        BtnResume.onClick.RemoveAllListeners();
        BtnRestart.onClick.RemoveAllListeners();
        BtnTutorial.onClick.RemoveAllListeners();
        BtnExit.onClick.RemoveAllListeners();
    }
}
