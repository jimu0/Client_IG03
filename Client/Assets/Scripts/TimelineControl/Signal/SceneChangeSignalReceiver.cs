using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.SceneManagement;

public class SceneChangeSignalReceiver : MonoBehaviour, INotificationReceiver
{
    // 实现INotificationReceiver接口
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        // 检查信号类型
        if (notification is SceneChangeSignal sceneChangeSignal)
        {
            // 触发关卡切换
            LoadTargetScene(sceneChangeSignal.TargetSceneName);
        }
    }

    private void LoadTargetScene(string sceneName)
    {
        Debug.Log($"{sceneName}");
        LevelManager.instance.EnterLevel(sceneName);
        LevelManager.instance.NextLevel();
    }
}