 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;

public static class TimerManager
{
    public static void Init()
    {
    }

    /// <summary>
    /// ������timer
    /// �л�����ʱ����timer�����Զ�����
    /// </summary>
    /// <param name="duration">����</param>
    /// <param name="onComplete">���ڽ�������</param>
    /// <param name="onUpdate">ÿ֡����</param>
    /// <param name="isLooped">ѭ������</param>
    /// <param name="useRealTime">true������time scaleӰ��</param>
    /// <param name="autoDestroyOwner">������object��object����ʱ��timerҲ�Զ�����</param>
    /// <returns></returns>
    public static Timer Register (float duration, System.Action onComplete, System.Action<float> onUpdate = null,
        bool isLooped = false, bool useRealTime = false, MonoBehaviour autoDestroyOwner = null)
    {
        return Timer.Register(duration, onComplete, onUpdate, isLooped, useRealTime, autoDestroyOwner);
    }

    /// <summary>
    /// ȡ��->����
    /// </summary>
    /// <param name="timer"></param>
    public static void Cancel(Timer timer)
    {
        Timer.Cancel(timer);
    }

    /// <summary>
    /// ��ͣ
    /// </summary>
    /// <param name="timer"></param>
    public static void Pause(Timer timer)
    {
        Timer.Pause(timer);
    }

    /// <summary>
    /// �ָ�
    /// </summary>
    /// <param name="timer"></param>
    public static void Resume(Timer timer)
    {
        Timer.Resume(timer);

    }

    public static void CancelAllRegisteredTimers()
    {
        Timer.CancelAllRegisteredTimers();
    }

    public static void PauseAllRegisteredTimers()
    {
        Timer.PauseAllRegisteredTimers();
    }

    public static void ResumeAllRegisteredTimers()
    {
        Timer.ResumeAllRegisteredTimers();
    }
}
