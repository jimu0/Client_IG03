using System.Collections.Generic;
using UnityEngine;

public class FunctionalSwitch : MonoBehaviour, I_ElectricWire
{
    public enum SwitchAnimation { SwitchON, SwitchOFF }

    [Header("动画设置")]
    [SerializeField] private Animation animationComponent;
    [SerializeField] private List<AnimationClip> animationClips;

    [Header("电力参数")]
    public int KwhOwn;
    public int kwhCondition;

    [Header("关联机关")]
    [SerializeField] private List<MonoBehaviour> linkedDevices = new();
    private List<I_ElectricWire> activatables = new();

    public bool IsActive { get; private set; }
    public int Kwh { get; set; }

    private void Awake()
    {
        if (animationComponent == null) animationComponent = GetComponent<Animation>();

        // 验证、收集可激活组件
        foreach (var device in linkedDevices)
        {
            if (device is I_ElectricWire activatable) activatables.Add(activatable);
            else Debug.LogWarning($"对象 {device.name} 未实现 I_ElectricWire 接口", device);
        }
    }

    private void Start() => Charge(KwhOwn);

    // 播放动画
    private void PlayAnimation(SwitchAnimation animationType)
    {
        int index = (int)animationType;
        if (animationClips != null && index < animationClips.Count) animationComponent.Play(animationClips[index].name);
        else Debug.LogError($"动画索引 {index} 无效或未赋值！");
    }

    public void ON()
    {
        if (Kwh < kwhCondition) return;
        
        if (!IsActive) PlayAnimation(SwitchAnimation.SwitchON);
        IsActive = true;
        foreach (var device in activatables)
        {
            if (Kwh > 0)
            {
                device.Charge(1);
                Kwh -= 1;
            }
            if (!device.IsActive) device.ON();
        }
        //DistributePower(1, activateDevice: true);
    }

    public void OFF()
    {
        if (Kwh >= kwhCondition) return;

        if (IsActive) PlayAnimation(SwitchAnimation.SwitchOFF);

        IsActive = false;
        foreach (var device in activatables)
        {
            if (Kwh < KwhOwn)
            {
                device.Charge(-1);
                Kwh += 1;
            }
            if (device.IsActive) device.OFF();
        }
        //DistributePower(-1, activateDevice: false);
    }

    // 电力分配逻辑
    // private void DistributePower(int powerDelta, bool activateDevice)
    // {
    //     foreach (var device in activatables)
    //     {
    //         if (activateDevice && !device.IsActive) device.ON();
    //         else if (!activateDevice && device.IsActive) device.OFF();
    //
    //         device.Charge(powerDelta);
    //         kwh -= powerDelta;
    //     }
    // }

    public void Charge(int i) => Kwh += i;
}