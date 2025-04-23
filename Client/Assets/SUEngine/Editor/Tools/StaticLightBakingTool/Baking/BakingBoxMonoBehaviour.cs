using System;
using System.Collections.Generic;
using System.Linq;
using Tools.StaticLightBakingTool.Forging;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tools.StaticLightBakingTool.Baking
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class BakingBoxMonoBehaviour : MonoBehaviour
    {
        private Bounds bounds; //包围盒数据
        private Color lineColor = Color.gray; //包围盒边框颜色
        public bool realTimeMonitoring; //实时跟新？
        [Range(1, 20)] public int some = 1; //生成张数
        [Range(-1, 1)] public float lightmapAllowance; //偏差变量
        public string savePath = "";
        public Shader defaultShader;
        public string defaultShaderPath = "Universal Render Pipeline/Lit";
        [Serializable] public struct SpecialRes
        {
            public MeshRenderer res;
            [Range(0, 2)] public float value;
        }

        [SerializeField] public SpecialRes[] specialRes = Array.Empty<SpecialRes>();
        [SerializeField] private int[] specialResID = Array.Empty<int>();
        [Serializable] public struct TextureBrightness
        {
            public string texName;
            public Color color;
            [Range(0, 1)] public float value;
        }

        [SerializeField] public List<TextureBrightness> textureBrightness = new();
        private bool isMoving;
        private float timer;
        private const float MoveDuration = 1f; // 移动持续时间
        private float movingPosX;
        public List<GameObject> cGameObjects;
        //public StaticUVUnwrapPackSys.BaseClass parent = StaticUVUnwrapPackSys.BaseClass.NoParent;
        [Range(1, 13)] public int textureSizePower = 7;//资源合批大小
        public int textureSizeResult;
        
        public void Awake()
        {
            RetrieveSpecialRes();
        }

        private void Start()
        {
            SetBounds();
            SetDefaultShader();
        }

        public void Update()
        {
            SetBounds();
            SavePath(savePath);
            ShaderPath(defaultShaderPath,defaultShader);
            MovingUpdate();
        }

        private void MovingUpdate()
        {
            if (!isMoving) return;
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / MoveDuration);
            movingPosX = Mathf.Lerp(bounds.center.x, bounds.center.x + bounds.size.x, t)+4;
            Vector3 pos = new() { x = movingPosX };
            if (cGameObjects != null)
            {
                foreach (GameObject obj in cGameObjects.Where(obj => obj))
                {
                    Transform objTransform = obj.transform;
                    pos.y = objTransform.position.y;
                    obj.transform.SetPositionAndRotation(pos, objTransform.rotation);
                }
            }
            else { isMoving = false; }
            if (timer >= MoveDuration) isMoving = false;
        }

        /// <summary>
        /// 绘图
        /// </summary>
        private void OnDrawGizmos()
        {
            DrawAreaFrame(lineColor);
        }

        /// <summary>
        /// 画烘培盒边框
        /// </summary>
        /// <param name="color">颜色</param>
        private void DrawAreaFrame(Color color)
        {
            Color gizmosColor = Gizmos.color;
            Gizmos.color = color;
            Vector3 lossyScale = transform.lossyScale;
            Gizmos.DrawWireCube(bounds.center, lossyScale);
            Vector3 p = bounds.center;
            p.y -= (lossyScale.y / 2);
            Vector3 s = lossyScale;
            s.y = 0.01f;
            color.a = 0.1f;
            Gizmos.color = color;
            Gizmos.DrawCube(p, s);
            color.a = 1;
            Gizmos.color = gizmosColor;
        }

        /// <summary>
        /// 设置包围盒
        /// </summary>
        private void SetBounds()
        {
            Transform transform1 = transform;
            bounds.center = transform1.position;
            Vector3 boundsSize;
            Vector3 lossyScale = transform1.lossyScale;
            boundsSize.x = Mathf.Abs(lossyScale.x);
            boundsSize.y = Mathf.Abs(lossyScale.y);
            boundsSize.z = Mathf.Abs(lossyScale.z);
            bounds.size = boundsSize;
        }

        /// <summary>
        /// 设置烘培资源属性scaleInLightmap
        /// </summary>
        /// <param name="some1">张数</param>
        /// <param name="allowance">偏差变量</param>
        public void SetBakingObjsProperty(int some1, float allowance)
        {
            BakingBoxMonoBehaviour[] bakingBoxes = FindObjectsOfType<BakingBoxMonoBehaviour>();
            foreach (BakingBoxMonoBehaviour bakingBox in bakingBoxes)
            {
                bakingBox.lineColor = bakingBox == this ? Color.green : Color.gray;
            }

            StaticBakingSys.Instance.SpecialRenderers.Clear();
            foreach (SpecialRes res in specialRes)
            {
                if (res.res && !StaticBakingSys.Instance.SpecialRenderers.ContainsKey(res.res))
                    StaticBakingSys.Instance.SpecialRenderers.Add(res.res, res.value);
            }

            MeshRenderer[] mrs = FindObjectsOfType<MeshRenderer>();
            
            StaticBakingSys.Instance.SetBakingObjsProperty(bounds, mrs, allowance, some1);
        }
        

        /// <summary>
        /// 设置排除的资源属性
        /// </summary>
        /// <param name="length">数组长度</param>
        /// <param name="i">当前id</param>
        /// <param name="res">属性</param>
        public void SetSpecialRe(int length, int i, SpecialRes res)
        {
            if (i <= 0)
            {
                Array.Resize(ref specialRes, 0);
                Array.Resize(ref specialResID, 0);
                return;
            }

            if (specialRes.Length != length)
            {
                Array.Resize(ref specialRes, length);
            }

            if (specialResID.Length != specialRes.Length)
            {
                Array.Resize(ref specialResID, length);
            }

            specialRes[i] = res;
            specialResID[i] = res.res ? res.res.GetInstanceID() : 0;
        }

        /// <summary>
        /// 重拾specialRes资源
        /// </summary>
        private void RetrieveSpecialRes()
        {
            if (specialRes.Length == 0) return;
            MeshRenderer[] mrs = FindObjectsOfType<MeshRenderer>();
            for (int i = 0; i < specialRes.Length; i++)
            {
                if (specialResID[i] == 0)
                {
                    specialRes[i].res = null;
                }
                else
                {
                    foreach (MeshRenderer mr in mrs)
                    {
                        if (specialResID[i] != mr.GetInstanceID()) continue;
                        specialRes[i].res = mr;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 产出(位移)新生资源
        /// </summary>
        public void CSpawnAni()
        {
            cGameObjects = StaticUVUnwrapPackSys.Instance.CGameObjects;
            timer = 0f;
            isMoving = true;
        }

        /// <summary>
        /// 设置纹理校色面板数据
        /// </summary>
        public void SetTexturesCorrectionPanel()
        {
            List<Texture2D> texturesTex = StaticUVUnwrapPackSys.Instance.CloneTextures;
            List<float> texturesValue = StaticUVUnwrapPackSys.Instance.TexturesValue;
            textureBrightness.Clear();
            TextureBrightness tb = new();
            for (int i = 0; i < texturesTex.Count; i++)
            {
                tb.texName = texturesTex[i] != null ? texturesTex[i].name : "null";
                tb.value = texturesValue[i];
                Color color = Color.white * tb.value;
                color.a = 1;
                tb.color = color;
                textureBrightness.Add(tb);
            }
        }
        
        /// <summary>
        /// 跟新纹理校色面板
        /// </summary>
        /// <param name="texBrightColor">颜色</param>
        /// <param name="texBrightValue">明度</param>
        public void UpdateTexturesCorrectionPanel(List<Color> texBrightColor, List<float> texBrightValue)
        {
            texBrightColor.Clear();
            texBrightValue.Clear();
            if (textureBrightness is not { Count: > 0 } || texBrightColor.Count == textureBrightness.Count) return;
            for (int i = 0; i < textureBrightness.Count; i++)
            {
                texBrightColor.Add(textureBrightness[i].color);
                texBrightValue.Add(textureBrightness[i].value);
            }
        }

        /// <summary>
        /// 获取纹理校色的纹理属性
        /// </summary>
        /// <param name="m">批次</param>
        /// <param name="c">颜色值</param>
        /// <param name="v">灰度值</param>
        public void GetTextureBrightness(int m, Color c, float v)
        {
            StaticUVUnwrapPackSys.Instance.SetColorOfTexture(m, c, v);
        }
        
        /// <summary>
        /// 删除新生资源
        /// </summary>
        public void RemoveCGameObjects()
        {
            StaticUVUnwrapPackSys.Instance.RemoveCGameObjects();
            textureBrightness.Clear();
        }
        
        /// <summary>
        /// 保存纹理组
        /// </summary>
        /// <param name="path">路径</param>
        public void SaveTextures(string path)
        {
            StaticUVUnwrapPackSys.Instance.SaveTextures(path);
        }
        /// <summary>
        /// 判断烘培资源是否为空
        /// </summary>
        /// <returns></returns>
        public bool CheckResourcesNotEmpty()
        {
            return StaticUVUnwrapPackSys.Instance.CheckResourcesNotEmpty();
        }

        public void CompleteFusion()
        {
            StaticUVUnwrapPackSys.Instance.CompleteFusion();
        }

        public void TexturesColourCorrection()
        {
            StaticUVUnwrapPackSys.Instance.TexturesColourCorrection();
        }

        public void BatchOfObjects()
        {
            StaticUVUnwrapPackSys.Instance.BatchOfObjects(textureSizeResult);
        }

        public void SavePath(string path)
        {
            StaticUVUnwrapPackSys.Instance.SavePath = path;
        }
        
        public void ShaderPath(string path, Shader shader)
        {
            StaticUVUnwrapPackSys.Instance.ShaderPath = path;
            StaticUVUnwrapPackSys.Instance.DefaultShader = shader;
        }

        public void SetDefaultShader()
        {
            string path;
            if (GraphicsSettings.renderPipelineAsset != null && GraphicsSettings.renderPipelineAsset.GetType().Name.Contains("UniversalRenderPipelineAsset"))
            {
                path = "Universal Render Pipeline/Lit";
            }
            else if (GraphicsSettings.renderPipelineAsset != null && GraphicsSettings.renderPipelineAsset.GetType().Name.Contains("HDRenderPipelineAsset"))
            {
                path = "HDRenderPipeline/Lit";
            }
            else
            {
                path = "Standard";
            }
            defaultShader = Shader.Find(path);
            defaultShaderPath = path;
        }

    }
}
