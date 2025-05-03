using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Switch : MonoBehaviour
{
    public int triggerExistCount = 0;
    public UnityEvent SwitchOnEvent;
    public UnityEvent SwitchOffEvent;

    void Start()
    {
    }

    public void DoSwitch(int stayCount)
    {
        if (stayCount > 0 && triggerExistCount == 0)
            SwitchOnEvent?.Invoke();
        else if (stayCount == 0 && triggerExistCount > 0)
            SwitchOffEvent?.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        triggerExistCount++;
    }

    private void OnTriggerExit(Collider other)
    {
        triggerExistCount--;
    }
}
