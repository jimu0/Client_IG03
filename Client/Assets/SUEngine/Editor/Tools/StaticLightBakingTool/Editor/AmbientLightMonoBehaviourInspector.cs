using System;
using Tools.StaticLightBakingTool.Light;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace Tools.StaticLightBakingTool.Editor
{
    [CustomEditor(typeof(AmbientLightMonoBehaviour))]
    public class AmbientLightMonoBehaviourInspector : UnityEditor.Editor
    {
        private SerializedObject obj;
        private AmbientLightMonoBehaviour script;

        private SerializedProperty axiaTiltAngle;
        private SerializedProperty radius;
        private SerializedProperty center;
        private SerializedProperty mySunLight;
        private SerializedProperty autoCling;
    
        private SerializedProperty mode;
        private SerializedProperty color;
        private SerializedProperty intensity;
        private SerializedProperty texture;

        private readonly GUIContent label = new(String.Empty);
        private bool isFoldout1 = true;
        private bool isFoldout2 = true;

        private void OnEnable()
        {
            obj = new SerializedObject(target);
            axiaTiltAngle = obj.FindProperty("axiaTiltAngle");
            radius = obj.FindProperty("radius");
            center = obj.FindProperty("center");
            mySunLight = obj.FindProperty("mySunLight");
            autoCling = obj.FindProperty("autoCling");
            mode = obj.FindProperty("mode");
            color = obj.FindProperty("color");
            intensity = obj.FindProperty("intensity");
            texture = obj.FindProperty("texture");
        }

        public override void OnInspectorGUI()
        {
            script = (AmbientLightMonoBehaviour)target;
            label.text = "主光源";
            EditorGUILayout.PropertyField(mySunLight,label);
            isFoldout1 = EditorGUILayout.Foldout(isFoldout1, "光源属性");
            if (isFoldout1)
            {
                label.text = "轴倾角";
                EditorGUILayout.PropertyField(axiaTiltAngle,label);
                EditorGUI.BeginDisabledGroup((SunLightMonoBehaviour)mySunLight.objectReferenceValue);
                label.text = "半径";
                EditorGUILayout.PropertyField(radius,label);
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup((SunLightMonoBehaviour)mySunLight.objectReferenceValue&&autoCling.boolValue);
                label.text = "偏移";
                EditorGUILayout.PropertyField(center,label);
                EditorGUI.EndDisabledGroup();
            
                if ((SunLightMonoBehaviour)mySunLight.objectReferenceValue)
                {
                    label.text = "自适应";
                    EditorGUILayout.PropertyField(autoCling,label);
                }
            
            }

            EditorGUI.BeginDisabledGroup((SunLightMonoBehaviour)mySunLight.objectReferenceValue && script.mySunLight&&script.mySunLight.controlMode == 0);
            isFoldout2 = EditorGUILayout.Foldout(isFoldout2, "基础属性");
            if (isFoldout2)
            {
                label.text = "纹理模式";
                EditorGUILayout.PropertyField(mode,label);
                label.text = "颜色";
                EditorGUILayout.PropertyField(color,label);
                label.text = "强度";
                EditorGUILayout.PropertyField(intensity,label);
                if (mode.GetEnumValue<AmbientLightMonoBehaviour.Mode>() == AmbientLightMonoBehaviour.Mode.Texture)
                {
                    label.text = "纹理";
                    EditorGUILayout.PropertyField(texture,label);
                }
            }
            EditorGUI.EndDisabledGroup();
        
            script.mySunLight = (SunLightMonoBehaviour)mySunLight.objectReferenceValue;
            script.autoCling = autoCling.boolValue;
            script.axiaTiltAngle = axiaTiltAngle.floatValue;
            script.radius = radius.floatValue;
            script.center = center.vector3Value;
            script.mode = mode.GetEnumValue<AmbientLightMonoBehaviour.Mode>();
            script.color = color.colorValue;
            script.intensity = intensity.floatValue;
            script.texture = (Texture2D)texture.objectReferenceValue;
            script.SetAmbientLight();
        }

        private void OnSceneGUI()
        {
            script = (AmbientLightMonoBehaviour)target;
            script.SetAmbientLight();
            if (script.autoCling) return;
            //创建可在场景中拖动的控制柄
            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = Handles.PositionHandle(center.vector3Value, Quaternion.identity);
            if (!EditorGUI.EndChangeCheck()) return;
            Undo.RecordObject(script, "Move point");
            center.vector3Value = newPosition;
            script.center = center.vector3Value;

        }
    }
}


