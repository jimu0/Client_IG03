using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotHoldHandler : MonoBehaviour
{
    [SerializeField]
    private Joint fixedJoint;
    private HashSet<HoldableObject> canTakeObjs = new HashSet<HoldableObject>();
    private HoldableObject holdingObj;

    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        TakeOrPutObject();
    }

    private void TakeOrPutObject()
    {
        // ÄÃÆð
        if (Input.GetMouseButtonDown(0))
        {
            if (holdingObj != null)
                return;

            if (canTakeObjs.Count == 0)
                return;

            HoldableObject obj = null;
            foreach (var item in canTakeObjs)
            {
                obj = item;
                break;
            }
            canTakeObjs.Remove(obj);
            holdingObj = obj;

            holdingObj.OnHold();
            fixedJoint.connectedBody = obj.GetComponent<Rigidbody>();
        }

        // ·ÅÏÂ
        if (Input.GetMouseButtonDown(1))
        {
            if (holdingObj == null)
                return;

            holdingObj.OnPut();
            fixedJoint.connectedBody = null;
            holdingObj = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HoldableObject holdableObject = other.GetComponent<HoldableObject>();
        if (holdableObject == null)
            return;

        canTakeObjs.Add(holdableObject);
    }

    private void OnTriggerExit(Collider other)
    {
        HoldableObject holdableObject = other.GetComponent<HoldableObject>();
        if (holdableObject == null)
            return;

        canTakeObjs.Remove(holdableObject);
    }
}
