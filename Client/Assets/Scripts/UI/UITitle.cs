using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using cfg;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// UI基类
/// </summary>
public class UITitle : UIBase
{
    //public TextMeshProUGUI text_title;
    public Text text_title;

    int timerID;
    public override void OnShow(object param)
    {
        List<StoryCfg> storyList = param as List<StoryCfg>;
        if (storyList == null || storyList.Count == 0)
        {
            Close();
            return;
        }

        text_title.text = storyList[0].Content;
        // 2s后关闭
        timerID = TimerManager.Register(2f, () =>
        {
            Close();
        });
    }

    public override void OnClose()
    {
        TimerManager.Cancel(timerID);
    }
}
