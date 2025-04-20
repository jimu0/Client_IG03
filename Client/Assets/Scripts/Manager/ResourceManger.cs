using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using UnityEngine.SceneManagement;

public static class ResourceManger
{
    private static ResourcePackage package;
    private static string defaultPackageName = "DefaultPackage";

    private static Dictionary<string, AssetOperationHandle> m_dicHandle;

    public static IEnumerator Init()
    {
        m_dicHandle = new Dictionary<string, AssetOperationHandle>();

        YooAssets.Initialize();
        package = YooAssets.CreatePackage(defaultPackageName);
        YooAssets.SetDefaultPackage(package);

#if UNITY_EDITOR
        var initParameters = new EditorSimulateModeParameters();
        initParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(defaultPackageName);
        yield return package.InitializeAsync(initParameters);
#else
        var initParameters = new OfflinePlayModeParameters();
        yield return package.InitializeAsync(initParameters);
#endif

        //Debug.Log("YooAssets"+ YooAssets.GetPackage(defaultPackageName));

        //LoadAllAssetsSync<Shader>("Shader_Lit");
    }

    /// <summary>
    /// �첽���س���
    /// </summary>
    /// <param name="location"></param>
    /// <param name="callback">����������ɻص�</param>
    /// <param name="sceneMode">������/�ӳ���</param>
    /// <param name="suspendLoad">Ԥ���س���</param>
    /// <param name="priority"></param>
    public static void LoadSceneAsync(string location, System.Action callback, LoadSceneMode sceneMode = LoadSceneMode.Single, bool suspendLoad = false, int priority = 100)
    {
        var handler = package.LoadSceneAsync(location, sceneMode, suspendLoad, priority);
        handler.Completed += (SceneOperationHandle _handle) =>
        {
            //UnloadUnusedRes();
            callback?.Invoke();
        };
    }

    /// <summary>
    /// ͬ��������Դ
    /// ʹ�ó�����С��Դ �� ʵʱ��Ҫ��ߵ���Դ
    /// ע�⣺sprite��Դʹ�� LoadSubRes �ӿ� 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="location"></param>
    /// <returns></returns>
    public static T LoadResSync<T>(string location) where T : UnityEngine.Object
    {
        var handler = package.LoadAssetSync<T>(location);
        var res = handler.GetAssetObject<T>();
        handler.Release();
        return res;
    }

    /// <summary>
    /// �첽����
    /// ʹ�ó�����Ԥ���� �� ���ؽϴ���Դ
    /// ע�⣺sprite��Դʹ�� LoadSubRes �ӿ� 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="location"></param>
    /// <param name="callback"></param>
    public static void LoadResAsync<T>(string location, System.Action<T> callback) where T : UnityEngine.Object
    {
        var handler = package.LoadAssetAsync(location);
        handler.Completed += (AssetOperationHandle _handle) =>
        {
            callback?.Invoke(_handle.GetAssetObject<T>());
            handler.Release();
        };
    }

    /// <summary>
    /// ͬ�������Ӷ���
    /// sprite
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="location"></param>
    /// <param name="subResName"></param>
    /// <returns></returns>
    public static T LoadSubResSync<T>(string location, string subResName) where T : UnityEngine.Object
    {
        var handler = package.LoadSubAssetsSync<T>(location);
        var res = handler.GetSubAssetObject<T>(subResName);
        handler.Release();
        return res;
    }

    /// <summary>
    /// �첽�����Ӷ���
    /// sprite
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="location"></param>
    /// <param name="subResName"></param>
    /// <returns></returns>
    public static void LoadSubResAsync<T>(string location, string subResName, System.Action<T> callback) where T : UnityEngine.Object
    {
        var handler = package.LoadSubAssetsAsync<T>(location);
        handler.Completed += (SubAssetsOperationHandle _handle) =>
        {
            callback?.Invoke(_handle.GetSubAssetObject<T>(subResName));
            handler.Release();
        };
    }

    /// <summary>
    /// ͬ������bundle��������Դ
    /// ���������ļ�����
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="location"></param>
    /// <returns></returns>
    public static T[] LoadAllAssetsSync<T>(string location) where T : UnityEngine.Object
    {
        var handler = package.LoadAllAssetsSync<T>(location);
        var res = handler.AllAssetObjects as T[];
        handler.Release();
        return res;
    }

    /// <summary>
    /// �첽����bundle��������Դ
    /// ���������ļ�����
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="location"></param>
    /// <param name="callback"></param>
    public static void LoadAllAssetsAsync<T>(string location, System.Action<T[]> callback) where T : UnityEngine.Object
    {
        var handler = package.LoadAllAssetsAsync<T>(location);
        handler.Completed += (AllAssetsOperationHandle _handle) =>
        {
            callback?.Invoke(handler.AllAssetObjects as T[]);
            handler.Release();
        };
    }

    /// <summary>
    /// �г�������
    /// </summary>
    public static void UnloadUnusedRes()
    {
        package.UnloadUnusedAssets();
    }

}
