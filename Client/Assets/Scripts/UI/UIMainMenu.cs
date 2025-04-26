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
    public Button BtnLevel;
    public Button BtnExit;

    public GameObject UILevel;

    public Button BtnCloseLevel;
    public GameObject LevelItem;
    public Transform ScrollViewContent;

    private List<GameObject> m_levelItems = new List<GameObject>();

    public override void OnShow(object param)
    {
        BtnStart.onClick.AddListener(() =>
        {
            TmpGameManager.instance.StartPlay();
        });

        BtnLevel.onClick.AddListener(() =>
        {
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

        UpdateLevel();
    }

    public override void OnClose()
    {
        BtnStart.onClick.RemoveAllListeners();
        BtnExit.onClick.RemoveAllListeners();
        BtnStart.onClick.RemoveAllListeners();
        BtnCloseLevel.onClick.RemoveAllListeners();

        LevelItem.SetActive(false);
        foreach (var item in m_levelItems)
        {
            Destroy(item);
        }
    }

    public void UpdateLevel()
    {
        foreach (var levelName in LevelManager.instance.sceneNameArray)
        {
            var item = GameObject.Instantiate(LevelItem);
            item.transform.SetParent(ScrollViewContent, false);
            item.SetActive(true);
            var text = item.GetComponentInChildren<TextMeshProUGUI>();
            text.text = levelName;

            var btn = item.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                TmpGameManager.instance.StartPlay(levelName);
            });
        }
    }
}
