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
public class UITalk : UIBase
{
    //public TextMeshProUGUI text_title;
    public Text text_speaker;
    public Text text_content;
    public Button btn_next;

    /// <summary>
    /// 单个字出现间隔
    /// </summary>
    public float singleTextAppearTime = 0.2f;

    private int m_curIndex = -1;
    List<StoryCfg> m_storyList;

    public override void OnShow(object param)
    {
        List<StoryCfg> storyList = param as List<StoryCfg>;
        if (storyList == null || storyList.Count == 0)
        {
            Close();
            return;
        }

        m_storyList = storyList;
        m_curIndex = 0;
        btn_next.onClick.AddListener(ClickBtnNext);
        ShowContent();
    }

    public override void OnClose()
    {
        btn_next.onClick.RemoveAllListeners();
    }

    public void ShowContent()
    {
        text_speaker.text = m_storyList[m_curIndex].Speaker;
        text_content.text = "";
        float time = m_storyList[m_curIndex].Content.Length / 3 * singleTextAppearTime;
        text_content.DOText(m_storyList[m_curIndex].Content, time);
        m_curIndex++;
    }

    public void ClickBtnNext()
    {
        if (m_curIndex < m_storyList.Count)
            ShowContent();
        else
            Close();
    }
}
