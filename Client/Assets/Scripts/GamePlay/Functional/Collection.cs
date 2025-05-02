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
        m_collider.bounds.SetMinMax(transform.position, transform.position + Vector3.one);
        m_collider.isTrigger = true;
        gameObject.layer = LayerMask.NameToLayer("Checkpoint");
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((PlayerManager.instance.GetLayerMask(ELayerMaskUsage.TriggerForPlayer) & (1 << other.gameObject.layer)) == 0)
            return;

        PlayerManager.instance.Collecte(levelIndex, number);
    }
}
