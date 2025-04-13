 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;

public static class TimerManager
{
    public static int TimerID = 0;
    private static Dictionary<int, Timer> m_dicTimer;
    public static void Init()
    {
        TimerID = 0;
        m_dicTimer = new Dictionary<int, Timer>();
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
    public static int Register (float duration, System.Action onComplete, System.Action<float> onUpdate = null,
        bool isLooped = false, bool useRealTime = false, MonoBehaviour autoDestroyOwner = null)
    {
        var timer = Timer.Register(duration, onComplete, onUpdate, isLooped, useRealTime, autoDestroyOwner);
        m_dicTimer.Add(++TimerID, timer);
        return TimerID;
    }

    /// <summary>
    /// ȡ��->����
    /// </summary>
    /// <param name="timer"></param>
    public static void Cancel(int timerID)
    {
        Timer timer;
        if (!m_dicTimer.TryGetValue(timerID, out timer))
            return;
        Timer.Cancel(timer);
    }

    /// <summary>
    /// ��ͣ
    /// </summary>
    /// <param name="timer"></param>
    public static void Pause(int timerID)
    {
        Timer timer;
        if (!m_dicTimer.TryGetValue(timerID, out timer))
            return;
        Timer.Pause(timer);
    }

    /// <summary>
    /// �ָ�
    /// </summary>
    /// <param name="timer"></param>
    public static void Resume(int timerID)
    {
        Timer timer;
        if (!m_dicTimer.TryGetValue(timerID, out timer))
            return;
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
