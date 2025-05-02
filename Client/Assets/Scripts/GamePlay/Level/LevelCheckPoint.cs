using System.Collections;
using System.Collections.Generic;
using Scripts.TimelineControl.SceneFX;
using UnityEngine;


public class LevelCheckPoint : MonoBehaviour
{
    public enum ECheckPointType
    {
        LevelStart, 
        LevelComplete,
    }

    public ECheckPointType type;
    public string levelSceneName;
    private Collider m_collider;
    void Start()
    {
        if (!TryGetComponent<Collider>(out m_collider))
        {
            Debug.LogError(" check point no collider added !");
            return;
        }
        m_collider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.LogError($" OnTriggerEnter ! {other.gameObject.name}  {other.gameObject.layer} {PlayerManager.instance.GetLayerMask(ELayerMaskUsage.TriggerForPlayer)}");
        if (((1 << other.gameObject.layer) & PlayerManager.instance.GetLayerMask(ELayerMaskUsage.TriggerForPlayer)) == 0)
            return;
        
        if (type == ECheckPointType.LevelStart)
        {
            LevelManager.instance.EnterLevel(levelSceneName);
        }
        else if(type == ECheckPointType.LevelComplete)
        {

            SceneFXController.Instance.FadeOutFX(() =>
            {
                LevelManager.instance.CompleteLevel(levelSceneName); 
                LevelManager.instance.NextLevel();
            });

        }
        

    }
}
