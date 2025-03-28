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
        ResourceManger.LoadSceneAsync("Scene_NPRscence", null);
    }

    private IEnumerator InitAndStart()
    {
        yield return ResourceManger.Init();
        yield return  ConfigManager.Init();

        GameStart();
    }

    void Update()
    {
        // ��Ϸѭ��
        // �����淨ѭ�����������
    }
}
