using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StarUnion_EfficientAnimation;
using UnityEditor;
using UnityEngine;

namespace StarUnion_EfficientAnimation_Tool
{
    [CustomEditor(typeof(EfficientAnimationPlayer))]
    public class EfficientAnimationPlayerEditor : Editor
    {
        private EfficientAnimationPlayer _player;
        private EfficientAnimationPlayerAgent _playerAgent;
        private AnimationCollectionInfo animationCollectionInfo;
        private Dictionary<string, List<string>> roleAnimationNameDictionary;
        private Mesh _mesh;
        private Material _material;
        private string[] roleNames;
        private string[] animationNames;
        public int roleIdPlayNow;
        public int animationIdPlayNow;
        public string thisRoleName;
        public string thisAnimationName;
        private bool loop = true;
        private static EfficientAnimationShaderType playShaderType;
        private float loopOffset = 0.0f;
        private float speedScale = 1.0f;
        private Color color = Color.white;
        private float colorStrength = 1;
        private bool _initRoleAndAnimationName = false;
        private bool wantPlay = false;
        private bool notInScene = false;
        private bool redefineEffects;
        private EfficientAnimationShaderType _redefineShaderType = (EfficientAnimationShaderType)(-1);
        private EfficientAnimationShaderType _redefineInitShaderType = EfficientAnimationShaderType.Base;
        
        private static bool _zWrite = true;
        private static bool _zTest = true;
        private static float _cutOff = 0.1f;
        private static Texture2D _dissipationTexture;

        private static Color _DissolveEdgeColor = new Color(1.0f,0.0f,0.0f,1.0f);
        private static float _DissolveEdgeWidth = 0.2f;
        private static float _DissolveTime = 1.0f;
        private static Vector2 _DissolveTiling = new Vector2(0.5f, 0.5f);
        private static Color _FlashColor = new Color(1.0f, 0.6f, 0.07f, 1.0f);
        private static float _FlashTime = 0.2f;
        private static float _ScaleStrength = 1.1f;
        private static float _ScaleTime = 0.1f;
        private static float _EffectTime;
        
        public void Awake()
        {
            _player = (EfficientAnimationPlayer) target;
            if (_player.gameObject.scene.name == null)
            {
                notInScene = true;
                return;
            }
            thisRoleName = _player.thisRoleName;
            thisAnimationName = _player.thisAnimationName;
            roleIdPlayNow = _player.roleIdPlayNow;
            animationIdPlayNow = _player.animationIdPlayNow;
            animationCollectionInfo = _player.animationCollectionInfo;

            if (animationCollectionInfo != null)
            {
                for (int i = 0; i < animationCollectionInfo.effectMaterials.Count; i++)
                {
                    if (animationCollectionInfo.effectMaterials[i] == null)
                    {
                        continue;
                    }
                    if (animationCollectionInfo.effectMaterials[i].IsKeywordEnabled("_DISSIPATION"))
                    {
                        _DissolveEdgeColor = animationCollectionInfo.effectMaterials[i].GetColor("_DissolveEdgeColor");
                        _DissolveEdgeWidth = animationCollectionInfo.effectMaterials[i].GetFloat("_DissolveEdgeWidth");
                        _DissolveTime = animationCollectionInfo.effectMaterials[i].GetFloat("_DissolveTime");
                        _DissolveTiling = animationCollectionInfo.effectMaterials[i].GetTextureScale("_DissolveTex");
                    }
                    if (animationCollectionInfo.effectMaterials[i].IsKeywordEnabled("_FLASH"))
                    {
                        _FlashColor = animationCollectionInfo.effectMaterials[i].GetColor("_FlashColor");
                        _FlashTime = animationCollectionInfo.effectMaterials[i].GetFloat("_FlashTime");
                    }
                    if (animationCollectionInfo.effectMaterials[i].IsKeywordEnabled("_SCALE"))
                    {
                        _ScaleStrength = animationCollectionInfo.effectMaterials[i].GetFloat("_ScaleStrength");
                        _ScaleTime = animationCollectionInfo.effectMaterials[i].GetFloat("_ScaleTime");
                    }
                }
            }
            
            //_mesh = _player.GetComponent<MeshFilter>().sharedMesh;
            if (!Application.isPlaying)
            {
                if (_mesh == null)
                {
                    _mesh = _player.CreateQuadMesh();
                    _mesh.name = _player.name + "_Mesh";
                    _player.GetComponent<MeshFilter>().sharedMesh = _mesh;
                }
            }
            else
            {
                _mesh = _player.GetComponent<MeshFilter>().sharedMesh;
            }
            //_material = _player.GetComponent<MeshRenderer>().sharedMaterial;
            if (!Application.isPlaying)
            {
                if (_material == null)
                {
                    /*Shader shader = Shader.Find("StarUnion/Efficient Animation Player");
                    if (shader != null)
                    {
                        _material = new Material(shader);
                        _material.name = _player.name + "_Material";
                    }
                    else
                    {
                        Debug.LogError("Can not find EfficientAnimationPlayer Shader");
                    }*/

                    if (animationCollectionInfo != null && animationCollectionInfo.defaultMaterial != null)
                    {
                        _material = animationCollectionInfo.defaultMaterial;
                    }
                    else
                    {
                        Debug.LogError("Can not find Default Material");
                    }
                    
                }
            }
            else
            {
                _material = _player.GetComponent<MeshRenderer>().sharedMaterial;
            }
            
            if (animationCollectionInfo != null && _playerAgent == null)
            {
                _playerAgent = new EfficientAnimationPlayerAgent(animationCollectionInfo, _mesh, _material,_player.gameObject.GetComponent<MeshRenderer>());
                _playerAgent.roleIdPlayNow = roleIdPlayNow;
                _playerAgent.animationIdPlayNow = animationIdPlayNow;
                _playerAgent.thisRoleName = thisRoleName;
                _playerAgent.thisAnimationName = thisAnimationName;
                
            }

            if (!_initRoleAndAnimationName && animationCollectionInfo != null)
            {
                roleAnimationNameDictionary = new Dictionary<string, List<string>>();
                for (int i = 0; i < animationCollectionInfo.roleAndAnimationNames.Count; i++)
                {
                    string roleAndAnimationName = animationCollectionInfo.roleAndAnimationNames[i];
                    string roleName = roleAndAnimationName.Substring(0, roleAndAnimationName.IndexOf("_"));
                    string animationName = roleAndAnimationName.Substring(roleAndAnimationName.IndexOf("_") + 1);
                    if (!roleAnimationNameDictionary.ContainsKey(roleName))
                    {
                        List<string> newRole = new List<string>();
                        roleAnimationNameDictionary.Add(roleName, newRole);
                    }

                    roleAnimationNameDictionary[roleName].Add(animationName);
                }

                roleNames = roleAnimationNameDictionary.Keys.ToArray();
                for (int i = 0; i < roleNames.Length; i++)
                {
                    if (thisRoleName.Equals(roleNames[i]))
                    {
                        _player.roleIdPlayNow = i;
                        _playerAgent.roleIdPlayNow = i;
                        roleIdPlayNow = i;
                    }
                }
                if (roleAnimationNameDictionary.Count > 0)
                {
                    animationNames = roleAnimationNameDictionary[roleNames[roleIdPlayNow]].ToArray();
                    for (int i = 0; i < animationNames.Length; i++)
                    {
                        if (thisAnimationName.Equals(animationNames[i]))
                        {
                            _player.animationIdPlayNow = i;
                            _playerAgent.animationIdPlayNow = i;
                            animationIdPlayNow = i;
                        }
                    }
                    _initRoleAndAnimationName = true;
                }
            }
            if (!Application.isPlaying && _playerAgent != null && _playerAgent.thisInit)
            {
                _playerAgent.SetRole(thisRoleName);
                _playerAgent.PlayAnimation(thisAnimationName);
            }
        }

        public bool IsTypeInclude(Enum type)
        {
            if ((EfficientAnimationShaderType)type == EfficientAnimationShaderType.Base)
            {
                return true;
            }
            if (animationCollectionInfo.effectShaderTypes.Contains((EfficientAnimationShaderType)type))
            {
                return true;
            }
            return false;
        }
        
        public bool IsTypeInState(Enum type)
        {
            EfficientAnimationShaderType effectType = (EfficientAnimationShaderType) type;
            if (effectType == EfficientAnimationShaderType.Base)
            {
                return true;
            }
            if ((int)effectType < 16 && (effectType & _redefineShaderType) != 0)
            {
                return true;
            }
            return false;
        }
        
        public override void OnInspectorGUI()
        {
            if (notInScene)
            {
                return;
            }
            
            //这个不能放在init里，会导致init一直执行，原因不明。
            if (!Application.isPlaying)
            {
                if (_player.GetComponent<MeshRenderer>().sharedMaterial == null)
                {
                    _player.GetComponent<MeshRenderer>().sharedMaterial = _material;
                }
            }

            if (_player != null && _player.GetComponent<MeshRenderer>() != null && _player.GetComponent<MeshRenderer>().sharedMaterial != null)
            {
                string shaderName = _player.GetComponent<MeshRenderer>().sharedMaterial.shader.name;
                shaderName = shaderName.Substring(shaderName.LastIndexOf(" ")+1);
                if (shaderName.Equals("MomentAll"))
                {
                    playShaderType = EfficientAnimationShaderType.Base;
                    if (_player.GetComponent<MeshRenderer>().sharedMaterial.IsKeywordEnabled("_DISSIPATION"))
                    {
                        playShaderType = playShaderType | EfficientAnimationShaderType.Dissipation;
                    }
                    if (_player.GetComponent<MeshRenderer>().sharedMaterial.IsKeywordEnabled("_FLASH"))
                    {
                        playShaderType = playShaderType | EfficientAnimationShaderType.Flash;
                    }
                    if (_player.GetComponent<MeshRenderer>().sharedMaterial.IsKeywordEnabled("_SCALE"))
                    {
                        playShaderType = playShaderType | EfficientAnimationShaderType.Scale;
                    }
                }
                else
                {
                    playShaderType = (EfficientAnimationShaderType)Enum.Parse(typeof(EfficientAnimationShaderType),shaderName);
                
                }
            }

            EditorGUI.BeginChangeCheck();
            loop = EditorGUILayout.Toggle("Animation Loop",loop);
            if (EditorGUI.EndChangeCheck())
            {
                wantPlay = true;
            }
        
            EditorGUI.BeginChangeCheck();
            roleIdPlayNow = EditorGUILayout.Popup(roleIdPlayNow, roleNames);
            if (EditorGUI.EndChangeCheck())
            {
                animationNames = roleAnimationNameDictionary[roleNames[roleIdPlayNow]].ToArray();
                if (animationIdPlayNow >= animationNames.Length)
                {
                    animationIdPlayNow = 0;
                }
                _player.roleIdPlayNow = roleIdPlayNow;
                _player.thisRoleName = roleNames[roleIdPlayNow];
                _playerAgent.roleIdPlayNow = roleIdPlayNow;
                _playerAgent.thisRoleName = roleNames[roleIdPlayNow];
                if (Application.isPlaying)
                {
                    _player.SetRole(roleNames[roleIdPlayNow]);
                }
                else
                {
                    _playerAgent.SetRole(roleNames[roleIdPlayNow]);
                }
                
                wantPlay = true;
            }
            
            EditorGUI.BeginChangeCheck();
            if (animationNames != null)
            {
                animationIdPlayNow = EditorGUILayout.Popup(animationIdPlayNow, animationNames);
            }
            if (EditorGUI.EndChangeCheck())
            {
                _player.animationIdPlayNow = animationIdPlayNow;
                _player.thisAnimationName = animationNames[animationIdPlayNow];
                _playerAgent.animationIdPlayNow = animationIdPlayNow;
                _playerAgent.thisAnimationName = animationNames[animationIdPlayNow];
                wantPlay = true;
            }
            
            EditorGUI.BeginChangeCheck();
            playShaderType = (EfficientAnimationShaderType)EditorGUILayout.EnumPopup(new GUIContent("Shader Type"),playShaderType,IsTypeInclude,false);
            if (EditorGUI.EndChangeCheck())
            {
                if (Application.isPlaying)
                {
                    _player.PlayEffect(playShaderType);
                }
                else
                {
                    _playerAgent.PlayEffect(playShaderType);
                }
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            _EffectTime = 0;
            if ((playShaderType & EfficientAnimationShaderType.Dissipation) != 0)
            {
                EditorGUI.BeginChangeCheck();
                _DissolveEdgeColor = EditorGUILayout.ColorField("Dissipation Edge Color",_DissolveEdgeColor);
                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < animationCollectionInfo.effectMaterials.Count; i++)
                    {
                        if (animationCollectionInfo.effectMaterials[i].IsKeywordEnabled("_DISSIPATION"))
                        {
                            animationCollectionInfo.effectMaterials[i].SetColor("_DissolveEdgeColor",_DissolveEdgeColor);
                        }
                    }
                }
                EditorGUI.BeginChangeCheck();
                _DissolveEdgeWidth = EditorGUILayout.FloatField("Dissipation Edge Width",_DissolveEdgeWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < animationCollectionInfo.effectMaterials.Count; i++)
                    {
                        if (animationCollectionInfo.effectMaterials[i].IsKeywordEnabled("_DISSIPATION"))
                        {
                            animationCollectionInfo.effectMaterials[i].SetFloat("_DissolveEdgeWidth",_DissolveEdgeWidth);
                        }
                    }
                }
                EditorGUI.BeginChangeCheck();
                _DissolveTime = EditorGUILayout.FloatField("Dissipation Time",_DissolveTime);
                if (EditorGUI.EndChangeCheck())
                {
                    if (_DissolveTime > _EffectTime)
                    {
                        _EffectTime = _DissolveTime;
                    }
                    for (int i = 0; i < animationCollectionInfo.effectMaterials.Count; i++)
                    {
                        if (animationCollectionInfo.effectMaterials[i].IsKeywordEnabled("_DISSIPATION"))
                        {
                            animationCollectionInfo.effectMaterials[i].SetFloat("_DissolveTime",_DissolveTime);
                            animationCollectionInfo.effectMaterials[i].SetFloat("_EffectTime",_EffectTime);
                        }
                    }
                }
                EditorGUI.BeginChangeCheck();
                _DissolveTiling = EditorGUILayout.Vector2Field("Dissipation Tiling", _DissolveTiling);
                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < animationCollectionInfo.effectMaterials.Count; i++)
                    {
                        if (animationCollectionInfo.effectMaterials[i].IsKeywordEnabled("_DISSIPATION"))
                        {
                            animationCollectionInfo.effectMaterials[i].SetTextureScale("_DissolveTex",_DissolveTiling);
                        }
                    }
                }
            }
            if ((playShaderType & EfficientAnimationShaderType.Flash) != 0)
            {
                EditorGUI.BeginChangeCheck();
                _FlashColor = EditorGUILayout.ColorField("Flash Color", _FlashColor);
                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < animationCollectionInfo.effectMaterials.Count; i++)
                    {
                        if (animationCollectionInfo.effectMaterials[i].IsKeywordEnabled("_FLASH"))
                        {
                            animationCollectionInfo.effectMaterials[i].SetColor("_FlashColor",_FlashColor);
                        }
                    }
                }
                EditorGUI.BeginChangeCheck();
                _FlashTime = EditorGUILayout.FloatField("Flash Time", _FlashTime);
                if (EditorGUI.EndChangeCheck())
                {
                    if (_FlashTime > _EffectTime)
                    {
                        _EffectTime = _FlashTime;
                    }
                    for (int i = 0; i < animationCollectionInfo.effectMaterials.Count; i++)
                    {
                        if (animationCollectionInfo.effectMaterials[i].IsKeywordEnabled("_FLASH"))
                        {
                            animationCollectionInfo.effectMaterials[i].SetFloat("_FlashTime",_FlashTime);
                            animationCollectionInfo.effectMaterials[i].SetFloat("_EffectTime",_EffectTime);
                        }
                    }
                }
            }
            if ((playShaderType & EfficientAnimationShaderType.Scale) != 0)
            {
                EditorGUI.BeginChangeCheck();
                _ScaleStrength = EditorGUILayout.FloatField("Scale Strength", _ScaleStrength);
                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < animationCollectionInfo.effectMaterials.Count; i++)
                    {
                        if (animationCollectionInfo.effectMaterials[i].IsKeywordEnabled("_SCALE"))
                        {
                            animationCollectionInfo.effectMaterials[i].SetFloat("_ScaleStrength",_ScaleStrength);
                        }
                    }
                }
                EditorGUI.BeginChangeCheck();
                _ScaleTime = EditorGUILayout.FloatField("Scale Time",_ScaleTime);
                if (EditorGUI.EndChangeCheck())
                {
                    if (_ScaleTime > _EffectTime)
                    {
                        _EffectTime = _ScaleTime;
                    }
                    for (int i = 0; i < animationCollectionInfo.effectMaterials.Count; i++)
                    {
                        if (animationCollectionInfo.effectMaterials[i].IsKeywordEnabled("_SCALE"))
                        {
                            animationCollectionInfo.effectMaterials[i].SetFloat("_ScaleTime",_ScaleTime);
                            animationCollectionInfo.effectMaterials[i].SetFloat("_EffectTime",_EffectTime);
                        }
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUI.BeginChangeCheck();
            loopOffset = EditorGUILayout.FloatField("Animation Offset",loopOffset);
            if (EditorGUI.EndChangeCheck())
            {
                if (Application.isPlaying)
                {
                    _player.SetLoopOffset(loopOffset);
                }
                else
                {
                    _playerAgent.SetLoopOffset(loopOffset);
                }
            }
            
            EditorGUI.BeginChangeCheck();
            speedScale = EditorGUILayout.FloatField("Animation Speed Scale",speedScale);
            if (EditorGUI.EndChangeCheck())
            {
                if (Application.isPlaying)
                {
                    _player.SetSpeedScale(speedScale);
                }
                else
                {
                    _playerAgent.SetSpeedScale(speedScale);
                }
            }
            
            EditorGUI.BeginChangeCheck();
            color = EditorGUILayout.ColorField("Color Tint",color);
            colorStrength = EditorGUILayout.FloatField("Color Strength", colorStrength);
            if (EditorGUI.EndChangeCheck())
            {
                if (Application.isPlaying)
                {
                    _player.SetColorTint(color,colorStrength);
                }
                else
                {
                    _playerAgent.SetColorTint(color,colorStrength);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            redefineEffects = EditorGUILayout.Foldout(redefineEffects, "ReBake Shader Types");
            if (redefineEffects)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Init Shader Type");
                _redefineInitShaderType = (EfficientAnimationShaderType)EditorGUILayout.EnumPopup(new GUIContent(""), _redefineInitShaderType,IsTypeInState,false);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Bake Shader Type");
                _redefineShaderType = (EfficientAnimationShaderType)EditorGUILayout.EnumFlagsField("",_redefineShaderType);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("ZWrite  ",GUILayout.Width(60));
                _zWrite = EditorGUILayout.Toggle("",_zWrite,GUILayout.Width(40));
                EditorGUILayout.LabelField("ZTest ",GUILayout.Width(60));
                _zTest = EditorGUILayout.Toggle("",_zTest,GUILayout.Width(40));
                EditorGUILayout.LabelField("Cut Off",GUILayout.Width(60));
                _cutOff = EditorGUILayout.Slider("", _cutOff, 0, 1,GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();
                
                if ((_redefineShaderType & EfficientAnimationShaderType.Dissipation) != 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Dissipation Texture",GUILayout.Width(120));
                    if (_dissipationTexture == null)
                    {
                        for (int i = 0; i < animationCollectionInfo.effectShaderTypes.Count; i++)
                        {
                            if ((animationCollectionInfo.effectShaderTypes[i] & EfficientAnimationShaderType.Dissipation) != 0)
                            {
                                Material dissipationMaterial = animationCollectionInfo.effectMaterials[i];
                                _dissipationTexture = (Texture2D)dissipationMaterial.GetTexture("_DissolveTex");
                                if(_dissipationTexture != null)
                                {
                                    break;
                                }
                            }
                        }
                        if (_dissipationTexture == null)
                        {
                            string scriptPath = EfficientAnimationAtlasTool.GetPath("EfficientAnimationAtlasTool");
                            if (_dissipationTexture == null)
                            {
                                scriptPath = scriptPath.Substring(0,scriptPath.IndexOf("Script"))+"Texture/";
                                _dissipationTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(scriptPath +"Dissovle.png");
                            }
                        }
                    }
                    _dissipationTexture = (Texture2D)EditorGUILayout.ObjectField(_dissipationTexture, typeof(Texture2D), false,GUILayout.Width(100));
                    EditorGUILayout.LabelField("Dissipation Tiling",GUILayout.Width(110));
                    _DissolveTiling = EditorGUILayout.Vector2Field("", _DissolveTiling,GUILayout.Width(110));
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.Space();
                if ((_redefineShaderType & EfficientAnimationShaderType.DissipationAndFlash) != 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    if ((_redefineShaderType & EfficientAnimationShaderType.Dissipation) != 0)
                    {
                        EditorGUILayout.LabelField("Dissipation Color",GUILayout.Width(110));
                        _DissolveEdgeColor = EditorGUILayout.ColorField(_DissolveEdgeColor);
                        EditorGUILayout.LabelField("Dissipation Width",GUILayout.Width(110));
                        _DissolveEdgeWidth = EditorGUILayout.FloatField(_DissolveEdgeWidth);
                        EditorGUILayout.LabelField("Dissipation Time",GUILayout.Width(110));
                        _DissolveTime = EditorGUILayout.FloatField(_DissolveTime);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    if ((_redefineShaderType & EfficientAnimationShaderType.Flash) != 0)
                    {
                        EditorGUILayout.LabelField("Flash Color",GUILayout.Width(70));
                        _FlashColor = EditorGUILayout.ColorField(_FlashColor);
                        EditorGUILayout.LabelField("Flash Time",GUILayout.Width(70));
                        _FlashTime = EditorGUILayout.FloatField(_FlashTime);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if ((_redefineShaderType & EfficientAnimationShaderType.Scale) != 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Scale Strength",GUILayout.Width(100));
                    _ScaleStrength = EditorGUILayout.FloatField(_ScaleStrength,GUILayout.Width(100));
                    EditorGUILayout.LabelField("     Scale Time",GUILayout.Width(100));
                    _ScaleTime = EditorGUILayout.FloatField(_ScaleTime,GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.Space();
                if (GUILayout.Button("ReBake"))
                {
                    if (animationCollectionInfo != null)
                    {
                        string collectionInfoPath = AssetDatabase.GetAssetPath(animationCollectionInfo.defaultMaterial);
                        string collectionFolder = collectionInfoPath.Substring(0, collectionInfoPath.LastIndexOf('/'));
                        string[] materialsPath = Directory.GetFiles(collectionFolder, "*_mat.asset");
                        foreach (var materialPath in materialsPath)
                        {
                            //调用AssetDatabase.DeleteAsset会执行awake;
                            //AssetDatabase.DeleteAsset(materialPath);
                            File.Delete(materialPath);
                            File.Delete(materialPath + ".meta");
                        }
                        
                        string defaultMaterialSavePath = collectionFolder + "\\" + animationCollectionInfo.collectionName + "_default_mat.asset";
                        Shader defaultShader = Shader.Find("StarUnion/Efficient Animation Player Base");
                        if (defaultShader != null)
                        {
                            Material defaultMaterial = new Material(defaultShader);
                            defaultMaterial.DisableKeyword("_PREVIEWMODE");
                            defaultMaterial.SetFloat("_ZWriteMode",_zWrite?1:0);
                            defaultMaterial.SetFloat("_ZTestMode",_zTest?4:0);
                            defaultMaterial.SetFloat("_CutOff",_cutOff);
                            AssetDatabase.CreateAsset(defaultMaterial,defaultMaterialSavePath);
                            AssetDatabase.Refresh();
                            animationCollectionInfo.defaultMaterial = defaultMaterial;
                        }
                        else
                        {
                            Debug.LogError("Cant Find Shader : StarUnion/Efficient Animation Player Base");
                        }
                        animationCollectionInfo.initShaderType = _redefineInitShaderType;

                        animationCollectionInfo.effectShaderTypes = new List<EfficientAnimationShaderType>();
                        animationCollectionInfo.effectMaterials = new List<Material>();
                        int shaderTypeNumber = (int)_redefineShaderType;
                        if ((int)_redefineShaderType == -1)
                        {
                            shaderTypeNumber  = 0;
                            Array allValues = Enum.GetValues(typeof(EfficientAnimationShaderType));
                            foreach (var value in allValues)
                            {
                                shaderTypeNumber = shaderTypeNumber + (int)value;
                            }
                        }
                        
                        for (int i = 1; i <= shaderTypeNumber; i++)
                        {
                            if (((int)_redefineShaderType & i) == 0)
                            {
                                continue;
                            }
                            if (Enum.IsDefined(typeof(EfficientAnimationShaderType),i))
                            {
                                string shaderTypeName = Enum.GetName(typeof(EfficientAnimationShaderType), i);
                                string addMaterialSavePath = collectionFolder + "\\" + animationCollectionInfo.collectionName + "_" + shaderTypeName + "_mat.asset";
                                Shader addShader;
                                if (i >= 16)
                                {
                                    addShader = Shader.Find("StarUnion/Efficient Animation Player MomentAll");
                                }
                                else
                                {
                                    addShader = Shader.Find("StarUnion/Efficient Animation Player " + shaderTypeName);
                                }
                                if (addShader != null)
                                {
                                    Material addMaterial = new Material(addShader);
                                    float _EffectTime = 0;
                                    if (shaderTypeName.Contains("Dissipation"))
                                    {
                                        addMaterial.SetColor("_DissolveEdgeColor",_DissolveEdgeColor);
                                        addMaterial.SetFloat("_DissolveEdgeWidth",_DissolveEdgeWidth);
                                        addMaterial.SetFloat("_DissolveTime",_DissolveTime);
                                        if (_DissolveTime > _EffectTime)
                                        {
                                            _EffectTime = _DissolveTime;
                                        }
                                        addMaterial.SetTextureScale("_DissolveTex",_DissolveTiling);
                                        if (_dissipationTexture != null)
                                        {
                                            addMaterial.SetTexture("_DissolveTex",_dissipationTexture);
                                        }
                                        addMaterial.EnableKeyword("_DISSIPATION");
                                    }
                                    else
                                    {
                                        addMaterial.DisableKeyword("_DISSIPATION");
                                    }
                                    if (shaderTypeName.Contains("Flash"))
                                    {
                                        addMaterial.SetColor("_FlashColor",_FlashColor);
                                        addMaterial.SetFloat("_FlashTime",_FlashTime);
                                        if (_FlashTime > _EffectTime)
                                        {
                                            _EffectTime = _FlashTime;
                                        }
                                        addMaterial.EnableKeyword("_FLASH");
                                    }
                                    else
                                    {
                                        addMaterial.DisableKeyword("_FLASH");
                                    }
                                    if (shaderTypeName.Contains("Scale"))
                                    {
                                        addMaterial.SetFloat("_ScaleStrength",_ScaleStrength);
                                        addMaterial.SetFloat("_ScaleTime",_ScaleTime);
                                        addMaterial.SetFloat("_EffectTime",_ScaleTime);
                                        if (_ScaleTime > _EffectTime)
                                        {
                                            _EffectTime = _ScaleTime;
                                        }
                                        addMaterial.EnableKeyword("_SCALE");
                                    }
                                    else
                                    {
                                        addMaterial.DisableKeyword("_SCALE");
                                    }
                                    if (i >= 16)
                                    {
                                        addMaterial.SetFloat("_EffectTime",_EffectTime);
                                    }
                                    addMaterial.EnableKeyword("_PREVIEWMODE");
                                    addMaterial.SetFloat("_ZWriteMode",_zWrite?1:0);
                                    addMaterial.SetFloat("_ZTestMode",_zTest?4:0);
                                    addMaterial.SetFloat("_CutOff",_cutOff);
                                    addMaterial.renderQueue = 3000 + EfficientAnimationPlayer.effectMaterialTypeStep * (animationCollectionInfo.effectMaterials.Count + 1);
                                    AssetDatabase.CreateAsset(addMaterial,addMaterialSavePath);
                                    AssetDatabase.Refresh();
                                    animationCollectionInfo.effectShaderTypes.Add((EfficientAnimationShaderType)i);
                                    animationCollectionInfo.effectMaterials.Add(addMaterial);
                                }
                                else
                                {
                                    if (i >= 16)
                                    {
                                        Debug.LogError("Cant Find Shader : " + "StarUnion/Efficient Animation Player MomentAll");
                                    }
                                    else
                                    {
                                        Debug.LogError("Cant Find Shader : " + "StarUnion/Efficient Animation Player " + shaderTypeName);
                                    }
                                }
                            }
                        }
                        EditorUtility.SetDirty(animationCollectionInfo);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        Awake();
                        if (!Application.isPlaying)
                        {
                            playShaderType = EfficientAnimationShaderType.Base;
                            _playerAgent.PlayEffect(playShaderType);
                        }
                        return;
                    }
                }
            }
        
            if (wantPlay)
            {
                if (Application.isPlaying)
                {
                    
                    _player.PlayAnimation( animationNames[animationIdPlayNow], loop);
                }
                else
                {
                    _playerAgent.PlayAnimation( animationNames[animationIdPlayNow], loop);
                }
                wantPlay = false;
            }
        }
    }
 }


