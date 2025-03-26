using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StarUnion_EfficientAnimation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace StarUnion_EfficientAnimation_Tool
{
    [Serializable]
    public class EfficientAnimaitonInfo
    {
        public string roleName;
        public string animationName;
        private string _path;
        private Vector4 _offsetAndScale;
        public int fps;
        public List<Texture2D> animationTexture2Ds;
        public EfficientAnimaitonInfo(string setRoleName,string setAnimationName, int setFps)
        {
            roleName = setRoleName;
            animationName = setAnimationName;
            fps = setFps;
            animationTexture2Ds = new List<Texture2D>();
        }

        public void SetOffsetAndScale(Vector4 offsetAndScale)
        {
            _offsetAndScale = offsetAndScale;
        }
        public Vector4 GetOffsetAndScale()
        {
            return _offsetAndScale;
        }
        public void SetPath(string path)
        {
            _path = path;
        }
        public string GetPath()
        {
            return _path;
        }
    }

    [Serializable]
    public class EfficientRoleInfo
    {
        public string roleName;
        private string _path;
        public List<EfficientAnimaitonInfo> animationInfosList;
        public EfficientRoleInfo()
        {
            animationInfosList = new List<EfficientAnimaitonInfo>();
        }
        
        public void SetPath(string path)
        {
            _path = path;
        }
        public string GetPath()
        {
            return _path;
        }
    }

    [Serializable]
    public class EfficientCollectionInfo
    {
        public string collectionName;
        private string _path;
        public int firstId;
        public List<EfficientRoleInfo> roleInfosList;
        public EfficientCollectionInfo()
        {
            roleInfosList = new List<EfficientRoleInfo>();
        }

        public void SetPath(string path)
        {
            _path = path;
        }
        public string GetPath()
        {
            return _path;
        }
    }

    public class EfficientAnimationAtlasTool : EditorWindow
    {
        public static void CopyFolder(string sourcePath, string targetPath)
        {
            if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath,true);
            }
            Directory.CreateDirectory(targetPath);
            foreach (string dir in Directory.GetDirectories(sourcePath))
            {
                string newDir = dir.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(newDir);
                CopyFolder(dir,newDir);
            }
            foreach (string file in Directory.GetFiles(sourcePath))
            {
                string newFile = file.Replace(sourcePath, targetPath);
                File.Copy(file,newFile);
            }
        }
        
        [MenuItem("Assets/Efficient Animation Atlas Tool(open as all animation folder)")]
        static void InitAsAllFolder()
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            string pathGUID = Selection.assetGUIDs[0];
            _animationsTopPath = AssetDatabase.GUIDToAssetPath(pathGUID);
            _folderDepth = GetFolderDepth(_animationsTopPath);
            if (_folderDepth != 4)
            {
                EditorUtility.DisplayDialog("Error","the folder must have depth 4,now depth is " + _folderDepth,"ok");
                return;
            }
            string targetFolder = _animationsTopPath+"Bak~";
            CopyFolder(_animationsTopPath, targetFolder);
            AssetDatabase.Refresh();
            EfficientAnimationAtlasTool window = (EfficientAnimationAtlasTool)EditorWindow.GetWindow(typeof(EfficientAnimationAtlasTool));
            window.minSize = window.maxSize = new Vector2(800, 900);
            window.Show();
        }
        
        [MenuItem("Assets/Efficient Animation Atlas Tool(open as collection folder)")]
        static void InitAsCollection()
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            _animationPaths.Clear();
            for (int i = 0; i < Selection.assetGUIDs.Length; i++)
            {
                string collectionPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]);
                _folderDepth = GetFolderDepth(collectionPath);
                if (_folderDepth == 3)
                {
                    _animationPaths.Add(collectionPath);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error",collectionPath + " : folder must have depth 3,now depth is " + _folderDepth,"ok");
                }
            }
            if (_animationPaths.Count == 0)
            {
                return;
            }
            
            for (int i = 0; i < _animationPaths.Count; i++)
            {
                string targetFolder = _animationPaths[i]+"Bak~";
                CopyFolder(_animationPaths[i], targetFolder);
                AssetDatabase.Refresh();
            }
            EfficientAnimationAtlasTool window = (EfficientAnimationAtlasTool)EditorWindow.GetWindow(typeof(EfficientAnimationAtlasTool));
            window.minSize = window.maxSize = new Vector2(800, 820);
            window.Show();
            /*SetPackageIdWindow window = (SetPackageIdWindow)EditorWindow.GetWindow(typeof(SetPackageIdWindow));
            window.minSize = window.maxSize = new Vector2(400, 200);
            window.Show();*/
        }

        [SerializeField]
        private List<EfficientCollectionInfo> collectionInfoList = new List<EfficientCollectionInfo>();
        private static string _animationsTopPath;
        private static List<string> _animationPaths = new List<string>();
        private static Dictionary<string, int> _atlasIdDictionary;
        private static int _folderDepth;
        private SerializedObject _serObj;
        private SerializedProperty _serPty;
        private bool _canCloseNow = false;
        public static int maxTextureSize = 2048;
        private string _tipString = "提示信息";
        
        private string _outputPath = "Assets\\BundleResources\\EfficientAnimationAtlas";
        private int _defaultFps = 8;
        private bool _powOfTwoTextureSize = false;
        private Vector3 _prefabRotation = new Vector3(0, 0, 0);
        private bool _autoCropTexturesGroupByAnimation = true;
        private bool _mergeSimilarHeightTexturesToAtlas = true;
        private int _heightDifference = 40;
        private bool _removeEmptyLine = true;
        private const int _minCellSize = 4;
        private static int _packageIdStart = 0;
        
        //下面是材质参数
        private bool _zWrite = true;
        private bool _zTest = true;
        private float _cutOff = 0.1f;
        private static Texture2D _dissipationTexture;
        private EfficientAnimationShaderType _shaderType = (EfficientAnimationShaderType)(-1);
        private EfficientAnimationShaderType _shaderTypeInit = EfficientAnimationShaderType.Base;
        private Color _dissolveEdgeColor = Color.red;
        private float _dissolveEdgeWidth = 0.2f;
        private float _dissolveTime = 1.0f;
        private Vector2 _DissolveTiling = new Vector2(0.5f, 0.5f);
        private Color _flashColor = new Color(1.0f,0.6f,0.07f,1.0f);
        private float _flashTime = 0.2f;
        private float _ScaleStrength = 0.9f;
        private float _ScaleTime = 0.1f;

        public static int GetFolderDepth(string pathTop)
        {
            if (!Directory.Exists(pathTop))
            {
                return 0;
            }
            int maxDepth = 0;
            string[] directorys = Directory.GetDirectories(pathTop);
            if (directorys.Length > 0)
            {
                for (int i = 0; i < directorys.Length; i++)
                {
                    int childDepth = GetFolderDepth(directorys[i]);
                    if (childDepth > maxDepth)
                    {
                        maxDepth = childDepth;
                    }
                }
            }
            return maxDepth + 1;
        }
        
        public static string GetPath(string _scriptName)
        {
            string[] path = UnityEditor.AssetDatabase.FindAssets(_scriptName);
            if(path.Length>1)
            {
                Debug.LogError("有同名文件"+_scriptName+"获取路径失败");
                return null;
            }
            string _path = AssetDatabase.GUIDToAssetPath(path[0]).Replace((@"/"+_scriptName+".cs"),"");
            return _path;
        }
        
        private void OnEnable()
        {
            CreateAllAnimtionInfo();
            _serObj = new SerializedObject(this);
            _serPty = _serObj.FindProperty("collectionInfoList");
            string scriptPath = GetPath("EfficientAnimationAtlasTool");
            if (_dissipationTexture == null)
            {
                //scriptPath = scriptPath.Substring(0,scriptPath.IndexOf("Script"))+"Texture/";
                scriptPath = "Assets/SURender/StarUnion_EfficientAnimation/Texture/";
                _dissipationTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(scriptPath +"Dissovle.png");
            }
        }

        private void CreateAllAnimtionInfo()
        {
            if (_folderDepth == 4)
            {
                string[] childrenPaths = Directory.GetDirectories(_animationsTopPath);
                int collectionindex = EfficientAnimationPlayer.collectionMaterialMaxNumber;
                foreach (var childPath in childrenPaths)
                {
                    if (GetFolderDepth(childPath) == 3)
                    {
                        EfficientCollectionInfo collectionInfo = CreateCollectionInfo(childPath,collectionindex);
                        collectionInfoList.Add(collectionInfo);
                    }
                    collectionindex = collectionindex + EfficientAnimationPlayer.collectionMaterialMaxNumber;
                }
            }
            else
            {
                _atlasIdDictionary = GetAllAtlasIdList(_outputPath);
                if (_atlasIdDictionary != null)
                {
                    _animationPaths.Sort();
                    foreach (var animationPath in _animationPaths)
                    {
                        EfficientCollectionInfo collectionInfo = CreateCollectionInfo(animationPath);
                        collectionInfoList.Add(collectionInfo);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "the output path is not exist!", "ok");
                }
            }
        }

        private Dictionary<string, int> GetAllAtlasIdList(string path)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }
            Dictionary<string, int> list = new Dictionary<string, int>();
            //string[] allFilePaths = Directory.GetFiles(path, "*_animation.asset",SearchOption.AllDirectories);
            string[] allFilePaths = Directory.GetFiles(path, "*_ass.asset",SearchOption.AllDirectories);
            foreach (var filePath in allFilePaths)
            {
                AnimationCollectionInfo animationCollectionInfo = AssetDatabase.LoadAssetAtPath<AnimationCollectionInfo>(filePath);
                list.Add(animationCollectionInfo.collectionName,animationCollectionInfo.firstId);
            }
            return list;
        }
        
        private EfficientCollectionInfo CreateCollectionInfo(string path)
        {
            EfficientCollectionInfo collectionInfo = new EfficientCollectionInfo();
            collectionInfo.collectionName = Path.GetFileName(path);
            collectionInfo.SetPath(path);
            List<int> atlasIds = _atlasIdDictionary.Values.ToList();
            if (_atlasIdDictionary.ContainsKey(collectionInfo.collectionName))
            {
                collectionInfo.firstId = _atlasIdDictionary[collectionInfo.collectionName];
            }
            else
            {
                int thisCollectionId = _packageIdStart * EfficientAnimationPlayer.collectionMaterialMaxNumber;
                for (int i = 1; i < -EfficientAnimationPlayer.sortingOrderStart; i++)
                {
                    int atlasId = (_packageIdStart + i) * EfficientAnimationPlayer.collectionMaterialMaxNumber; 
                    if (!atlasIds.Contains(atlasId))
                    {
                        thisCollectionId = atlasId;
                        break;
                    }
                }
                _atlasIdDictionary.Add(collectionInfo.collectionName,thisCollectionId);
                collectionInfo.firstId = thisCollectionId;
            }
            string[] childrenPaths = Directory.GetDirectories(path);
            for (int i = 0; i < childrenPaths.Length; i++)
            {
                if (GetFolderDepth(childrenPaths[i]) == 2)
                {
                    EfficientRoleInfo roleInfo = CreateRoleInfo(childrenPaths[i]);
                    collectionInfo.roleInfosList.Add(roleInfo);
                }
            }
            return collectionInfo;
        }

        private EfficientCollectionInfo CreateCollectionInfo(string path,int index)
        {
            EfficientCollectionInfo collectionInfo = new EfficientCollectionInfo();
            collectionInfo.collectionName = Path.GetFileName(path);
            collectionInfo.SetPath(path);
            collectionInfo.firstId = index;
            string[] childrenPaths = Directory.GetDirectories(path);
            for (int i = 0; i < childrenPaths.Length; i++)
            {
                if (GetFolderDepth(childrenPaths[i]) == 2)
                {
                    EfficientRoleInfo roleInfo = CreateRoleInfo(childrenPaths[i]);
                    collectionInfo.roleInfosList.Add(roleInfo);
                }
            }
            return collectionInfo;
        }

        private EfficientRoleInfo CreateRoleInfo(string path)
        {
            EfficientRoleInfo roleInfo = new EfficientRoleInfo();
            roleInfo.roleName = Path.GetFileName(path);
            roleInfo.SetPath(path);
            string[] childrenPaths = Directory.GetDirectories(path);
            for (int i = 0; i < childrenPaths.Length; i++)
            {
                if (GetFolderDepth(childrenPaths[i]) == 1)
                {
                    EfficientAnimaitonInfo animaitonInfo = CreateAnimationInfo(roleInfo.roleName,childrenPaths[i]);
                    if (animaitonInfo != null)
                    {
                        roleInfo.animationInfosList.Add(animaitonInfo);
                    }
                }
            }
            return roleInfo;
        }

        private EfficientAnimaitonInfo CreateAnimationInfo(string roleName,string path)
        {
            EfficientAnimaitonInfo animaitonInfo = new EfficientAnimaitonInfo(roleName,Path.GetFileName(path),_defaultFps);
            animaitonInfo.SetPath(path);
            string[] childrenPathsWithMeta = Directory.GetFiles(path);
            List<string> childrenPaths = new List<string>();
            for (int i = 0; i < childrenPathsWithMeta.Length; i++)
            {
                if (!childrenPathsWithMeta[i].EndsWith(".meta"))
                {
                    childrenPaths.Add(childrenPathsWithMeta[i]);
                }
            }
            if (childrenPaths.Count == 0)
            {
                return null;
            }

            bool textureNeedSetting = true;
            for (int i = 0; i < childrenPaths.Count; i++)
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(childrenPaths[i]) as TextureImporter;
                if (textureImporter == null)
                {
                    continue;
                }
                if (i == 0 && textureImporter.alphaIsTransparency && textureImporter.isReadable && !textureImporter.mipmapEnabled && textureImporter.npotScale == TextureImporterNPOTScale.None)
                {
                    textureNeedSetting = false;
                }
                if (textureNeedSetting)
                {
                    textureImporter.alphaIsTransparency = true;
                    textureImporter.isReadable = true;
                    textureImporter.mipmapEnabled = false;
                    textureImporter.npotScale = TextureImporterNPOTScale.None;
                    AssetDatabase.ImportAsset(childrenPaths[i]);
                }
                Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(childrenPaths[i]);
                animaitonInfo.animationTexture2Ds.Add(texture2D);
            }
            return animaitonInfo;
        }

        private void SetAllAnimationDefaultFps()
        {
            foreach (var collectionInfo in collectionInfoList)
            {
                foreach (var roleInfo in collectionInfo.roleInfosList)
                {
                    foreach (var animaitonInfo in roleInfo.animationInfosList)
                    {
                        animaitonInfo.fps = _defaultFps;
                    }
                }
            }
        }

        private void ResetPackageFirstId()
        {
            foreach (var collectionInfo in collectionInfoList)
            {
                List<int> atlasIds = _atlasIdDictionary.Values.ToList();
                int thisCollectionId = _packageIdStart * EfficientAnimationPlayer.collectionMaterialMaxNumber;
                for (int j = 1; j < -EfficientAnimationPlayer.sortingOrderStart; j++)
                {
                    int atlasId = (_packageIdStart + j) * EfficientAnimationPlayer.collectionMaterialMaxNumber; 
                    if (!atlasIds.Contains(atlasId))
                    {
                        thisCollectionId = atlasId;
                        break;
                    }
                }
                if (_atlasIdDictionary.ContainsKey(collectionInfo.collectionName))
                {
                    _atlasIdDictionary[collectionInfo.collectionName] = thisCollectionId;
                }
                else
                {
                    _atlasIdDictionary.Add(collectionInfo.collectionName,thisCollectionId);
                }
                
                collectionInfo.firstId = thisCollectionId;
            }
            _serObj.ApplyModifiedProperties();
        }

        private void OnDestroy()
        {
            if (_folderDepth == 4)
            {
                string bakPath = _animationsTopPath + "Bak~";
                if (Directory.Exists(bakPath))
                {
                    Directory.Delete(_animationsTopPath,true);
                    CopyFolder(bakPath,_animationsTopPath);
                    Directory.Delete(bakPath,true);
                    AssetDatabase.DeleteAsset(bakPath+".meta");
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                for (int i = 0; i < _animationPaths.Count; i++)
                {
                    string bakPath = _animationPaths[i] + "Bak~";
                    if (Directory.Exists(bakPath))
                    {
                        Directory.Delete(_animationPaths[i],true);
                        CopyFolder(bakPath,_animationPaths[i]);
                        Directory.Delete(bakPath,true);
                        AssetDatabase.DeleteAsset(bakPath+".meta");
                        AssetDatabase.Refresh();
                    }
                }
            }
            
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            EditorSettings.enterPlayModeOptionsEnabled = false;
        }
        
        
        public bool IsTypeInState(Enum type)
        {
            EfficientAnimationShaderType effectType = (EfficientAnimationShaderType) type;
            if (effectType == EfficientAnimationShaderType.Base)
            {
                return true;
            }
            if ((int)effectType < 16 && (effectType & _shaderType) != 0)
            {
                return true;
            }
            return false;
        }

        Vector2 scrollPos;
        
        void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (collectionInfoList != null && collectionInfoList.Count > 0)
            {
                _tipString = "please push <do it> button!";
            }
            EditorGUILayout.LabelField(_tipString);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            _outputPath = EditorGUILayout.TextField("output path", _outputPath);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            _packageIdStart = EditorGUILayout.IntField("Package Id Start : ", _packageIdStart);
            if (EditorGUI.EndChangeCheck())
            {
                ResetPackageFirstId();
            }
            EditorGUILayout.LabelField("      Bake Shader Type",GUILayout.Width(140));
            _shaderType = (EfficientAnimationShaderType)EditorGUILayout.EnumFlagsField("", _shaderType,GUILayout.Width(120));
            EditorGUILayout.LabelField("      Init Shader Type",GUILayout.Width(120));
            _shaderTypeInit = (EfficientAnimationShaderType) EditorGUILayout.EnumPopup(new GUIContent(""), _shaderTypeInit,IsTypeInState,false,GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ZWrite  ",GUILayout.Width(60));
            _zWrite = EditorGUILayout.Toggle("",_zWrite,GUILayout.Width(40));
            EditorGUILayout.LabelField("ZTest ",GUILayout.Width(60));
            _zTest = EditorGUILayout.Toggle("",_zTest,GUILayout.Width(40));
            EditorGUILayout.LabelField("Cut Off",GUILayout.Width(60));
            _cutOff = EditorGUILayout.Slider("", _cutOff, 0, 1,GUILayout.Width(200));
            if ((_shaderType & EfficientAnimationShaderType.Dissipation) != 0)
            {
                EditorGUILayout.LabelField("     Dissipation Texture",GUILayout.Width(160));
                _dissipationTexture = (Texture2D)EditorGUILayout.ObjectField(_dissipationTexture, typeof(Texture2D), false);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            if ((_shaderType & EfficientAnimationShaderType.Dissipation) != 0)
            {
                EditorGUILayout.LabelField("Dissipation Color",GUILayout.Width(120));
                _dissolveEdgeColor = EditorGUILayout.ColorField(_dissolveEdgeColor,GUILayout.Width(120));
                EditorGUILayout.LabelField("Width",GUILayout.Width(40));
                _dissolveEdgeWidth = EditorGUILayout.FloatField(_dissolveEdgeWidth,GUILayout.Width(100));
                EditorGUILayout.LabelField("Time",GUILayout.Width(40));
                _dissolveTime = EditorGUILayout.FloatField(_dissolveTime,GUILayout.Width(100));
                EditorGUILayout.LabelField("Tiling",GUILayout.Width(40));
                _DissolveTiling = EditorGUILayout.Vector2Field("", _DissolveTiling);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            
            if ((_shaderType & EfficientAnimationShaderType.FlashAndScale) != 0)
            {
                EditorGUILayout.BeginHorizontal();
                if ((_shaderType & EfficientAnimationShaderType.Flash) != 0)
                {
                    EditorGUILayout.LabelField("Flash Color",GUILayout.Width(70));
                    _flashColor = EditorGUILayout.ColorField(_flashColor);
                    EditorGUILayout.LabelField("Flash Time",GUILayout.Width(70));
                    _flashTime = EditorGUILayout.FloatField(_flashTime);
                }
                if ((_shaderType & EfficientAnimationShaderType.Scale) != 0)
                {
                    EditorGUILayout.LabelField("Scale Strength",GUILayout.Width(100));
                    _ScaleStrength = EditorGUILayout.FloatField(_ScaleStrength,GUILayout.Width(100));
                    EditorGUILayout.LabelField("     Scale Time",GUILayout.Width(100));
                    _ScaleTime = EditorGUILayout.FloatField(_ScaleTime,GUILayout.Width(100));
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.BeginHorizontal();
            _defaultFps = EditorGUILayout.IntField("defult fps", _defaultFps);
            if (GUILayout.Button("set all animation default fps"))
            {
                SetAllAnimationDefaultFps();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            _powOfTwoTextureSize = EditorGUILayout.Toggle("Pow Of Two Texture Size",_powOfTwoTextureSize);
            _prefabRotation = EditorGUILayout.Vector3Field("Prefab Rotation",_prefabRotation);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (!_powOfTwoTextureSize)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Auto Crop Textures Group By Animation",GUILayout.Width(240));
                _autoCropTexturesGroupByAnimation = EditorGUILayout.Toggle(_autoCropTexturesGroupByAnimation);
                if (_autoCropTexturesGroupByAnimation)
                {
                    EditorGUILayout.LabelField("Merge Similar Height Textures To Atlas",GUILayout.Width(240));
                    _mergeSimilarHeightTexturesToAtlas = EditorGUILayout.Toggle(_mergeSimilarHeightTexturesToAtlas);
                }
                else
                {
                    _mergeSimilarHeightTexturesToAtlas = false;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                _autoCropTexturesGroupByAnimation = false;
            }
            if (_autoCropTexturesGroupByAnimation && _mergeSimilarHeightTexturesToAtlas)
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height Difference",GUILayout.Width(160));
                _heightDifference = EditorGUILayout.IntField(_heightDifference,GUILayout.Width(80));
                EditorGUILayout.LabelField("      ",GUILayout.Width(150));
                EditorGUILayout.LabelField("Atlas With The Minimum Empty",GUILayout.Width(240));
                _removeEmptyLine = EditorGUILayout.Toggle(_removeEmptyLine);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button("do it！"))
            {
                if (!Directory.Exists(_outputPath))
                {
                    Directory.CreateDirectory(_outputPath);
                }
                if (collectionInfoList != null && collectionInfoList.Count > 0)
                {
                    foreach (var collectionInfo in collectionInfoList)
                    {
                        CreateCollectionAssets(collectionInfo);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Error","the efficient Animaiton List is null","ok");
                }
            }
            EditorGUILayout.LabelField("---------------------------------------------------------------------------------------------------------------------------------------");
            EditorGUILayout.Space();
            if (_serObj != null && !_canCloseNow)
            {
                _serObj.Update();
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(800), GUILayout.Height(400));
            EditorGUI.BeginChangeCheck();
            if (_serPty != null && !_canCloseNow)
            {
                EditorGUILayout.PropertyField(_serPty, true);
            }
            if (EditorGUI.EndChangeCheck())
            {
                _serObj.ApplyModifiedProperties();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.LabelField("---------------------------------------------------------------------------------------------------------------------------------------");
            if (_canCloseNow)
            {
                Close();
            }
        }

        private Dictionary<Vector2Int, List<EfficientAnimaitonInfo>> animationInfoInAtlas;
        private int textureId;
        private AnimationCollectionInfo _animationCollectionInfo;
        private int textureIndex;
        
        public static Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[4];
            Vector2[] uvs = new Vector2[4];
            Vector4[] uvsDefault = new Vector4[4];
            int[] triangles = new int[6]{0,2,1,0,3,2};
            Color[] colors = new Color[4];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
                uvsDefault[i] = Vector4.zero;
            }
            vertices[0] = new Vector3(-0.5f, -0.5f);
            uvs[0] = new Vector3(0.0f, 0.0f);
            vertices[1] = new Vector3(0.5f, -0.5f);
            uvs[1] = new Vector3(1.0f, 0.0f);
            vertices[2] = new Vector3(0.5f, 0.5f);
            uvs[2] = new Vector3(1.0f, 1.0f);
            vertices[3] = new Vector3(-0.5f, 0.5f);
            uvs[3] = new Vector3(0.0f, 1.0f);
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.SetUVs(1,uvsDefault);
            mesh.SetUVs(2,uvsDefault);
            mesh.SetUVs(3,uvsDefault);
            mesh.triangles = triangles;
            mesh.colors = colors;
            return mesh;
        }
        
        private void CreateCollectionAssets(EfficientCollectionInfo collectionInfo)
        {
            _canCloseNow = false;
            if (_autoCropTexturesGroupByAnimation)
            {
                foreach (var roleInfo in collectionInfo.roleInfosList)
                {
                    string rolePath = roleInfo.GetPath();
                    Vector2Int size = ResizeTexturesTool.CalculationSize(rolePath, _minCellSize);
                    int biggerSize = size.x > size.y ? size.x : size.y;
                    ResizeTexturesTool.ResizeAllTexture(rolePath,biggerSize,biggerSize);
                    Vector2 spriteSize = new Vector2(biggerSize, biggerSize);
                    foreach (var animaitonInfo in roleInfo.animationInfosList)
                    {
                        string animationPath = animaitonInfo.GetPath();
                        Vector4 crop = new Vector4();
                        CropTexturesTool.CalculationCrop(animationPath, _minCellSize,out spriteSize,out crop);
                        CropTexturesTool.CropAllTexture(animationPath, spriteSize,crop);
                        Vector4 offsetAndScale = CropTexturesTool.GetOffsetAndScale(spriteSize, crop);
                        animaitonInfo.SetOffsetAndScale(offsetAndScale);
                    }
                }
            }
            else
            {
                foreach (var roleInfo in collectionInfo.roleInfosList)
                {
                    string rolePath = roleInfo.GetPath();
                    Vector2Int size = ResizeTexturesTool.CalculationSize(rolePath, _minCellSize);
                    int biggerSize = size.x > size.y ? size.x : size.y;
                    ResizeTexturesTool.ResizeAllTexture(rolePath,biggerSize,biggerSize);
                }
                
                foreach (var roleInfo in collectionInfo.roleInfosList)
                {
                    string rolePath = roleInfo.GetPath();
                    Vector2Int size = ResizeTexturesTool.CalculationSize(rolePath, _minCellSize);
                    int biggerSize = size.x > size.y ? size.x : size.y;
                    foreach (var animaitonInfo in roleInfo.animationInfosList)
                    {
                        float spriteWidth = biggerSize;
                        float spriteHeight = biggerSize;
                        if (animaitonInfo.animationTexture2Ds.Count > 0)
                        {
                            spriteWidth = animaitonInfo.animationTexture2Ds[0].width;
                            spriteHeight = animaitonInfo.animationTexture2Ds[0].height;
                        }
                        Vector4 offsetAndScale = new Vector4(0, 0, spriteWidth / biggerSize, spriteHeight / biggerSize);
                        animaitonInfo.SetOffsetAndScale(offsetAndScale);
                    }
                }
            }
            textureIndex = 0;
            string collectionFolder = _outputPath + "\\" + collectionInfo.collectionName;
            if (Directory.Exists(collectionFolder))
            {
                Directory.Delete(collectionFolder,true);
            }
            Directory.CreateDirectory(collectionFolder);
            animationInfoInAtlas = new Dictionary<Vector2Int, List<EfficientAnimaitonInfo>>();
            textureId = 0;
            _animationCollectionInfo = (AnimationCollectionInfo)CreateInstance(typeof(AnimationCollectionInfo));
            _animationCollectionInfo.firstId = collectionInfo.firstId;
            _animationCollectionInfo.collectionName = collectionInfo.collectionName;
            _animationCollectionInfo.roleAndAnimationNames = new List<string>();
            _animationCollectionInfo.animationLocations = new List<Vector2Int>();
            _animationCollectionInfo.atlasInfos = new List<AtlasInfo>();
            
            string defaultMaterialSavePath = collectionFolder + "\\" + collectionInfo.collectionName + "_default_mat.asset";
            Shader defaultShader = Shader.Find("StarUnion/Efficient Animation Player Base");
            if (defaultShader != null)
            {
                Material defaultMaterial = new Material(defaultShader);
                defaultMaterial.DisableKeyword("_PREVIEWMODE");
                defaultMaterial.SetFloat("_ZWriteMode",_zWrite?1:0);
                defaultMaterial.SetFloat("_ZTestMode",_zTest?4:0);
                defaultMaterial.SetFloat("_CutOff",_cutOff);
                AssetDatabase.CreateAsset(defaultMaterial, defaultMaterialSavePath);
                _animationCollectionInfo.defaultMaterial = defaultMaterial;
            }
            else
            {
                Debug.LogError("Cant Find Shader : StarUnion/Efficient Animation Player Base");
            }
            _animationCollectionInfo.initShaderType = _shaderTypeInit;

            _animationCollectionInfo.effectShaderTypes = new List<EfficientAnimationShaderType>();
            _animationCollectionInfo.effectMaterials = new List<Material>();
            int shaderTypeNumber = (int)_shaderType;
            if ((int)_shaderType == -1)
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
                if (((int)_shaderType & i) == 0)
                {
                    continue;
                }
                if (Enum.IsDefined(typeof(EfficientAnimationShaderType),i))
                {
                    string shaderTypeName = Enum.GetName(typeof(EfficientAnimationShaderType), i);
                    string addMaterialSavePath = collectionFolder + "\\" + collectionInfo.collectionName + "_" + shaderTypeName + "_mat.asset";
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
                            addMaterial.SetColor("_DissolveEdgeColor",_dissolveEdgeColor);
                            addMaterial.SetFloat("_DissolveEdgeWidth",_dissolveEdgeWidth);
                            addMaterial.SetFloat("_DissolveTime",_dissolveTime);
                            addMaterial.SetTextureScale("_DissolveTex",_DissolveTiling);
                            if (_dissolveTime > _EffectTime)
                            {
                                _EffectTime = _dissolveTime;
                            }
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
                            addMaterial.SetColor("_FlashColor",_flashColor);
                            addMaterial.SetFloat("_FlashTime",_flashTime);
                            if (_flashTime > _EffectTime)
                            {
                                _EffectTime = _flashTime;
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
                        addMaterial.renderQueue = 3000 + EfficientAnimationPlayer.effectMaterialTypeStep * (_animationCollectionInfo.effectMaterials.Count + 1);
                        AssetDatabase.CreateAsset(addMaterial,addMaterialSavePath);
                        _animationCollectionInfo.effectShaderTypes.Add((EfficientAnimationShaderType)i);
                        _animationCollectionInfo.effectMaterials.Add(addMaterial);
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
            
            foreach (var roleInfo in collectionInfo.roleInfosList)
            {
                foreach (var animaitonInfo in roleInfo.animationInfosList)
                {
                    if (animaitonInfo.animationTexture2Ds.Count == 0)
                    {
                        continue;
                    }
                    Vector2Int size = new Vector2Int(animaitonInfo.animationTexture2Ds[0].width,
                        animaitonInfo.animationTexture2Ds[0].height);
                    if (!animationInfoInAtlas.ContainsKey(size))
                    {
                        List<EfficientAnimaitonInfo> animaitonInfoList = new List<EfficientAnimaitonInfo>();
                        animationInfoInAtlas.Add(size,animaitonInfoList);
                    }
                    animationInfoInAtlas[size].Add(animaitonInfo);
                }
            }
            string texturesPath = collectionFolder + "\\Textures";
            Directory.CreateDirectory(texturesPath);
            foreach (var animationInfoList in animationInfoInAtlas)
            {
                CreateTexturesForSize(texturesPath,animationInfoList);
            }

            //string collectionInfoSavePath = collectionFolder + "\\" + collectionInfo.collectionName + "_animation.asset";
            string collectionInfoSavePath = collectionFolder + "\\" + collectionInfo.collectionName + "_0_ass.asset";
            AssetDatabase.CreateAsset(_animationCollectionInfo,collectionInfoSavePath);

            // Mesh mesh = CreateQuadMesh();
            // string meshSavePath = collectionFolder + "\\" + collectionInfo.collectionName + "_mesh.asset";
            // AssetDatabase.CreateAsset(mesh,meshSavePath);
            //
            // Shader shader = Shader.Find("StarUnion/Efficient Animation Player");
            // if (shader == null)
            // {
            //     Debug.LogError("Can not find EfficientAnimationPlayer Shader");
            // }
            // Material material = new Material(shader);
            // string materialSavePath = collectionFolder + "\\" + collectionInfo.collectionName + "_material.asset";
            // AssetDatabase.CreateAsset(material,materialSavePath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Quaternion quaternion = Quaternion.Euler(_prefabRotation);
            string prefabSavePath = null;
            for (int i = 0; i < collectionInfo.roleInfosList.Count; i++)
            {
                if (collectionInfo.roleInfosList[i].animationInfosList.Count > 0)
                {
                    string roleName = collectionInfo.roleInfosList[i].animationInfosList[0].roleName;
                    string animationName = collectionInfo.roleInfosList[i].animationInfosList[0].animationName;
                    
                    GameObject efficientAnimationPrefab = new GameObject();
                    efficientAnimationPrefab.transform.rotation = quaternion;
                    MeshFilter meshFilter = efficientAnimationPrefab.AddComponent<MeshFilter>();
                    //meshFilter.mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshSavePath);
                    MeshRenderer meshRenderer = efficientAnimationPrefab.AddComponent<MeshRenderer>();
                    meshRenderer.sortingOrder = collectionInfo.firstId / EfficientAnimationPlayer.collectionMaterialMaxNumber + EfficientAnimationPlayer.sortingOrderStart;
                    //meshRenderer.material = AssetDatabase.LoadAssetAtPath<Material>(materialSavePath);
            
                    EfficientAnimationPlayer player = efficientAnimationPrefab.AddComponent<EfficientAnimationPlayer>();
                    player.animationCollectionInfo = _animationCollectionInfo;
                    player.thisRoleName = roleName;
                    player.thisAnimationName = animationName;
                    player.roleIdPlayNow = 0;
                    player.animationIdPlayNow = 0;
                    //prefabSavePath = collectionFolder + "\\" + collectionInfo.collectionName + "_" + roleName + ".prefab";
                    prefabSavePath = collectionFolder + "\\" + collectionInfo.collectionName + "_" + roleName + "_prefab.prefab";
                    PrefabUtility.SaveAsPrefabAsset(efficientAnimationPrefab,prefabSavePath);
                }
            }

            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            Camera.main.transform.position = new Vector3(0.0f, 0.4f, -4.0f);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabSavePath);
            Instantiate(prefab);
            Selection.activeObject = prefab;

            if (_mergeSimilarHeightTexturesToAtlas)
            {
                CreateAtlasTool.MergeTexturesWithSimilarHeight(texturesPath, _heightDifference,
                    maxTextureSize, _removeEmptyLine);
            }
            
            _canCloseNow = true;
        }

        private Vector2Int GetTextureSize(bool powOfTwo,int width,int height,int spriteNumber)
        {
            int textureWidth = 1;
            int textureHeight = 1;
            if (powOfTwo)
            {
                bool canFill = false;
                do
                {
                    if (textureWidth <= textureHeight)
                    {
                        textureWidth = textureWidth * 2;
                    }
                    else
                    {
                        textureHeight = textureHeight * 2;
                    }
                    int fillNumberInTexture = (textureWidth / width) * (textureHeight / height);
                    if (fillNumberInTexture < spriteNumber)
                    {
                        canFill = false;
                    }
                    else
                    {
                        canFill = true;
                    }
                } while (!canFill);
            }
            else
            {
                textureWidth = 0;
                textureHeight = 0;
                int addWidth = 4;
                int addHeight = 4;
                bool canFill = true;
                do
                {
                    if (textureWidth <= textureHeight)
                    {
                        textureWidth = textureWidth + addWidth;
                    }
                    else
                    {
                        textureHeight = textureHeight + addHeight;
                    }
                    int fillNumberInTexture = (textureWidth / width) * (textureHeight / height);
                    if (fillNumberInTexture < spriteNumber)
                    {
                        canFill = true;
                    }
                    else
                    {
                        canFill = false;
                    }
                } while (canFill);
                int widthNumber = textureWidth / width;
                textureWidth = (widthNumber * width + 3) / 4 * 4;
                int heightNumber = 1;
                while (widthNumber * heightNumber < spriteNumber)
                {
                    heightNumber = heightNumber + 1;
                }
                textureHeight = (height * heightNumber + 3) / 4 * 4;
            }
            return new Vector2Int(textureWidth, textureHeight);
        }

        private void CreateTexturesForSize(string folderPath,KeyValuePair<Vector2Int,List<EfficientAnimaitonInfo>> animaitonInfoList)
        {
            int width = animaitonInfoList.Key.x;
            int height = animaitonInfoList.Key.y;
            int spriteNumber = 0;
            foreach (var animaitonInfo in animaitonInfoList.Value)
            {
                spriteNumber = spriteNumber + animaitonInfo.animationTexture2Ds.Count;
            }
            int numberInMaxTexture = (maxTextureSize / width) * (maxTextureSize / height);
            int textureNumber = (spriteNumber + numberInMaxTexture - 1) / numberInMaxTexture;
            if (textureNumber == 1)
            {
                Vector2Int size = GetTextureSize(_powOfTwoTextureSize, width, height, spriteNumber);
                CreateTexture(folderPath,textureIndex,size.x,size.y,animaitonInfoList.Value);
                textureIndex = textureIndex + 1;
            }
            else
            {
                Queue<EfficientAnimaitonInfo> queue = new Queue<EfficientAnimaitonInfo>();
                foreach (var animationInfo in animaitonInfoList.Value)
                {
                    queue.Enqueue(animationInfo);
                }
                while (spriteNumber > numberInMaxTexture)
                {
                    List<EfficientAnimaitonInfo> textureInfo = new List<EfficientAnimaitonInfo>();
                    int numberInTexture = 0;
                    bool canFill = true;
                    while (canFill && queue.Count > 0)
                    {
                        EfficientAnimaitonInfo getForLook = queue.Peek();
                        if (numberInTexture + getForLook.animationTexture2Ds.Count <= numberInMaxTexture)
                        {
                            EfficientAnimaitonInfo getForUse = queue.Dequeue();
                            textureInfo.Add(getForUse);
                            numberInTexture = numberInTexture + getForUse.animationTexture2Ds.Count;
                        }
                        else
                        {
                            canFill = false;
                        }
                    }
                    if (numberInTexture > 0)
                    {
                        CreateTexture(folderPath,textureIndex,maxTextureSize,maxTextureSize,textureInfo);
                    }
                    else
                    {
                        EfficientAnimaitonInfo getForLook = queue.Peek();
                        Debug.LogError("Texture size is too big to fill in " + maxTextureSize + " ： " + getForLook.animationTexture2Ds[0].name);
                        return;
                    }
                    spriteNumber = spriteNumber - numberInTexture;
                    textureIndex = textureIndex + 1;
                }
                if (spriteNumber > 0)
                {
                    List<EfficientAnimaitonInfo> textureInfo = new List<EfficientAnimaitonInfo>();
                    while (queue.Count > 0)
                    {
                        textureInfo.Add(queue.Dequeue());
                    }
                    Vector2Int size = GetTextureSize(_powOfTwoTextureSize, width, height, spriteNumber);
                    CreateTexture(folderPath,textureIndex,size.x,size.y,textureInfo);
                    textureIndex = textureIndex + 1;
                }
            }
        }

        private Vector3[] GetVertexFromOffsetAndScale(Vector4 offsetAndScale)
        {
            Vector3[] vertexs = new Vector3[4];
            vertexs[0] = new Vector3(-0.5f * offsetAndScale.z + offsetAndScale.x,-0.5f * offsetAndScale.w + offsetAndScale.y);
            vertexs[1] = new Vector3(0.5f * offsetAndScale.z + offsetAndScale.x,-0.5f * offsetAndScale.w + offsetAndScale.y);
            vertexs[2] = new Vector3(0.5f * offsetAndScale.z + offsetAndScale.x,0.5f * offsetAndScale.w + offsetAndScale.y);
            vertexs[3] = new Vector3(-0.5f * offsetAndScale.z + offsetAndScale.x,0.5f * offsetAndScale.w + offsetAndScale.y);
            return vertexs;
        }

        private void CreateTexture(string folderPath,int index,int width, int height, List<EfficientAnimaitonInfo> animaitonInfoList)
        {
            AtlasInfo atlasInfo = new AtlasInfo();
            atlasInfo.id = _animationCollectionInfo.firstId + textureId;
            atlasInfo.atlasId = -1;
            int spriteWidth = animaitonInfoList[0].animationTexture2Ds[0].width;
            int spriteHeight = animaitonInfoList[0].animationTexture2Ds[0].height;
            float spriteAspectRatio = spriteWidth / (float)spriteHeight;
            int colNumber = width / spriteWidth;
            int rowNumber = height / spriteHeight;
            atlasInfo.colNumber = colNumber;
            atlasInfo.rowNumber = rowNumber;
            atlasInfo.colBlank = (width - spriteWidth * colNumber) / (float)width;
            atlasInfo.rowBlank = (height - spriteHeight * rowNumber) / (float)height;
            atlasInfo.animationLocations = new List<AnimationLocationInfo>();
            _animationCollectionInfo.atlasInfos.Add(atlasInfo);
            Texture2D newTexture = new Texture2D(width, height,TextureFormat.RGBA32,false);
            for (int i = 0; i < newTexture.width; i++)
            {
                for (int j = 0; j < newTexture.height; j++)
                {
                    newTexture.SetPixel(i,j,Color.clear);
                }
            }
            List<Texture2D> allSprites = new List<Texture2D>();
            int animationId = 0;
            for (int i = 0; i < animaitonInfoList.Count; i++)
            {
                string roleAndAnimationName = animaitonInfoList[i].roleName + "_" + animaitonInfoList[i].animationName;
                Vector2Int animationLocation = new Vector2Int(textureId,i);
                _animationCollectionInfo.roleAndAnimationNames.Add(roleAndAnimationName);
                _animationCollectionInfo.animationLocations.Add(animationLocation);
                AnimationLocationInfo animationInfo = new AnimationLocationInfo();
                animationInfo.AnimationID = i;
                animationInfo.RoleName = animaitonInfoList[i].roleName;
                animationInfo.AnimationName = animaitonInfoList[i].animationName;
                animationInfo.LocationMin = animationId;
                animationId = animationId + animaitonInfoList[i].animationTexture2Ds.Count;
                animationInfo.LocationMax = animationId - 1;
                animationInfo.FPS = animaitonInfoList[i].fps;
                animationInfo.aspect = spriteAspectRatio;
                animationInfo.vertices = GetVertexFromOffsetAndScale(animaitonInfoList[i].GetOffsetAndScale());
                atlasInfo.animationLocations.Add(animationInfo);
                
                for (int j = 0; j < animaitonInfoList[i].animationTexture2Ds.Count; j++)
                {
                    allSprites.Add(animaitonInfoList[i].animationTexture2Ds[j]);
                }
            }
            for (int i = 0; i < allSprites.Count; i++)
            {
                int locationX = i % colNumber * spriteWidth;
                int locationY = i / colNumber * spriteHeight;
                Color[] colorsBlock = allSprites[i].GetPixels();
                newTexture.SetPixels(locationX, locationY, spriteWidth,spriteHeight,colorsBlock);
            }
            newTexture.Apply();
            byte[] textureData = ImageConversion.EncodeToPNG(newTexture);
            //string texturePath = folderPath + "\\" + _animationCollectionInfo.collectionName + "_" + index + "_" + width + "_" + height + ".png";
            string texturePath = folderPath + "\\" + _animationCollectionInfo.collectionName + "_" + index + "_tex.png";
            FileStream file = File.Open(texturePath, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(file);
            writer.Write(textureData);
            file.Close();
            DestroyImmediate(newTexture);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            textureImporter.alphaIsTransparency = true;
            textureImporter.mipmapEnabled = false;
            if (!_powOfTwoTextureSize)
            {
                textureImporter.npotScale = TextureImporterNPOTScale.None;
            }
            AssetDatabase.ImportAsset(texturePath);
            atlasInfo.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            atlasInfo.offsetAndScale = new Vector4(0, 0, 1, 1);
            textureId = textureId + 1;
        }
    }
}
