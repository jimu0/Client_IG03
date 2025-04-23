using System;
using System.Collections.Generic;
using Tools.StaticLightBakingTool.Baking;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tools.StaticLightBakingTool.Editor
{
    [CustomEditor(typeof(BakingBoxMonoBehaviour))]
    public class BakingBoxMonoBehaviourInspector : UnityEditor.Editor
    {
        private SerializedObject obj; 
        private BakingBoxMonoBehaviour script;
        private SerializedProperty lightmapAllowance;
        private SerializedProperty realTimeMonitoring;
        private SerializedProperty some;
        private SerializedProperty specialRes;
        private readonly List<Color> texBrightColor = new();
        [Range(0, 2)] private readonly List<float> texBrightValue = new();

        //private SerializedProperty savePath;
        private const string Text0 = "烘培";
        private const string Text1 = "烘培中...";
        private readonly GUIContent label = new(string.Empty);
        private int bakingState;//烘培状态(0烘培，1烘培中，2烘培结束)
        private bool isPersistent;//是否在场景中
        private Texture2D textureToSave;
        private SerializedProperty textureSizePower;
        private enum ExecuteForgingMode
        {
            Default,
            BakeLighting,
            CompleteFusion,
            TexturesColourCorrection,
            BatchOfObjects,
        }
        private ExecuteForgingMode executeMode = ExecuteForgingMode.Default;
        private bool execute;
        private bool isFoldout1 = true;
        private bool isFoldout2 = true;
        private bool isFoldout3 = true;
        
        void OnEnable()
        {
            script = (BakingBoxMonoBehaviour)target;
            obj = new SerializedObject(target);
            lightmapAllowance = obj.FindProperty("lightmapAllowance");
            realTimeMonitoring = obj.FindProperty("realTimeMonitoring");
            some = obj.FindProperty("some");
            specialRes = obj.FindProperty("specialRes");
            isPersistent = !EditorUtility.IsPersistent(target);
            textureSizePower = obj.FindProperty("textureSizePower");
        }

        public override void OnInspectorGUI()
        {
            Repaint();
            script = (BakingBoxMonoBehaviour)target;
            serializedObject.Update();
            script.Update();
            script.UpdateTexturesCorrectionPanel(texBrightColor, texBrightValue);
            
            BakingLampSetting();
            EditorGUILayout.Space(30);
            BakingLampTool();
            EditorGUILayout.Space(30);
            
            SetParameters(isPersistent);

            serializedObject.ApplyModifiedProperties();// 应用修改
        }

        private void BakingLampSetting()
        {
            isFoldout1 = EditorGUILayout.Foldout(isFoldout1, "烘培设置");
            if (!isFoldout1) return;
            label.text = "张数";
            EditorGUILayout.PropertyField(some,label);
            label.text = "偏差值ֵ";
            EditorGUILayout.PropertyField(lightmapAllowance,label);
            EditorGUILayout.PropertyField(specialRes, isPersistent);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal(GUILayout.Height(30));
            label.text = "自动跟新";
            EditorGUILayout.PropertyField(realTimeMonitoring,label);
            
            EditorGUI.BeginDisabledGroup(bakingState == 1);
            label.text = "分布scaleInLightmap";
            if (GUILayout.Button(label.text,GUILayout.Height(22)))
            {
                ButtonOnClick();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.Height(50));
            EditorGUILayout.Space();
            float buildProgress = Lightmapping.buildProgress;
            string text = bakingState == 1 ? Text1 : script.realTimeMonitoring? $"分布并{Text0}" : Text0;
            EditorGUI.BeginDisabledGroup(Lightmapping.isRunning);
            
            if (GUILayout.Button(text,GUILayout.Height(42)))
            {
                StartBaking(ExecuteForgingMode.BakeLighting);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            
            if (bakingState != 1 || !(buildProgress > 0)) return;
            EditorGUILayout.BeginHorizontal(GUILayout.Height(16));
            EditorGUILayout.Space();
            Rect progressBarRect = EditorGUILayout.GetControlRect(GUILayout.Height(16));
            EditorGUI.ProgressBar(progressBarRect, buildProgress, $"{buildProgress * 100:F0}%");
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }

        private void BakingLampTool()
        {
            isFoldout2 = EditorGUILayout.Foldout(isFoldout2, "资源整理工具");
            if (!isFoldout2) return;
            //ShaderPath();
            //EditorGUILayout.Space();
            SavePath();
            EditorGUILayout.Space();
            CompleteFusion();
            EditorGUILayout.Space();
            TexturesColourCorrection();
            EditorGUILayout.Space();
            BatchOfObjects();
            EditorGUILayout.Space();
        }

        private void ShaderPath()
        {
            EditorGUILayout.BeginHorizontal();
            Shader shader = script.defaultShader;
            Object shaderObj = EditorGUILayout.ObjectField("Shader", shader, typeof(Shader), false);
            script.defaultShaderPath = AssetDatabase.GetAssetPath(shaderObj as Shader);
            script.ShaderPath(script.defaultShaderPath, shader);
            EditorGUILayout.EndHorizontal();
        }

        private void SavePath()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"savePath:{script.savePath}",GUILayout.Height(16));
            if (GUILayout.Button("保存路径",GUILayout.ExpandHeight(true)))
            {
                script.savePath = EditorUtility.OpenFolderPanel("保存到", "", "");
                script.SavePath(script.savePath);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void CompleteFusion()
        {
            EditorGUILayout.BeginVertical();//x
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));//1x
            EditorGUILayout.BeginVertical(GUI.skin.box,GUILayout.ExpandHeight(true));//1xx
            EditorGUILayout.LabelField("资源全展:",GUILayout.Height(16));
            string labelText = "克隆将烘培元素以LightingMap纹理为UV分布方式的新\n整合模型，并输出相关资源";
            EditorGUILayout.LabelField(labelText,GUILayout.Height(36), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndVertical();//1xx
            labelText = bakingState == 1 ? "等待烘培..." : "资源全展";
            EditorGUI.BeginDisabledGroup(Lightmapping.isRunning && !execute);
            if (GUILayout.Button(labelText,GUILayout.Width(150), GUILayout.ExpandHeight(true)))
            {
                script.SetDefaultShader();
                StartBaking(ExecuteForgingMode.CompleteFusion);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();//1x
            EditorGUILayout.EndVertical();//x
        }

        private void TexturesColourCorrection()
        {
            TexturesColourCorrectionBtn();
            TexturesColourCorrectionList(out bool show);
            TexturesColourCorrectionSave(show);
        }

        /// <summary>
        /// 
        /// </summary>
        private void TexturesColourCorrectionBtn()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));//x
            EditorGUILayout.BeginVertical(GUI.skin.box,GUILayout.ExpandHeight(true));//1x
            EditorGUILayout.LabelField("纹理校色:",GUILayout.Height(16));
            string labelText = "克隆将参与烘培物体的纹理，批量调整纹理到统一明度，\n并输出相关资源";
            EditorGUILayout.LabelField(labelText,GUILayout.Height(36), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndVertical();//1x
            labelText = bakingState == 1 ? "等待烘培..." : "纹理校色";
            EditorGUI.BeginDisabledGroup(Lightmapping.isRunning && !execute);//2x
            if (GUILayout.Button(labelText,GUILayout.Width(150), GUILayout.ExpandHeight(true)))
            {
                script.SetDefaultShader();
                StartBaking(ExecuteForgingMode.TexturesColourCorrection);

            }
            EditorGUI.EndDisabledGroup();//2x
            EditorGUILayout.EndHorizontal();//x
        }

        private void TexturesColourCorrectionList(out bool show)
        {
            show = false;
            if (script.textureBrightness is not { Count: > 0 }) return;
            isFoldout3 = EditorGUILayout.Foldout(isFoldout3, "纹理:");
            if (!isFoldout3) return;
            show = true;
            EditorGUILayout.BeginVertical();//x
            for (int i = 0; i < script.textureBrightness.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();//1x
                EditorGUI.BeginChangeCheck();
                string labelText = $"纹理_{i}({script.textureBrightness[i].texName})";
                texBrightValue[i] = EditorGUILayout.Slider(labelText, texBrightValue[i], 0, 2);
                texBrightColor[i] = EditorGUILayout.ColorField(texBrightColor[i], GUILayout.Width(48));
                if (EditorGUI.EndChangeCheck())
                {
                    script.GetTextureBrightness(i, texBrightColor[i], texBrightValue[i]);
                }
                EditorGUILayout.EndHorizontal();//1x
            }
            EditorGUILayout.EndVertical();//x
        }

        private void TexturesColourCorrectionSave(bool show)
        {
            if (!show) return;
            if (script.savePath == "") return;
            EditorGUILayout.BeginHorizontal();//x
            string labelText = "当前在内存中编辑纹理，是否保存到工程中";
            EditorGUILayout.LabelField(labelText,GUILayout.Height(16));
            if (GUILayout.Button("保存",GUILayout.Width(150), GUILayout.ExpandHeight(true)))
            {
                //string saveTexPath = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                script.SaveTextures(script.savePath);
            }
            EditorGUILayout.EndHorizontal();//x


        }
        
        /// <summary>
        /// 资源合批
        /// </summary>
        private void BatchOfObjects()
        {
            EditorGUILayout.BeginVertical();//x
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));//1x
            EditorGUILayout.BeginVertical(GUI.skin.box,GUILayout.ExpandHeight(true));//1xx
            string labelText = "资源合批:";
            EditorGUILayout.LabelField(labelText,GUILayout.Height(16));
            labelText = "克隆将参与烘培物体的纹理合并为指定张纹理，调整模\n型uv与其匹配，并输出相关资源";
            EditorGUILayout.LabelField(labelText,GUILayout.Height(36), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndVertical();//1xx
            labelText = bakingState == 1 ? "等待烘培..." : "资源合批";
            EditorGUI.BeginDisabledGroup(Lightmapping.isRunning && !execute);
            if (GUILayout.Button(labelText,GUILayout.Width(150), GUILayout.ExpandHeight(true)))
            {
                script.SetDefaultShader();
                StartBaking(ExecuteForgingMode.BatchOfObjects);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();//1x
            label.text = $"尺寸:{script.textureSizeResult}*{script.textureSizeResult}";
            EditorGUILayout.PropertyField(textureSizePower,label);
            EditorGUILayout.EndVertical();//x
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="b">许可</param>
        private void SetParameters(bool b)
        {
            if (!b) return;
            
            script.lightmapAllowance = lightmapAllowance.floatValue;
            script.realTimeMonitoring = realTimeMonitoring.boolValue;
            script.some = some.intValue;
            
            //排除项
            Array.Resize(ref script.specialRes, specialRes.arraySize);
            BakingBoxMonoBehaviour.SpecialRes resValue = new();
            if (specialRes.arraySize != 0)
            {
                for (int i = 0; i < specialRes.arraySize; i++)
                {
                    SerializedProperty resValueElement = specialRes.GetArrayElementAtIndex(i);
                    resValue.res = (MeshRenderer)resValueElement.FindPropertyRelative("res").objectReferenceValue;
                    resValue.value = resValueElement.FindPropertyRelative("value").floatValue;
                    script.SetSpecialRe(specialRes.arraySize, i, resValue);
                }
            }
            else
            {
                script.SetSpecialRe(specialRes.arraySize, 0, resValue);
            }

            if (script.textureBrightness.Count != 0)
            {
                for (int i = 0; i < script.textureBrightness.Count; i++)
                {
                    BakingBoxMonoBehaviour.TextureBrightness texValue = script.textureBrightness[i];
                    texValue.color = texBrightColor[i];
                    texValue.value = texBrightValue[i];
                    script.textureBrightness[i] = texValue;
                }
            }
            
            script.textureSizeResult = 1 << textureSizePower.intValue;
            script.textureSizePower = textureSizePower.intValue;
        }

        /// <summary>
        /// 响应界面烘培按钮
        /// </summary>
        private void ButtonOnClick()
        {
            script.SetBakingObjsProperty(script.some, script.lightmapAllowance);
        }
        
        /// <summary>
        /// 开始烘培
        /// </summary>
        private void StartBaking(ExecuteForgingMode mode)
        {
            executeMode = mode;
            switch (executeMode)
            {
                case ExecuteForgingMode.Default:
                    break;
                case ExecuteForgingMode.BakeLighting:
                    script.RemoveCGameObjects();
                    script.SetBakingObjsProperty(script.some, script.lightmapAllowance);
                    break;
                case ExecuteForgingMode.CompleteFusion:
                    script.RemoveCGameObjects();
                    script.SetBakingObjsProperty(script.some, script.lightmapAllowance);
                    break;
                case ExecuteForgingMode.TexturesColourCorrection:
                    script.RemoveCGameObjects();
                    script.SetBakingObjsProperty(script.some, script.lightmapAllowance);
                    break;
                case ExecuteForgingMode.BatchOfObjects:
                    script.RemoveCGameObjects();
                    script.SetBakingObjsProperty(script.some, script.lightmapAllowance);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            bakingState = 0;
            EditorApplication.update += OnUpdate;
            Lightmapping.BakeAsync();//异步进行烘焙
        }

        /// <summary>
        /// 烘培等待结束
        /// </summary>
        private void OnUpdate() 
        {
            if (Lightmapping.isRunning)
            {
                bakingState = 1;
                return;
            }
            bakingState = 2;
            EditorApplication.update -= OnUpdate;
            switch (executeMode)
            {
                case ExecuteForgingMode.Default:
                    break;
                case ExecuteForgingMode.BakeLighting:
                    break;
                case ExecuteForgingMode.CompleteFusion:
                    if (script.CheckResourcesNotEmpty()) break;
                    script.CompleteFusion();
                    script.CSpawnAni();
                    break;
                case ExecuteForgingMode.TexturesColourCorrection:
                    if (script.CheckResourcesNotEmpty()) break;
                    script.TexturesColourCorrection();
                    script.SetTexturesCorrectionPanel();
                    script.CSpawnAni();
                    break;
                case ExecuteForgingMode.BatchOfObjects:
                    if (script.CheckResourcesNotEmpty()) break;
                    script.BatchOfObjects();
                    script.CSpawnAni();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            bakingState = 0;
        }
        
    }
}

