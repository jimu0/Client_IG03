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
public class UITitle : UIBase
{
    //public TextMeshProUGUI text_title;
    public Text text_title;

    Sequence seq;
    int timerID;
    public override void OnShow(object param)
    {
        List<StoryCfg> storyList = param as List<StoryCfg>;
        if (storyList == null || storyList.Count == 0)
        {
            Close();
            return;
        }

        Color color1 = new Color(1, 1, 1, 1);
        Color color2 = new Color(1, 1, 1, 0);
        text_title.color = color2;

        seq = DOTween.Sequence();
        seq.Append(text_title.DOColor(color1, 0.5f));
        seq.AppendInterval(1f);
        seq.Append(text_title.DOColor(color2, 0.5f));
    
        text_title.text = storyList[0].Content;

        timerID = TimerManager.Register(2.5f, () =>
        {
            Close();
        });
    }

    public override void OnClose()
    {
        seq.Kill();
        TimerManager.Cancel(timerID);
    }
}
