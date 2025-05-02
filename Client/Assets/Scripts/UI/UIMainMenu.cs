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
    public Button BtnContinue;
    public Button BtnLevel;
    public Button BtnExit;

    public GameObject UILevel;

    public Button BtnCloseLevel;
    public UIMainMenu_LevelItem LevelItem;
    public Transform ScrollViewContent;

    private List<UIMainMenu_LevelItem> m_levelItems = new List<UIMainMenu_LevelItem>();

    public override void OnShow(object param)
    {
        BtnStart.onClick.AddListener(() =>
        {
            TmpGameManager.instance.StartPlay();
        });

        BtnContinue.onClick.AddListener(() =>
        {
            TmpGameManager.instance.StartPlay();
        });

        BtnLevel.onClick.AddListener(() =>
        {
            UpdateLevel();
            UILevel.SetActive(true);
        });

        BtnExit.onClick.AddListener(() =>
        {
            TmpGameManager.instance.Exit();
        });

        BtnCloseLevel.onClick.AddListener(() =>
        {
            UILevel.SetActive(false);
        });

        UILevel.SetActive(false);

        var levelIndex = LevelManager.instance.GetPlayLevelIndex();
        BtnStart.gameObject.SetActive(levelIndex == 0);
        BtnContinue.gameObject.SetActive(levelIndex > 0);
        BtnLevel.gameObject.SetActive(levelIndex > 0);
    }

    public override void OnClose()
    {
        BtnStart.onClick.RemoveAllListeners();
        BtnContinue.onClick.RemoveAllListeners();
        BtnLevel.onClick.RemoveAllListeners();
        BtnExit.onClick.RemoveAllListeners();
        BtnCloseLevel.onClick.RemoveAllListeners();
    }

    public void UpdateLevel()
    {
        foreach (var item in m_levelItems)
        {
            GameObject.Destroy(item.gameObject);
        }
        m_levelItems.Clear();

        LevelItem.gameObject.SetActive(false);
        foreach (var cfg in LevelManager.instance.levelCfg)
        {
            var sceneName = cfg.sceneName;
            if (cfg.showInLevelUI)
            {
                var item = GameObject.Instantiate(LevelItem);
                item.transform.SetParent(ScrollViewContent, false);
                item.gameObject.SetActive(true);
                item.SetData(sceneName);
                m_levelItems.Add(item);
            }
        }
    }
}
