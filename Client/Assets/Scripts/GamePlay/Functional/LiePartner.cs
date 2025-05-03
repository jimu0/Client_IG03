using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiePartner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(!WorldStateManager.State.GetBool(WorldStateConst.HavePartner));
    }

    private void OnTriggerEnter(Collider other)
    { 
        PlayerManager.instance.GetPartner();
        gameObject.SetActive(!WorldStateManager.State.GetBool(WorldStateConst.HavePartner));
    }
}
