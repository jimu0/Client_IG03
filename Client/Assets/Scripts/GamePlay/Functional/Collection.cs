using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collection : MonoBehaviour
{
    //levelDirector ¸³Öµ
    public int number;
    public int levelIndex;

    private Collider m_collider;

    private void Start()
    {
        m_collider = gameObject.AddComponent<BoxCollider>();
        m_collider.bounds.SetMinMax(transform.position - Vector3.one * 0.5f, transform.position + Vector3.one * 0.5f);
        m_collider.isTrigger = true;
        gameObject.layer = LayerMask.NameToLayer("CheckPoint");
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((PlayerManager.instance.GetLayerMask(ELayerMaskUsage.TriggerForPlayer) & (1 << other.gameObject.layer)) == 0)
            return;

        PlayerManager.instance.Collecte(levelIndex, number);
        gameObject.SetActive(false);

        
        ResourceManger.LoadResAsync<GameObject>("FX_Flash_blue_purple", (obj) =>
        {
            if (obj == null)
                return;

            var fxObj = GameObject.Instantiate(obj);
            //fxObj.transform.SetParent(transform.parent);
            fxObj.transform.position = transform.position;
            fxObj.transform.localScale = Vector3.one * 0.05f;
            TimerManager.Register(1f, () =>
            {
                Destroy(fxObj);
            });
        });
    }
}
