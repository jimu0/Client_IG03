using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using FMODUnity;

public static class AudioManager
{
    private static FMOD.Studio.System m_studioSystem;
    private static FMOD.System m_coreSystem;

    private static EventInstance levelBgmInstance;
    public static void Init()
    {
        //FMODUnity.RuntimeManager.LoadBank("Master");
        Debug.Log("AudioManager IsInitialized" + FMODUnity.RuntimeManager.IsInitialized);

        m_studioSystem = FMODUnity.RuntimeManager.StudioSystem;
        m_coreSystem = FMODUnity.RuntimeManager.CoreSystem;

        //InitEventParameter();

        levelBgmInstance = FMODUnity.RuntimeManager.CreateInstance(AudioConst.LevelBGM);
    }

    public static void PlayLevelBGM()
    {
        PLAYBACK_STATE state;
        levelBgmInstance.getPlaybackState(out state);
        if (state == PLAYBACK_STATE.PLAYING)
            return;
        levelBgmInstance.start();
    }

    public static void StopLevelBGM()
    {
        levelBgmInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public static void PlayBtnClick()
    {
        PlayOneShot(AudioConst.BtnClick);
    }

    /// <summary>
    /// ���ŵ������������ò�����
    /// </summary>
    /// <param name="path"></param>
    /// <param name="transform">��Դ����</param>
    public static void PlayOneShotEvent(string path, Transform transform)
    {
        FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(path);
        if (transform != null)
        {
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(instance, transform);
        }

        // ���ò���
        //instance.setParameterByName();

        instance.start();
        instance.release();
    }

    /// <summary>
    /// ���ŵ�������������Ҫ���ò����ģ�
    /// λ��
    /// </summary>
    /// <param name="path"></param>
    /// <param name="position"></param>
    public static void PlayOneShot(string path, Vector3 position = default)
    {
        FMODUnity.RuntimeManager.PlayOneShot(path, position);
    }

    /// <summary>
    /// ���ŵ�������������Ҫ���ò����ģ�
    /// ������Ϸ�����ƶ�
    /// </summary>
    /// <param name="path"></param>
    /// <param name="gameObject"></param>
    public static void PlayOneShotAttached(string path, GameObject gameObject)
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached(path, gameObject);
    }

    // �������ñ��ʼ�� EventDescription?
    private static void InitEventParameter()
    {
        // var path = "";
        // var description = FMODUnity.RuntimeManager.GetEventDescription(path);
    }

    /// <summary>
    /// ֹͣ��������
    /// </summary>
    /// <param name="musicInstance"></param>
    public static void StopPlaying(EventInstance musicInstance)
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
    }
    
    // ֹͣ���������¼�
    public static void StopAllSounds()
    {
        Bus masterBus = RuntimeManager.GetBus("bus:/");
        masterBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }
}
