using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableObject : MonoBehaviour
{
    private Collider collider;
    public void Start()
    {
        collider = GetComponent<Collider>();
    }

    public void OnHold()
    {
        collider.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("Holding");
    }

    public void OnPut()
    {
        collider.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Holdable");
    }
}
