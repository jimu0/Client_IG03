using System.Collections;
using System.Collections.Generic;
using Scripts.TimelineControl.SceneFX;
using UnityEngine;

public class LevelDirector : MonoBehaviour
{
    public string levelSceneName;
    public int levelIndex;
    public List<LevelCheckPoint> checkPoints;
    public Vector3 playerPosition;
    public Transform LevelOrigin;

    [Header("箱子")]
    public List<Box> boxes;
    public List<Vector3> boxPositionList;

    [Header("收集物")]
    public List<Collection> collections;

    public RigidbodyConstraints[] constraints;
    
    private SceneFXController sceneFX;
    // todo 其他机关状态等

    
    private RigidbodyConstraints m_constraints;
    private void Awake()
    {
        foreach (var item in checkPoints)
        {
            item.levelSceneName = levelSceneName;
        }

        levelIndex = LevelManager.instance.GetLevelIndex(levelSceneName);
        //foreach (var item in constraints)
        //{
        //    m_constraints |= item;
        //}
        // 先写死
        m_constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        //Debug.Log($"LevelDirector {levelSceneName} m_constraints {m_constraints}");

        for (int i = 0; i < collections.Count; i++)
        {
            collections[i].levelIndex = levelIndex;
            collections[i].number = i;
        }

        LevelManager.instance.RegistLevelDirector(this);

    }

    private void OnEnable()
    {
        LevelManager.instance.RegistLevelDirector(this);
    }

    private void OnDisable()
    {
        LevelManager.instance.UnregistLevelDirector(this);
    }

    private void OnDestroy()
    {
        LevelManager.instance.UnregistLevelDirector(this);
    }

    public void ResetLevel()
    {
        PlayerManager.instance.TeleportPlayerAndReset(playerPosition + new Vector3(0.5f, 0.5f, 0) + LevelOrigin.position, m_constraints);
        for (int i = 0; i < boxes.Count; i++)
        {
            boxes[i].SetPostion(boxPositionList[i] + LevelOrigin.position);
        }

        for (int i = 0; i < collections.Count; i++)
        {
            collections[i].gameObject.SetActive(LevelManager.instance.GetCollectShow(levelIndex, i));
        }
    }

    public void LevelStart()
    {
        // 剧情表现等
        SceneFXController.Instance.FadeInFX(() =>
        {
            
        });
        // todo 根据存档进度做一次性的播放
        
    }

    
}
