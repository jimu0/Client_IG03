using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarUnion_EfficientAnimation
{
    [Serializable]
    public struct AnimationLocationInfo
    {

        public int AnimationID;
        public string RoleName;
        public string AnimationName;
        public int LocationMin;
        public int LocationMax;
        public int FPS;
        public float aspect;
        public Vector3[] vertices;
    }

    [Serializable]
    public class AtlasInfo
    {
        public int id;
        public int atlasId;
        public Texture2D texture;
        public Vector4 offsetAndScale;
        public int colNumber;
        public int rowNumber;
        public float colBlank;
        public float rowBlank;
        public List<AnimationLocationInfo> animationLocations;
    }

    public class AnimationCollectionInfo : ScriptableObject
    {
        public int firstId;
        public string collectionName;
        public List<string> roleAndAnimationNames;
        public List<Vector2Int> animationLocations;
        public List<AtlasInfo> atlasInfos;
        public EfficientAnimationShaderType initShaderType;
        public Material defaultMaterial;
        public List<EfficientAnimationShaderType> effectShaderTypes;
        public List<Material> effectMaterials;
    }
}


