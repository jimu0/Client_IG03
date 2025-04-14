using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class FunctionalSwitch : MonoBehaviour,I_ElectricWire
{
    private Animation animationComponent;
    public int KwhOwn;
    public int kwhCondition;
    [Header("关联机关")]
    [SerializeField] private List<MonoBehaviour> linkedDevices = new ();
    private List<I_ElectricWire> activatables = new ();


    private void Awake()
    {
        animationComponent = GetComponent<Animation>();
        // 验证并收集所有可激活组件
        foreach (var device in linkedDevices)
        {
            if (device is I_ElectricWire activatable)
            {
                activatables.Add(activatable);
            }
            else
            {
                Debug.LogWarning($"对象 {device.name} 未实现 IActivatable 接口", device);
            }
        }
    }

    private void Start()
    {
        Charge(KwhOwn);
    }
    
    public bool IsActive { get; private set; }
    public int kwh { get; set; }
    public void ON()
    {
        if (kwh < kwhCondition) return;
        if(!IsActive) animationComponent.Play("SwitchON");
        IsActive = true;
        foreach (var device in activatables)
        {
            if (kwh > 0)
            {
                device.Charge(1);
                kwh -= 1;
            }
            if (!device.IsActive) device.ON();
        }
    }
    public void OFF()
    {
        if (kwh >= kwhCondition) return;
        if(IsActive) animationComponent.Play("SwitchOFF");
        IsActive = false;

        foreach (var device in activatables)
        {
            if (kwh < KwhOwn)
            {
                device.Charge(-1);
                kwh += 1;
            }
            if (device.IsActive) device.OFF();
        }
    }
    public void Charge(int i)
    {
        kwh += i;
    }
    
}
