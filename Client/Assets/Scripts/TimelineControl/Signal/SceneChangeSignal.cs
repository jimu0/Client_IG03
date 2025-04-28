using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Scripts.TimelineControl.Signal
{
    [CreateAssetMenu(fileName = "SceneChangeSignal", menuName = "Signals/SceneChangeSignal")]
    public class SceneChangeSignal : Marker, INotification
    {
        public PropertyName id => new PropertyName("SceneChangeSignal");
        public string TargetSceneName; // 暴露给Timeline编辑器的字段
    }
}

