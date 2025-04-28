using UnityEngine;
using UnityEngine.Playables;

namespace Scripts.TimelineControl.Signal
{
    public class SceneChangeSignalReceiver : MonoBehaviour, INotificationReceiver
    {
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is SceneChangeSignal sceneChangeSignal)
            {
                LevelManager.instance.EnterLevel(sceneChangeSignal.TargetSceneName);
            }
        }
    }
}
