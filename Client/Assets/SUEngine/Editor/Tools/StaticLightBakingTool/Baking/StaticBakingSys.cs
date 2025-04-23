using System.Collections.Generic;
using System.Linq;
using Tools.StaticLightBakingTool.Light;
using UnityEditor;
using UnityEngine;

namespace Tools.StaticLightBakingTool.Baking
{
    public class StaticBakingSys
    {
        private static StaticBakingSys instance;
        public static StaticBakingSys Instance => instance ??= new StaticBakingSys();

        private readonly Dictionary<MeshRenderer, ObjRes> objDic = new();//模型数据封装

        private struct ObjRes//烘培资源属性
        {
            public MeshFilter MeshFilter;//烘培资源模型
            public Vector3 Size;//烘培资源缩放比例
            public float Area;//烘培资源面积
            public float ScaleInLightmap;//烘培资源LightmapUV占比率
        }
        public readonly Dictionary<MeshRenderer, float> SpecialRenderers = new();//单独处理的烘培资源，但也参与自动分布
        
        private float lightmapResolution;//Lighting面板LightmapResolution(1unity单位占多少像素)
        private int lightmapPadding;//Lighting面板LightmapPadding(UV之间的扩充像素)
        private int maxLightmapSize;//Lighting面板MaxLightmapSize(灯光烘培贴图的最大尺寸)
        
        /// <summary>
        /// 设置烘培物体属性
        /// </summary>
        /// <param name="bounds">包围盒</param>
        /// <param name="meshRenderers">烘培物体</param>
        /// <param name="lightmapAllowance">偏差值</param>
        /// <param name="some">张数</param>
        public void SetBakingObjsProperty(Bounds bounds,MeshRenderer[] meshRenderers,float lightmapAllowance,float some)
        {
            GetBakingObjs(bounds,meshRenderers);
            if (objDic.Count == 0) return;
            GetLightMapEditorSettings();
            float canvasSize = Mathf.Pow(maxLightmapSize / lightmapResolution, 2) * some;
            float allowance = 1 + lightmapAllowance;
            if (objDic.Count > 1)
            {
                Dictionary<MeshRenderer, ObjRes> objDicCopy = objDic.ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value); // 创建objDic的副本用于循环中设置自身值
                float sum = objDicCopy.Values.Sum(a => a.Area);
                foreach (MeshRenderer meshRenderer in objDicCopy.Keys)
                {
                    ObjRes objRes = objDicCopy[meshRenderer];
                    objRes.ScaleInLightmap = Mathf.Sqrt(canvasSize / (sum - objRes.Area)) * allowance;
                    meshRenderer.scaleInLightmap = objRes.ScaleInLightmap;
                    objDic[meshRenderer] = objRes;
                }
            }
            else
            {
                ObjRes objRes = objDic.Values.First();
                MeshRenderer meshRenderer = objDic.Keys.First();
                objRes.ScaleInLightmap = Mathf.Abs(Mathf.Sqrt(canvasSize / objRes.Area)) * allowance;
                meshRenderer.scaleInLightmap = objRes.ScaleInLightmap;
                objDic[meshRenderer] = objRes;

            }
        }
        
        /*/// <summary>
        /// 计算lightmapSetting中uv空间单位比
        /// </summary>
        /// <returns>比值，1为标准比值</returns>
        private float LightMapUVUnits()
        {
            GetLightMapEditorSettings();
            float v = maxLightmapSize / (lightmapResolution * 25.6f);
            return v;
        }*/
    
        /// <summary>
        /// 获取Lighting面板UV空间相关属性
        /// </summary>
        private void GetLightMapEditorSettings()
        {
            lightmapResolution = Lightmapping.lightingSettings.lightmapResolution;
            maxLightmapSize = Lightmapping.lightingSettings.lightmapMaxSize;
        }
    
        /// <summary>
        /// 获取烘培资源
        /// </summary>
        /// <param name="bounds">包围盒数据</param>
        /// <param name="meshRenderers">场景中所有资源</param>
        private void GetBakingObjs(Bounds bounds, MeshRenderer[] meshRenderers)
        {
            objDic.Clear();
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                if (IsLightSource(meshRenderer)) continue;
                MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                bool checkObj = CheckObj(bounds, meshFilter, meshRenderer);
                if (checkObj)
                {
                    ObjRes objRes = default;
                    objRes.MeshFilter = meshFilter;
                    objRes.Size = meshRenderer.gameObject.transform.lossyScale;
                    float area = ScanningModel(meshFilter.sharedMesh, objRes.Size);
                    if (SpecialRenderers.Count > 0 && SpecialRenderers.ContainsKey(meshRenderer))
                    {
                        float sr = SpecialRenderers[meshRenderer];
                        sr = sr > 0 ? sr : 0.00001f;
                        area *= sr;
                    }
                    float areaMax = GetBoundsArea(bounds);
                    objRes.Area = area > areaMax ? areaMax : area;//面积受限于包围盒范围
                    if (objDic.ContainsKey(meshRenderer))
                    {
                        objDic[meshRenderer] = objRes;
                    }
                    else
                    {
                        objDic.Add(meshRenderer, objRes);
                    }
                }
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(meshRenderer.gameObject);
                flags = checkObj ? flags | StaticEditorFlags.ContributeGI : flags & ~StaticEditorFlags.ContributeGI;
                GameObjectUtility.SetStaticEditorFlags(meshRenderer.gameObject, flags);

            }
        }
    
        /// <summary>
        /// 判断烘培资源的条件
        /// </summary>
        /// <param name="bounds">包围盒</param>
        /// <param name="meshFilter">MeshFilter组件</param>
        /// <param name="renderer">MeshRenderer组件</param>
        /// <returns>是否为烘培资源</returns>
        private bool CheckObj(Bounds bounds, MeshFilter meshFilter, MeshRenderer renderer)
        {
            bool b = renderer.gameObject.activeSelf &&
                     bounds.Contains(renderer.bounds.center) &&
                     renderer.enabled &&
                     meshFilter.sharedMesh;
            return b;
        }
    
        /// <summary>
        /// 判断是否为光源
        /// </summary>
        /// <param name="meshRenderer">场景中所有资源</param>
        /// <returns>是否为光源</returns>
        private bool IsLightSource(MeshRenderer meshRenderer)
        {
            bool b = meshRenderer.gameObject.GetComponent<AreaLightMonoBehaviour>() ||
                     meshRenderer.gameObject.GetComponent<AmbientLightMonoBehaviour>() ||
                     meshRenderer.gameObject.GetComponent<SunLightMonoBehaviour>();
            return b;
        }

        /// <summary>
        /// 获取模型面积
        /// </summary>
        /// <param name="mesh">模型</param>
        /// <param name="scale">lossyScale尺寸</param>
        /// <returns>模型面积</returns>
        private float ScanningModel(Mesh mesh, Vector3 scale)
        {
            float scaledArea = 0;
            int[] triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v1 = vertices[triangles[i]];
                Vector3 v2 = vertices[triangles[i + 1]];
                Vector3 v3 = vertices[triangles[i + 2]];
                Vector3 a = Vector3.Scale(v1, scale);
                Vector3 b = Vector3.Scale(v2, scale);
                Vector3 c = Vector3.Scale(v3, scale);
                Vector3 cross = Vector3.Cross(b-a, c-a);
                float area = 0.5f * cross.magnitude;
                scaledArea += area;
            }
        
            return scaledArea;
        }

        /// <summary>
        /// 获取包围盒面积
        /// </summary>
        /// <param name="bounds">包围盒数据</param>
        /// <returns>包围盒面积</returns>
        private float GetBoundsArea(Bounds bounds)
        {
            Vector3 size = bounds.size;
            float area = 2f * (size.x * size.y + size.x * size.z + size.y * size.z);
            return area;
        }
        
        public Dictionary<MeshFilter, MeshRenderer> GetObjs()
        {
            return objDic.Count == 0 ? null : objDic.ToDictionary(
                kvp => kvp.Value.MeshFilter, 
                kvp => kvp.Key);
        }
        
    }
}


