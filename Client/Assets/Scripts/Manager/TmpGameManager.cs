using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ҵ�����������gameObject��
/// </summary>
public class TmpGameManager : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        // ����ģ��ĳ�ʼ��
        StartCoroutine("InitAndStart");
    }

    /// <summary>
    /// ��Ϸ���
    /// </summary>
    private void GameStart()
    {
        Debug.Log("Game Start!");
        // ��Ϸ��ʼ���߼� ��ʾ���˵���  

        //test
        AudioManager.PlayOneShot(AudioConst.Test);
        TimerManager.Register(1f, 
            () => 
            {
                //Debug.Log("Change Scene");
                //AudioManager.StopAllSounds();//�ر���������
                //ResourceManger.LoadSceneAsync("Scene_UIScene", null);

                Debug.Log("Start Story");
                StoryManager.StartStory(10000);
                StoryManager.StartStory(10002);
            });
    }

    private IEnumerator InitAndStart()
    {
        yield return ResourceManger.Init();
        yield return  ConfigManager.Init();

        AudioManager.Init();
        TimerManager.Init();
        StoryManager.Init();
        WorldStateManager.Init();

        GameStart();
    }

    void Update()
    {
        // ��Ϸѭ��
        // �����淨ѭ�����������
    }
}
