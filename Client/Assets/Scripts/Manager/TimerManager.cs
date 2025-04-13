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
    /// 创建新timer
    /// 切换场景时所有timer都会自动销毁
    /// </summary>
    /// <param name="duration">周期</param>
    /// <param name="onComplete">周期结束调用</param>
    /// <param name="onUpdate">每帧调用</param>
    /// <param name="isLooped">循环调用</param>
    /// <param name="useRealTime">true：不受time scale影响</param>
    /// <param name="autoDestroyOwner">关联的object，object销毁时，timer也自动销毁</param>
    /// <returns></returns>
    public static int Register (float duration, System.Action onComplete, System.Action<float> onUpdate = null,
        bool isLooped = false, bool useRealTime = false, MonoBehaviour autoDestroyOwner = null)
    {
        var timer = Timer.Register(duration, onComplete, onUpdate, isLooped, useRealTime, autoDestroyOwner);
        m_dicTimer.Add(++TimerID, timer);
        return TimerID;
    }

    /// <summary>
    /// 取消->销毁
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
    /// 暂停
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
    /// 恢复
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
