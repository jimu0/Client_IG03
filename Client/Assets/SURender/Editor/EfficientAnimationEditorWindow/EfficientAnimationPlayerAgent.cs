using System;
using System.Collections.Generic;
using System.Linq;
using StarUnion_EfficientAnimation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarUnion_EfficientAnimation_Tool
{
    public class EfficientAnimationPlayerAgent
    {
        public int atlasId = 0;
        public int animationId = 0;
        public bool loop = true;
        [Range(0.0f,1.0f)]
        public float loopOffset = 0.0f;
        [Range(0.0f,10.0f)]
        public float speedScale = 1.0f;
        public Color color = Color.white;
        public AnimationCollectionInfo animationCollectionInfo;
        public string thisRoleName;
        public string thisAnimationName;
        public int roleIdPlayNow;
        public int animationIdPlayNow;
        public EfficientAnimationShaderType playShaderType;
        private Mesh _mesh;
        private Material _material;
        private MaterialPropertyBlock _materialPropertyBlock;
        private MeshRenderer _meshRenderer;
        private List<Vector4> _uv0List;
        private List<Vector4> _uv1List;
        private List<Vector4> _uv2List;
        private List<Vector4> _uv3List;
        private List<Color> _colorList;
        private Vector4 _uv1;
        private Vector4 _uv2;
        private Vector4 _uv3;
        private AtlasInfo _atlasInfo;
        private int onceOffset = 0;
        private Dictionary<string, Vector2Int> animationLocation;
        private Dictionary<string, Dictionary<string, Vector2Int>> roleAnimationsID;
        private string[] roleNames;
        public bool thisInit = false;

        public EfficientAnimationPlayerAgent(AnimationCollectionInfo collectionInfo,Mesh mesh,Material material,MeshRenderer meshRenderer)
        {
            animationCollectionInfo = collectionInfo;
            _mesh = mesh;
            _material = material;
            _meshRenderer = meshRenderer;
            _materialPropertyBlock = new MaterialPropertyBlock();
            Init();
        }

        public void PlayEffect(EfficientAnimationShaderType type)
        {
            if (type == EfficientAnimationShaderType.Base)
            {
                _meshRenderer.sharedMaterial = animationCollectionInfo.defaultMaterial;
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
                    _meshRenderer.sharedMaterial = animationCollectionInfo.effectMaterials[shaderTypeIndex];
                    playShaderType = animationCollectionInfo.effectShaderTypes[shaderTypeIndex];
                    /*if ((int)type >= 16)
                    {
                        SetPlayEffectData();
                    }*/
                }
                else
                {
                    Debug.LogError( animationCollectionInfo.name + " do not find material,type is : " + Enum.GetName(typeof(EfficientAnimationShaderType),type));
                }
            }
        }

        void Init()
        {
            if (animationCollectionInfo == null)
            {
                Debug.LogError("EfficientAnimationPlayer do not have AnimationCollectionInfo");
                return;
            }

            loopOffset = Random.Range(0.0f, 1.0f);
            if (animationCollectionInfo.atlasInfos.Count == 0)
            {
                Debug.LogError(_meshRenderer.gameObject.name+" have no resource!");
                return;
            }
            AtlasInfo atlas = animationCollectionInfo.atlasInfos[0];
            _materialPropertyBlock.SetInt("_ColNum",atlas.colNumber);
            _materialPropertyBlock.SetInt("_RowNum",atlas.rowNumber);
            _materialPropertyBlock.SetFloat("_ColBlank",atlas.colBlank);
            _materialPropertyBlock.SetFloat("_RowBlank",atlas.rowBlank);
            _materialPropertyBlock.SetTexture("_MainTex",atlas.texture);
            //在运行时初始化时不能设置材质的参数，因为每次选中预制体都会执行初始化，覆盖运行时设置的材质参数。
            if (!Application.isPlaying)
            {
                _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
            }

            roleAnimationsID = new Dictionary<string, Dictionary<string, Vector2Int>>();

            for (int i = 0; i < animationCollectionInfo.roleAndAnimationNames.Count; i++)
            {
                string roleAndAnimationName = animationCollectionInfo.roleAndAnimationNames[i];
                string roleName = roleAndAnimationName.Substring(0, roleAndAnimationName.IndexOf("_"));
                string animationName = roleAndAnimationName.Substring(roleAndAnimationName.IndexOf("_")+1);
                Dictionary<string, Vector2Int> animationNameDictionary;
                if (roleAnimationsID.ContainsKey(roleName))
                {
                    animationNameDictionary = roleAnimationsID[roleName];
                }
                else
                {
                    animationNameDictionary = new Dictionary<string, Vector2Int>();
                    roleAnimationsID.Add(roleName, animationNameDictionary);
                }
                animationNameDictionary.Add(animationName,animationCollectionInfo.animationLocations[i]);
            }
            thisInit = true;
            roleNames = roleAnimationsID.Keys.ToArray();
            SetRole(roleNames[0]);

            _uv0List = new List<Vector4>();
            _uv1List = new List<Vector4>();
            _uv2List = new List<Vector4>();
            _uv3List = new List<Vector4>();
            _colorList = new List<Color>();
            _mesh.GetUVs(0, _uv0List);
            _mesh.GetUVs(1, _uv1List);
            _mesh.GetUVs(2, _uv2List);
            _mesh.GetUVs(3, _uv3List);
            _mesh.GetColors(_colorList);
            _uv1 = new Vector4();
            _uv2 = new Vector4();
            _uv3 = new Vector4();
            SetRole(thisRoleName);
            PlayAnimation(thisAnimationName);
        }

        public bool SetRole(string roleName)
        {
            if (animationCollectionInfo == null)
            {
                Debug.LogError("animationCollectionInfo is null");
                return false;
            }
            if (!thisInit)
            {
                Debug.LogError("not init");
                return false;
            }
            if (roleAnimationsID.ContainsKey(roleName))
            {
                animationLocation = roleAnimationsID[roleName];
                thisRoleName = roleName;
                return true;
            }
            return false;
        }

        public bool PlayAnimation(string animationName,bool playLoop = true)
        {
            if (animationCollectionInfo == null)
            {
                Debug.LogError("animationCollectionInfo is null");
                return false;
            }
            if (animationLocation == null && roleAnimationsID.ContainsKey(thisRoleName))
            {
                animationLocation = roleAnimationsID[thisRoleName];
            }
            if (animationName != null && animationLocation.ContainsKey(animationName))
            {
                thisAnimationName = animationName;
                Vector2Int location = animationLocation[animationName];
                atlasId = location.x;
                animationId = location.y;
                return PlayAnimation(location.x,location.y,playLoop);
            }
            return false;
        }
    
        public bool PlayAnimation(int id,int index,bool playLoop = true)
        {
            if (animationCollectionInfo == null)
            {
                Debug.LogError("animationCollectionInfo is null");
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
            
            _materialPropertyBlock.SetInt("_ColNum",_atlasInfo.colNumber);
            _materialPropertyBlock.SetInt("_RowNum",_atlasInfo.rowNumber);
            _materialPropertyBlock.SetFloat("_ColBlank",_atlasInfo.colBlank);
            _materialPropertyBlock.SetFloat("_RowBlank",_atlasInfo.rowBlank);
            _materialPropertyBlock.SetTexture("_MainTex",_atlasInfo.texture);

            if (_meshRenderer != null && _materialPropertyBlock != null)
            {
                _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
                if (_meshRenderer.sharedMaterial != null && _atlasInfo != null && animationCollectionInfo != null)
                {
                    _meshRenderer.sharedMaterial.renderQueue = _atlasInfo.id - animationCollectionInfo.firstId + _meshRenderer.sharedMaterial.renderQueue / EfficientAnimationPlayer.effectMaterialTypeStep * EfficientAnimationPlayer.effectMaterialTypeStep;
                }
            }

            _uv1.x = _atlasInfo.animationLocations[index].LocationMin % _atlasInfo.colNumber + 0.5f;
            // ReSharper disable once PossibleLossOfFraction
            _uv1.y = _atlasInfo.animationLocations[index].LocationMin / _atlasInfo.colNumber + 0.5f;
            _uv1.z = _atlasInfo.animationLocations[index].LocationMax - _atlasInfo.animationLocations[index].LocationMin;
            _uv1.w = _atlasInfo.animationLocations[index].FPS * speedScale;
            _uv2.x = playLoop ? 0 : 1;
            _uv2.y = Mathf.RoundToInt(loopOffset * _uv1.z);
            _uv2.z = Shader.GetGlobalVector("_Time").y;
            _uv2.w = onceOffset;
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
            return true;
        }

        public void SetSpeedScale(float scale)
        {
            if (animationId < 0 || animationId >= _atlasInfo.animationLocations.Count)
            {
                return;
            }
            if (animationCollectionInfo == null)
            {
                Debug.LogError("animationCollectionInfo is null");
                return;
            }
            if (!thisInit)
            {
                Debug.LogError("not init");
                return;
            }
            float timeInShader = Shader.GetGlobalFloat("_Time.y");
            int fps = _atlasInfo.animationLocations[animationId].FPS;

            if (loop)
            {
                int frameNow = (int)((timeInShader * _uv1.w + _uv2.y) % (_uv1.z+1));
                int frameAfterScaleWithoutOffset = (int)((timeInShader * fps * scale) % (_uv1.z+1));
                int offset = (int)((frameNow - frameAfterScaleWithoutOffset + _uv1.z + 1) % (_uv1.z + 1));
                loopOffset = offset / _uv1.z;
                _uv2.y = loopOffset * _uv1.z;
            }
            else
            {
                int frameNow = (int)Mathf.Clamp((timeInShader - _uv2.z) * _uv1.w + _uv2.w,0,_uv1.z);
                int frameAfterScaleWithoutOffset = (int)Mathf.Clamp((timeInShader - _uv2.z) * fps * scale,0,_uv1.z);
                onceOffset = (int)((frameNow - frameAfterScaleWithoutOffset + _uv1.z + 1) % (_uv1.z + 1));
                _uv2.w = onceOffset;
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
                Debug.LogError("animationCollectionInfo is null");
                return;
            }
            if (!thisInit)
            {
                Debug.LogError("not init");
                return;
            }
            SetSpeedScale(0.0f);
        }

        public void Resume()
        {
            if (animationCollectionInfo == null)
            {
                Debug.LogError("animationCollectionInfo is null");
                return;
            }
            if (!thisInit)
            {
                Debug.LogError("not init");
                return;
            }
            SetSpeedScale(speedScale);
        }

        public void SetColorTint(Color tint,float strength = 1)
        {
            if (animationCollectionInfo == null)
            {
                Debug.LogError("animationCollectionInfo is null");
                return;
            }
            if (!thisInit)
            {
                Debug.LogError("not init");
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
                Debug.LogError("animationCollectionInfo is null");
                return;
            }
            if (!thisInit)
            {
                Debug.LogError("not init");
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

