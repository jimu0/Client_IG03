using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using FMODUnity;

public static class AudioManager
{
    private static FMOD.Studio.System m_studioSystem;
    private static FMOD.System m_coreSystem;
    public static void Init()
    {
        //FMODUnity.RuntimeManager.LoadBank("Master");
        Debug.Log("AudioManager IsInitialized" + FMODUnity.RuntimeManager.IsInitialized);

        m_studioSystem = FMODUnity.RuntimeManager.StudioSystem;
        m_coreSystem = FMODUnity.RuntimeManager.CoreSystem;

        //InitEventParameter();
    }

    /// <summary>
    /// 播放单次声音（设置参数）
    /// </summary>
    /// <param name="path"></param>
    /// <param name="transform">音源跟随</param>
    public static void PlayOneShotEvent(string path, Transform transform)
    {
        FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(path);
        if (transform != null)
        {
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(instance, transform);
        }

        // 设置参数
        //instance.setParameterByName();

        instance.start();
        instance.release();
    }

    /// <summary>
    /// 播放单次声音（不需要设置参数的）
    /// 位置
    /// </summary>
    /// <param name="path"></param>
    /// <param name="position"></param>
    public static void PlayOneShot(string path, Vector3 position = default)
    {
        FMODUnity.RuntimeManager.PlayOneShot(path, position);
    }

    /// <summary>
    /// 播放单次声音（不需要设置参数的）
    /// 跟随游戏对象移动
    /// </summary>
    /// <param name="path"></param>
    /// <param name="gameObject"></param>
    public static void PlayOneShotAttached(string path, GameObject gameObject)
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached(path, gameObject);
    }

    // 根据配置表初始化 EventDescription?
    private static void InitEventParameter()
    {
        // var path = "";
        // var description = FMODUnity.RuntimeManager.GetEventDescription(path);
    }

    /// <summary>
    /// 停止播放声音
    /// </summary>
    /// <param name="musicInstance"></param>
    public static void StopPlaying(EventInstance musicInstance)
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
    }
    
    // 停止所有声音事件
    public static void StopAllSounds()
    {
        Bus masterBus = RuntimeManager.GetBus("bus:/");
        masterBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }
}
