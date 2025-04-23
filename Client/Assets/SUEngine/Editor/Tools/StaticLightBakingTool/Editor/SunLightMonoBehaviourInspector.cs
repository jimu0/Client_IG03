using Tools.StaticLightBakingTool.Light;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace Tools.StaticLightBakingTool.Editor
{
    [CustomEditor(typeof(SunLightMonoBehaviour))]
    public class SunLightMonoBehaviourInspector : UnityEditor.Editor
    {
    
        private SerializedObject obj;
        private SunLightMonoBehaviour script;
    
        private SerializedProperty controlMode;
        private SerializedProperty lightMode;
        private SerializedProperty color;
        private SerializedProperty intensity;
        private SerializedProperty time;
        private SerializedProperty azimuthAngle;
        private SerializedProperty altitudeAngle;
        private SerializedProperty axiaTiltAngle;
        private SerializedProperty distance;
        private SerializedProperty radius;
        private SerializedProperty center;
        private bool isFoldout1 = true;
        private bool isFoldout2 = true;
        private bool setToRead;
        private readonly GUIContent label = new(string.Empty);

        private void OnEnable()
        {
            obj = new SerializedObject(target);
            time = obj.FindProperty("time");
            color = obj.FindProperty("color");
            intensity = obj.FindProperty("intensity");
            controlMode = obj.FindProperty("controlMode");
            lightMode = obj.FindProperty("lightMode");
            azimuthAngle = obj.FindProperty("azimuthAngle");
            altitudeAngle = obj.FindProperty("altitudeAngle");
            axiaTiltAngle = obj.FindProperty("axiaTiltAngle");
            distance = obj.FindProperty("distance");
            radius = obj.FindProperty("radius");
            center = obj.FindProperty("center");
        }
    
        public override void OnInspectorGUI()
        {
            script = (SunLightMonoBehaviour)target;
            label.text = "编辑模式";
            EditorGUILayout.PropertyField(controlMode,label);
            setToRead = controlMode.GetEnumValue<SunLightMonoBehaviour.ControlMode>() == SunLightMonoBehaviour.ControlMode.Auto;
            
            EditorGUI.BeginDisabledGroup(!setToRead);
            label.text = $"时间({script.timepiece})";
            EditorGUILayout.PropertyField(time,label);
            EditorGUI.EndDisabledGroup();
            label.text = "光源模式";
            EditorGUILayout.PropertyField(lightMode,label);
            if (lightMode.GetEnumValue<SunLightMonoBehaviour.LightMode>() != SunLightMonoBehaviour.LightMode.Custom && script.monitorLightEnabled && !script.unityLight.enabled)
            {
                lightMode.SetEnumValue(SunLightMonoBehaviour.LightMode.Custom);
            }
            
            isFoldout1 = EditorGUILayout.Foldout(isFoldout1, "光源属性");
            if (isFoldout1)
            {
                EditorGUI.BeginDisabledGroup(setToRead);
                label.text = "方位角";
                EditorGUILayout.PropertyField(azimuthAngle,label);
                label.text = "海拔角";
                EditorGUILayout.PropertyField(altitudeAngle,label);
                EditorGUI.EndDisabledGroup();
                label.text = "轴倾角";
                EditorGUILayout.PropertyField(axiaTiltAngle,label);
                EditorGUI.BeginDisabledGroup(setToRead);
                label.text = "距离";
                EditorGUILayout.PropertyField(distance,label);
                label.text = "半径";
                EditorGUILayout.PropertyField(radius,label);
                EditorGUI.EndDisabledGroup();
                label.text = "偏移";
                EditorGUILayout.PropertyField(center,label);
            }
            EditorGUI.BeginDisabledGroup(setToRead);
            if (lightMode.GetEnumValue<SunLightMonoBehaviour.LightMode>() != SunLightMonoBehaviour.LightMode.Unity)
            {
                isFoldout2 = EditorGUILayout.Foldout(isFoldout2, "基础属性");
                if (isFoldout2)
                {
                    label.text = "颜色";
                    EditorGUILayout.PropertyField(color,label);
                    label.text = "强度";
                    EditorGUILayout.PropertyField(intensity,label);
                }
            }
            EditorGUI.EndDisabledGroup();
            if (setToRead)
            {
                script.AutoLightingLayout(script.time);
                color.colorValue = script.color;
                intensity.floatValue = script.intensity;
                azimuthAngle.floatValue = script.azimuthAngle;
                altitudeAngle.floatValue = script.altitudeAngle;
                //axiaTiltAngle.floatValue = script.axiaTiltAngle;
                distance.floatValue = script.distance;
                radius.floatValue = script.radius;
            }

            script.controlMode = controlMode.GetEnumValue<SunLightMonoBehaviour.ControlMode>();
            script.time = time.floatValue;
            script.timepiece = script.beOnTime(script.time);
            script.lightMode = lightMode.GetEnumValue<SunLightMonoBehaviour.LightMode>();
            script.azimuthAngle = azimuthAngle.floatValue;
            script.altitudeAngle = altitudeAngle.floatValue;
            script.axiaTiltAngle = axiaTiltAngle.floatValue;
            script.distance = distance.floatValue;
            script.radius = radius.floatValue;
            script.center = center.vector3Value;
            script.color = color.colorValue;
            script.intensity = intensity.floatValue;
            
            script.SetLightMode(script.controlMode,script.lightMode);
            script.SetSunLight();
            
        }
        
        void OnSceneGUI()
        {
            script = (SunLightMonoBehaviour)target;
            script.SetSunLight();
            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = Handles.PositionHandle(center.vector3Value, Quaternion.identity);
            if (!EditorGUI.EndChangeCheck()) return;
            center.vector3Value = newPosition;
            script.center = center.vector3Value;

        }
        
    }
}
