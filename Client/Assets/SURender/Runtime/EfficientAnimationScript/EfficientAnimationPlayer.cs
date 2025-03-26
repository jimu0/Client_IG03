using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarUnion_EfficientAnimation
{
    public class EfficientAnimationPlayer : MonoBehaviour
    {
        public int atlasId = 0;
        public int animationId = 0;
        [SerializeField]
        public string thisRoleName;
        [SerializeField]
        public string thisAnimationName;
        [SerializeField]
        public int roleIdPlayNow;
        [SerializeField]
        public int animationIdPlayNow;
        public bool loop = true;
        [SerializeField] 
        public EfficientAnimationShaderType playShaderType;
        [Range(0.0f,1.0f)]
        public float loopOffset = 0.0f;
        [Range(0.0f,10.0f)]
        public float speedScale = 1.0f;
        public Color color = Color.white;
        public AnimationCollectionInfo animationCollectionInfo;
        private Mesh _mesh;
        public MeshRenderer _meshRenderer;
        private List<Vector4> _uv0List;
        private List<Vector4> _uv1List;
        private List<Vector4> _uv2List;
        private List<Vector4> _uv3List;
        private List<Vector4> _uv4List;
        private List<Color> _colorList;
        public Vector4 _uv1;
        public Vector4 _uv2;
        public Vector4 _uv3;
        public Vector4 _uv4;
        private AtlasInfo _atlasInfo;
        private int _onceOffset = 0;
        private Dictionary<string, Vector2Int> _animationLocation;
        private Dictionary<string, Dictionary<string, Vector2Int>> _roleAnimationsID;
        private string[] _roleNames;
        private bool _thisInit = false;
        private static Dictionary<int, int> _collectionReferenceCount;
        private static Dictionary<int, bool> _animationCollectionInit;
        private static Dictionary<int, Dictionary<string, Dictionary<string, Vector2Int>>> _roleAnimationsIDDictionary;
        private static Dictionary<int, Material> _materialAll;
        private static Dictionary<int, List<Material>> _effectMaterialAll;
        private static Dictionary<int, List<EfficientAnimationShaderType>> _effectMaterialTypeAll;
        private static Dictionary<int, MaterialPropertyBlock> _MaterialPropertyBlockAll;
        public static int collectionMaterialMaxNumber = 1000;
        public static int effectMaterialTypeStep = 100;
        public static int sortingOrderStart = -10000;

        private Action animationCallBack;
        private float callTimeForCallBack;
        private float timeNowForCallBack;
        private int pauseFrameIndex;
        private bool IsPause;
        private bool resetToDefault;
        private float resetToDefaultTimeNow;
        private float resetToDefaultTimeStep;

        public static void ReleaseCollection(int index)
        {
            _animationCollectionInit[index] = false;
            _roleAnimationsIDDictionary[index].Clear();
            _roleAnimationsIDDictionary.Remove(index);
            for (int i = 0; i < collectionMaterialMaxNumber; i++)
            {
                if (_MaterialPropertyBlockAll.ContainsKey(index+i))
                {
                    if (_materialAll.ContainsKey(index+i))
                    {
                        _materialAll.Remove(index + i);
                    }
                    if (_effectMaterialAll.ContainsKey(index+i))
                    {
                        _effectMaterialAll[index+i].Clear();
                        _effectMaterialAll.Remove(index+i);
                        _effectMaterialTypeAll[index+i].Clear();
                        _effectMaterialTypeAll.Remove(index+i);
                    }
                    _MaterialPropertyBlockAll.Remove(index + i);
                }
                else
                {
                    break;
                }
            }
        }

        public Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[4];
            _uv0List = new List<Vector4>();
            Vector4[] uvsDefault = new Vector4[4];
            int[] triangles = new int[6]{0,2,1,0,3,2};
            Color[] colors = new Color[4];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
                uvsDefault[i] = Vector4.zero;
            }
            vertices[0] = new Vector3(-0.5f, -0.5f);
            _uv0List.Add(new Vector4(0.0f, 0.0f,0.0f, 0.0f));
            vertices[1] = new Vector3(0.5f, -0.5f);
            _uv0List.Add(new Vector4(1.0f, 0.0f,1.0f, 0.0f));
            vertices[2] = new Vector3(0.5f, 0.5f);
            _uv0List.Add(new Vector4(1.0f, 1.0f,1.0f, 1.0f));
            vertices[3] = new Vector3(-0.5f, 0.5f);
            _uv0List.Add(new Vector4(0.0f, 1.0f,0.0f, 1.0f));
            mesh.vertices = vertices;
            //mesh.uv = uvs;
            mesh.SetUVs(0,_uv0List);
            mesh.SetUVs(1,uvsDefault);
            mesh.SetUVs(2,uvsDefault);
            mesh.SetUVs(3,uvsDefault);
            mesh.SetUVs(4,uvsDefault);
            mesh.triangles = triangles;
            mesh.colors = colors;
            return mesh;
        }

        private void ResetEffectToDefault()
        {
            _meshRenderer.sharedMaterial = _materialAll[_atlasInfo.id];
            playShaderType = EfficientAnimationShaderType.Base;
        }
        
        public void PlayEffect(EfficientAnimationShaderType type,bool reset = true)
        {
            if (type == EfficientAnimationShaderType.Base)
            {
                _meshRenderer.sharedMaterial = _materialAll[_atlasInfo.id];
                playShaderType = EfficientAnimationShaderType.Base;
            }
            else
            {
                int shaderTypeIndex = -1;
                for (int i = 0; i < animationCollectionInfo.effectShaderTypes.Count; i++)
                {
                    if (type == animationCollectionInfo.effectShaderTypes[i])
                    {
                        shaderTypeIndex = i;
                        break;
                    }
                }
                if (shaderTypeIndex != -1)
                {
                    _meshRenderer.sharedMaterial = _effectMaterialAll[_atlasInfo.id][shaderTypeIndex];
                    playShaderType = _effectMaterialTypeAll[_atlasInfo.id][shaderTypeIndex];
                    if (reset && (int)type >= 16)
                    {
                        resetToDefault = true;
                    }
                    else
                    {
                        resetToDefault = false;
                    }
                    SetPlayEffectData();
                    resetToDefaultTimeStep = _meshRenderer.sharedMaterial.GetFloat("_EffectTime");
                    resetToDefaultTimeNow = 0.0f;
                }
                else
                {
                    Debug.LogError( animationCollectionInfo.name + " do not find material,type is : " + Enum.GetName(typeof(EfficientAnimationShaderType),type));
                }
            }
        }

        void Awake()
        {
            if (animationCollectionInfo == null)
            {
                Debug.LogError(gameObject.name + " : EfficientAnimationPlayer do not have AnimationCollectionInfo");
                return;
            }
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                Debug.LogError(gameObject.name + " : EfficientAnimationPlayer script can only add to game object with MeshFilter");
                return;
            }

            if (_collectionReferenceCount == null)
            {
                _collectionReferenceCount = new Dictionary<int, int>();
            }

            if (_animationCollectionInit == null)
            {
                _animationCollectionInit = new Dictionary<int, bool>();
            }

            if (_roleAnimationsIDDictionary == null)
            {
                _roleAnimationsIDDictionary = new Dictionary<int, Dictionary<string, Dictionary<string, Vector2Int>>>();
            }

            if (_materialAll == null)
            {
                _materialAll = new Dictionary<int, Material>();
                _effectMaterialAll = new Dictionary<int, List<Material>>();
                _effectMaterialTypeAll = new Dictionary<int, List<EfficientAnimationShaderType>>();
            }

            if (_MaterialPropertyBlockAll == null)
            {
                _MaterialPropertyBlockAll = new Dictionary<int, MaterialPropertyBlock>();
            }
            
            _mesh = CreateQuadMesh();
            meshFilter.mesh = _mesh;
            _meshRenderer = GetComponent<MeshRenderer>();
            if (_meshRenderer == null)
            {
                Debug.LogError(gameObject.name + " : EfficientAnimationPlayer script can only add to game object with MeshFilter");
                return;
            }
            _meshRenderer.sortingOrder = animationCollectionInfo.firstId / collectionMaterialMaxNumber + sortingOrderStart;
            
            /*Shader shader = Shader.Find("StarUnion/Efficient Animation Player");
            if (shader == null)
            {
                Debug.LogError("Can not find EfficientAnimationPlayer Shader");
                return;
            }*/

            loopOffset = Random.Range(0.0f, 1.0f);
            if (!_animationCollectionInit.ContainsKey(animationCollectionInfo.firstId) || !_animationCollectionInit[animationCollectionInfo.firstId])
            {
                if (animationCollectionInfo.atlasInfos.Count == 0)
                {
                    _thisInit = false;
                    return;
                }
                foreach (var atlas in animationCollectionInfo.atlasInfos)
                {
                    if (!_materialAll.ContainsKey(atlas.id))
                    {
                        if (atlas.atlasId > -1 && _materialAll.ContainsKey(atlas.atlasId))
                        {
                            _materialAll.Add(atlas.id,_materialAll[atlas.atlasId]);
                            _effectMaterialAll.Add(atlas.id,_effectMaterialAll[atlas.atlasId]);
                            _effectMaterialTypeAll.Add(atlas.id,_effectMaterialTypeAll[atlas.atlasId]);
                        }
                        else
                        {
                            Material material = new Material(animationCollectionInfo.defaultMaterial);
                            material.CopyPropertiesFromMaterial(animationCollectionInfo.defaultMaterial);
                            material.DisableKeyword("_PREVIEWMODE");
                            material.name = atlas.id.ToString();
                            material.renderQueue = 3000 + atlas.id - animationCollectionInfo.firstId;
                            _materialAll.Add(atlas.id,material);
                            
                            if (animationCollectionInfo.effectMaterials != null && animationCollectionInfo.effectMaterials.Count > 0)
                            {
                                List<EfficientAnimationShaderType> types = new List<EfficientAnimationShaderType>();
                                List<Material> materials = new List<Material>();
                                for (int i = 0; i < animationCollectionInfo.effectMaterials.Count; i++)
                                {
                                    Material effectMaterial = new Material(animationCollectionInfo.effectMaterials[i]);
                                    effectMaterial.CopyPropertiesFromMaterial(animationCollectionInfo.effectMaterials[i]);
                                    effectMaterial.DisableKeyword("_PREVIEWMODE");
                                    effectMaterial.renderQueue = 3000 + (atlas.id - animationCollectionInfo.firstId) + effectMaterialTypeStep * (i + 1);
                                    materials.Add(effectMaterial);
                                    types.Add(animationCollectionInfo.effectShaderTypes[i]);
                                }
                                _effectMaterialAll.Add(atlas.id,materials);
                                _effectMaterialTypeAll.Add(atlas.id,types);
                            }
                        }
                        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
                        materialPropertyBlock.SetInt("_ColNum",atlas.colNumber);
                        materialPropertyBlock.SetInt("_RowNum",atlas.rowNumber);
                        materialPropertyBlock.SetFloat("_ColBlank",atlas.colBlank);
                        materialPropertyBlock.SetFloat("_RowBlank",atlas.rowBlank);
                        materialPropertyBlock.SetTexture("_MainTex",atlas.texture);
                        _MaterialPropertyBlockAll.Add(atlas.id,materialPropertyBlock);
                    }
                }
                if (_roleAnimationsIDDictionary.ContainsKey(animationCollectionInfo.firstId))
                {
                    _roleAnimationsID = _roleAnimationsIDDictionary[animationCollectionInfo.firstId];
                    _roleAnimationsID.Clear();
                }
                else
                {
                    _roleAnimationsID = new Dictionary<string, Dictionary<string, Vector2Int>>();
                    _roleAnimationsIDDictionary.Add(animationCollectionInfo.firstId,_roleAnimationsID);
                }

                for (int i = 0; i < animationCollectionInfo.roleAndAnimationNames.Count; i++)
                {
                    string roleAndAnimationName = animationCollectionInfo.roleAndAnimationNames[i];
                    string roleName = roleAndAnimationName.Substring(0, roleAndAnimationName.IndexOf("_"));
                    string animationName = roleAndAnimationName.Substring(roleAndAnimationName.IndexOf("_")+1);
                    Dictionary<string, Vector2Int> animationNameDictionary;
                    if (_roleAnimationsID.ContainsKey(roleName))
                    {
                        animationNameDictionary = _roleAnimationsID[roleName];
                    }
                    else
                    {
                        animationNameDictionary = new Dictionary<string, Vector2Int>();
                        _roleAnimationsID.Add(roleName, animationNameDictionary);
                    }
                    animationNameDictionary.Add(animationName,animationCollectionInfo.animationLocations[i]);
                }
                if (!_animationCollectionInit.ContainsKey(animationCollectionInfo.firstId))
                {
                    _animationCollectionInit.Add(animationCollectionInfo.firstId,true);
                }
                else
                {
                    _animationCollectionInit[animationCollectionInfo.firstId] = true;
                }
                _thisInit = true;
            }
            else
            {
                _roleAnimationsID = _roleAnimationsIDDictionary[animationCollectionInfo.firstId];
                _thisInit = true;
            }
            _uv0List = new List<Vector4>();
            _uv1List = new List<Vector4>();
            _uv2List = new List<Vector4>();
            _uv3List = new List<Vector4>();
            _uv4List = new List<Vector4>();
            _colorList = new List<Color>();
            _mesh.GetUVs(0, _uv0List);
            _mesh.GetUVs(1, _uv1List);
            _mesh.GetUVs(2, _uv2List);
            _mesh.GetUVs(3, _uv3List);
            _mesh.GetUVs(4, _uv4List);
            _mesh.GetColors(_colorList);
            _uv1 = new Vector4();
            _uv2 = new Vector4();
            _uv3 = new Vector4();
            _uv4 = new Vector4();
            _roleNames = _roleAnimationsID.Keys.ToArray();
            SetRole(thisRoleName);
            PlayAnimation(thisAnimationName);
            if (animationCollectionInfo.initShaderType == EfficientAnimationShaderType.Base)
            {
                _meshRenderer.sharedMaterial = _materialAll[_atlasInfo.id];
            }
            else
            {
                int effectShaderTypeIndex = -1;
                for (int i = 0; i < animationCollectionInfo.effectShaderTypes.Count; i++)
                {
                    if (animationCollectionInfo.initShaderType == animationCollectionInfo.effectShaderTypes[i])
                    {
                        effectShaderTypeIndex = i;
                        break;
                    }
                }
                if (effectShaderTypeIndex != -1)
                {
                    _meshRenderer.sharedMaterial = _effectMaterialAll[_atlasInfo.id][effectShaderTypeIndex];
                }
                else
                {
                    _meshRenderer.sharedMaterial = _materialAll[_atlasInfo.id];
                }
            }
            if (!_collectionReferenceCount.ContainsKey(animationCollectionInfo.firstId))
            {
                _collectionReferenceCount.Add(animationCollectionInfo.firstId,0);
            }
            _collectionReferenceCount[animationCollectionInfo.firstId]++;
        }

        private void OnDestroy()
        {
            if (_collectionReferenceCount.ContainsKey(animationCollectionInfo.firstId))
            {
                _collectionReferenceCount[animationCollectionInfo.firstId]--;
                if (_collectionReferenceCount[animationCollectionInfo.firstId] <= 0)
                {
                    ReleaseCollection(animationCollectionInfo.firstId);
                }
            }
        }

        public string[] GetRoleNames()
        {
            return _roleNames;
        }

        public string[] GetAnimationNames(string roleName)
        {
            if (_roleAnimationsID != null && _roleAnimationsID.ContainsKey(roleName))
            {
                return _roleAnimationsID[roleName].Keys.ToArray();
            }
            return null;
        }

        public bool SetRole(string roleName)
        {
            if (animationCollectionInfo == null)
            {
                Debug.LogError(gameObject.name + ": animationCollectionInfo is null");
                return false;
            }
            if (!_thisInit)
            {
                Debug.LogError(gameObject.name + ": not init");
                return false;
            }
            if (_roleAnimationsID.ContainsKey(roleName))
            {
                thisRoleName = roleName;
                _animationLocation = _roleAnimationsID[thisRoleName];
                return true;
            }
            return false;
        }
        
        public bool PlayAnimationLoop(string animationName,Action callBack = null,float animationTime = 0.0f)
        {
            if (!_thisInit)
            {
                Debug.LogError(gameObject.name + ": not init");
                return false;
            }
            thisAnimationName = animationName;
            return PlayAnimation(animationName, true,callBack,animationTime);
        }
        
        public bool PlayAnimationOnce(string animationName,Action callBack = null,float animationTime = 0.0f)
        {
            if (!_thisInit)
            {
                Debug.LogError(gameObject.name + ": not init");
                return false;
            }
            thisAnimationName = animationName;
            return PlayAnimation(animationName, false,callBack,animationTime);
        }

        public bool PlayAnimation(string animationName,bool playLoop = true,Action callBack = null,float animationTime = 0.0f)
        {
            if (animationCollectionInfo == null)
            {
                Debug.LogError(gameObject.name + ": animationCollectionInfo is null");
                return false;
            }
            if (!_thisInit)
            {
                Debug.LogError(gameObject.name + ": not init");
                return false;
            }
            if (_animationLocation == null && _roleAnimationsID.ContainsKey(thisRoleName))
            {
                _animationLocation = _roleAnimationsID[thisRoleName];
            }
            if (animationName != null && _animationLocation.ContainsKey(animationName))
            {
                thisAnimationName = animationName;
                Vector2Int location = _animationLocation[animationName];
                atlasId = location.x;
                animationId = location.y;
                return PlayAnimation(location.x,location.y,playLoop,callBack,animationTime);
            }
            if (!_animationLocation.ContainsKey(animationName))
            {
                Debug.LogError("can not find animation named " + animationName);
            }
            return false;
        }

        private void SetPlayEffectData()
        {
            _uv4.x = Shader.GetGlobalVector("_Time").y;
            _uv4.y = transform.position.x;
            _uv4.z = transform.position.y;
            _uv4.w = transform.position.z;
            for (int i = 0; i < _uv4List.Count; i++)
            {
                _uv4List[i] = _uv4;
            }
            _mesh.SetUVs(4,_uv4List);
        }
    
        private bool PlayAnimation(int id,int index,bool playLoop = true,Action callBack = null,float animationTime = 0.0f)
        {
            if (animationCollectionInfo == null)
            {
                Debug.LogError(gameObject.name + ": animationCollectionInfo is null");
                return false;
            }
            if (id < 0 || id >= animationCollectionInfo.atlasInfos.Count)
            {
                return false;
            }
            _atlasInfo = animationCollectionInfo.atlasInfos[id];
            if (_atlasInfo == null)
            {
                return false;
            }
            if (index < 0 || index >= _atlasInfo.animationLocations.Count)
            {
                return false;
            }
            atlasId = id;
            animationId = index;
            loop = playLoop;

            if (playShaderType == EfficientAnimationShaderType.Base)
            {
                _meshRenderer.sharedMaterial = _materialAll[_atlasInfo.id];
            }
            else
            {
                int effectMaterialIndex = -1;
                for (int i = 0; i < _effectMaterialTypeAll[_atlasInfo.id].Count; i++)
                {
                    if (playShaderType == _effectMaterialTypeAll[_atlasInfo.id][i])
                    {
                        effectMaterialIndex = i;
                        break;
                    }
                }
                if (effectMaterialIndex != -1)
                {
                    _meshRenderer.sharedMaterial = _effectMaterialAll[_atlasInfo.id][effectMaterialIndex];
                }
            }
            _meshRenderer.SetPropertyBlock(_MaterialPropertyBlockAll[_atlasInfo.id]);

            _uv1.x = _atlasInfo.animationLocations[index].LocationMin % _atlasInfo.colNumber + 0.5f;
            // ReSharper disable once PossibleLossOfFraction
            _uv1.y = _atlasInfo.animationLocations[index].LocationMin / _atlasInfo.colNumber + 0.5f;
            _uv1.z = _atlasInfo.animationLocations[index].LocationMax - _atlasInfo.animationLocations[index].LocationMin;
            if (animationTime > 0.001f)
            {
                speedScale = (_uv1.z + 1.0f) / _atlasInfo.animationLocations[index].FPS / animationTime;
            }
            _uv1.w = _atlasInfo.animationLocations[index].FPS * speedScale;
            _uv2.x = playLoop ? 0 : 1;
            _uv2.y = Mathf.RoundToInt(loopOffset * (_uv1.z + 1));
            _uv2.z = Shader.GetGlobalVector("_Time").y;
            _onceOffset = 0;
            _uv2.w = _onceOffset;
            _uv3 = _atlasInfo.offsetAndScale;
            for (int i = 0; i < _uv1List.Count; i++)
            {
                _uv1List[i] = _uv1;
                _uv2List[i] = _uv2;
                _uv3List[i] = _uv3;
                _colorList[i] = color;
            }
            
            for (int i = 0; i < _uv0List.Count; i++)
            {
                float uvZ = 0;
                if (_uv0List[i].z > 0)
                {
                    uvZ = _atlasInfo.animationLocations[animationId].aspect;
                }
                _uv0List[i] = new Vector4(_uv0List[i].x,_uv0List[i].y,uvZ,_uv0List[i].w);
            }
            
            _mesh.SetUVs(0,_uv0List);
            _mesh.SetUVs(1,_uv1List);
            _mesh.SetUVs(2,_uv2List);
            _mesh.SetUVs(3,_uv3List);
            _mesh.SetColors(_colorList);

            _mesh.vertices = _atlasInfo.animationLocations[index].vertices;
            if (callBack != null && !loop)
            {
                animationCallBack = callBack;
                callTimeForCallBack = (_uv1.z + 1.0f) / _uv1.w;
                timeNowForCallBack = 0.0f;
            }
            return true;
        }

        private void Update()
        {
            if (animationCallBack != null && !loop && !IsPause)
            {
                timeNowForCallBack += Time.deltaTime;
                if (timeNowForCallBack > callTimeForCallBack)
                {
                    animationCallBack();
                    timeNowForCallBack = 0.0f;
                    callTimeForCallBack = 0.0f;
                    animationCallBack = null;
                }
            }
            if (resetToDefault)
            {
                resetToDefaultTimeNow += Time.deltaTime;
                if (resetToDefaultTimeNow > resetToDefaultTimeStep)
                {
                    ResetEffectToDefault();
                    resetToDefault = false;
                    resetToDefaultTimeNow = 0.0f;
                    resetToDefaultTimeStep = 0.0f;
                }
            }
        }

        public int GetFrameNow()
        {
            float timeInShader = Shader.GetGlobalVector("_Time").y;
            int frameNow;
            if (loop)
            {
                frameNow = (int)((timeInShader * _uv1.w + _uv2.y) % (_uv1.z+1));
            }
            else
            {
                frameNow = (int)Mathf.Clamp((timeInShader - _uv2.z) * _uv1.w + _uv2.w,0,_uv1.z);
            }
            return frameNow;
        }

        public void SetSpeedScale(float scale)
        {
            if (animationId < 0 || animationId >= _atlasInfo.animationLocations.Count)
            {
                return;
            }
            if (animationCollectionInfo == null)
            {
                Debug.LogError(gameObject.name + ": animationCollectionInfo is null");
                return;
            }
            if (!_thisInit)
            {
                Debug.LogError(gameObject.name + ": not init");
                return;
            }
            float timeInShader = Shader.GetGlobalVector("_Time").y;
            int fps = _atlasInfo.animationLocations[animationId].FPS;

            if (loop)
            {
                int frameNow = (int)((timeInShader * _uv1.w + _uv2.y) % (_uv1.z+1));
                int frameAfterScaleWithoutOffset = (int)((timeInShader * fps * scale) % (_uv1.z+1));
                int offset = (int)((frameNow - frameAfterScaleWithoutOffset + _uv1.z + 1) % (_uv1.z + 1));
                loopOffset = offset / (_uv1.z + 1);
                _uv2.y = loopOffset * (_uv1.z + 1);
            }
            else
            {
                int frameNow = (int)Mathf.Clamp((timeInShader - _uv2.z) * _uv1.w + _uv2.w,0,_uv1.z);
                int frameAfterScaleWithoutOffset = (int)Mathf.Clamp((timeInShader - _uv2.z) * fps * scale,0,_uv1.z);
                _onceOffset = (int)((_onceOffset + frameNow - frameAfterScaleWithoutOffset) % (_uv1.z + 1));
                _uv2.w = _onceOffset;
            }
            
            speedScale = scale;
            _uv1.w = _atlasInfo.animationLocations[animationId].FPS * speedScale;
            for (int i = 0; i < _uv1List.Count; i++)
            {
                _uv1List[i] = _uv1;
                _uv2List[i] = _uv2;
            }
            _mesh.SetUVs(1,_uv1List);
            _mesh.SetUVs(2,_uv2List);
        }

        public void Pause()
        {
            if (animationCollectionInfo == null)
            {
                Debug.LogError(gameObject.name + ": animationCollectionInfo is null");
                return;
            }
            if (!_thisInit)
            {
                Debug.LogError(gameObject.name + ": not init");
                return;
            }
            
            float timeInShader = Shader.GetGlobalVector("_Time").y;

            if (loop)
            {
                int frameNow = (int)((timeInShader * _uv1.w + _uv2.y) % (_uv1.z+1));
                pauseFrameIndex = frameNow;
                loopOffset = pauseFrameIndex / (_uv1.z + 1);
                _uv2.y = loopOffset * (_uv1.z + 1);
            }
            else
            {
                int frameNow = (int)Mathf.Clamp((timeInShader - _uv2.z) * _uv1.w + _uv2.w,0,_uv1.z);
                pauseFrameIndex = frameNow;
                _onceOffset = pauseFrameIndex;
                _uv2.w = _onceOffset;
            }
            
            _uv1.w = 0;
            for (int i = 0; i < _uv1List.Count; i++)
            {
                _uv1List[i] = _uv1;
                _uv2List[i] = _uv2;
            }
            _mesh.SetUVs(1,_uv1List);
            _mesh.SetUVs(2,_uv2List);

            IsPause = true;
        }

        public void Resume()
        {
            if (animationCollectionInfo == null)
            {
                Debug.LogError(gameObject.name + ": animationCollectionInfo is null");
                return;
            }
            if (!_thisInit)
            {
                Debug.LogError(gameObject.name + ": not init");
                return;
            }
            
            float timeInShader = Shader.GetGlobalVector("_Time").y;
            int fps = _atlasInfo.animationLocations[animationId].FPS;

            if (loop)
            {
                int frameAfterScaleWithoutOffset = (int)((timeInShader * fps * speedScale) % (_uv1.z+1));
                int offset = (int)((pauseFrameIndex - frameAfterScaleWithoutOffset + _uv1.z + 1) % (_uv1.z + 1));
                loopOffset = offset / (_uv1.z + 1);
                _uv2.y = loopOffset * (_uv1.z + 1);
            }
            else
            {
                int frameAfterScaleWithoutOffset = (int)Mathf.Clamp((timeInShader - _uv2.z) * fps * speedScale,0,_uv1.z);
                _onceOffset = (int)((pauseFrameIndex - frameAfterScaleWithoutOffset) % (_uv1.z + 1));
                _uv2.w = _onceOffset;
            }
            
            _uv1.w = _atlasInfo.animationLocations[animationId].FPS * speedScale;
            for (int i = 0; i < _uv1List.Count; i++)
            {
                _uv1List[i] = _uv1;
                _uv2List[i] = _uv2;
            }
            _mesh.SetUVs(1,_uv1List);
            _mesh.SetUVs(2,_uv2List);
            
            IsPause = false;
        }

        public void SetColorTint(Color tint,float strength = 1)
        {
            if (animationCollectionInfo == null)
            {
                Debug.LogError(gameObject.name + ": animationCollectionInfo is null");
                return;
            }
            if (!_thisInit)
            {
                Debug.LogError(gameObject.name + ": not init");
                return;
            }
            color = tint;
            color.r *= strength;
            color.g *= strength;
            color.b *= strength;
            for (int i = 0; i < _colorList.Count; i++)
            {
                _colorList[i] = color;
            }
            _mesh.SetColors(_colorList);
        }

        public void SetLoopOffset(float offset)
        {
            if (animationCollectionInfo == null)
            {
                Debug.LogError(gameObject.name + ": animationCollectionInfo is null");
                return;
            }
            if (!_thisInit)
            {
                Debug.LogError(gameObject.name + ": not init");
                return;
            }
            loopOffset = offset;
            _uv2.y = Mathf.RoundToInt(loopOffset * _uv1.z);
            for (int i = 0; i < _uv2List.Count; i++)
            {
                _uv2List[i] = _uv2;
            }
            _mesh.SetUVs(2,_uv2List);
        }
    }
}

