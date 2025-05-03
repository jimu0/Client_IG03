using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using cfg;
using UnityEngine.UI;
using UnityEngine.U2D;

public class UIMainMenu_LevelItem : MonoBehaviour
{
    public Button Btn;
    public TextMeshProUGUI TextLevelName;
    public TextMeshProUGUI TextLock;

    public Image ImgLevel;
    public Image ImgLock;
    public Image ImgBattery;
       

    public void SetData(string sceneName)
    {
        TextLevelName.text = LevelManager.instance.GetLevelName(sceneName);

        bool unLock = LevelManager.instance.IsLevelUnlock(sceneName);
        ImgLock.gameObject.SetActive(!unLock);
        TextLock.text = $"<color=#FF6969>{PlayerManager.instance.GetScore()}</color>/{LevelManager.instance.GetNeedScore(sceneName)}";
        int index = LevelManager.instance.GetLevelIndex(sceneName);

        ResourceManger.LoadResAsync<SpriteAtlas>("Sprite_SpriteAtlas", (sprAtlas) =>
        {
            if (sprAtlas == null)
                return;
            ImgLevel.sprite = sprAtlas.GetSprite($"img_level_{index}");
            ImgBattery.sprite = sprAtlas.GetSprite("img_battery");
        });

        
        Btn.onClick.RemoveAllListeners();
        Btn.onClick.AddListener(() =>
        {
            if (!unLock)
                return;
            AudioManager.PlayBtnClick();
            TmpGameManager.instance.StartPlay(sceneName);
        });
    }
}
