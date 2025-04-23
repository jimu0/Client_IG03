using UnityEngine;
using UnityEngine.Rendering;

namespace Tools.StaticLightBakingTool.Light
{
    public class AreaLightMonoBehaviour : StaticLightingLayoutClass
    {
        public Color color = new(0.9f, 0.65f, 0.29f);
        public float intensity;
        private Vector3 areaScale = new(4f, 4f, 1f);//面积光比例
        private MeshFilter areaMeshF;
        private Mesh areaMesh;
        private MeshRenderer areaMeshRenderer;
        private Material areaMat;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            GetComponents();
            Modeling();
            SetComponentHideFlags();
        }

        private void Start()
        {
            SetAreaLight();
            SetComponentHideFlags();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                SetAreaLight();
            
            }
        }
    
        /// <summary>
        /// 获取组件
        /// </summary>
        private void GetComponents()
        {
            areaMeshF = gameObject.GetComponent<MeshFilter>();
            areaMeshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (!areaMeshF) areaMeshF = gameObject.AddComponent<MeshFilter>();
            if (!areaMeshRenderer) areaMeshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
    
        /// <summary>
        /// 构建
        /// </summary>
        private void Modeling()
        {
        
            if(!areaMesh) areaMesh = CreatePlane("areaPlane", 1, 1, 1, 1);
            areaMeshF.mesh = areaMesh;
            if(!areaMat) areaMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            areaMat.name = "staticAmbientLightMat";
            areaMat.name = "staticAmbientLightMat";
            areaMat.EnableKeyword("_EMISSION");
            areaMat.SetColor(BaseColor, Color.black);
            areaMat.SetColor(EmissionColor, Color.white * Mathf.Pow(2f, 0f));
            areaMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            areaMeshRenderer.material = areaMat;
            areaMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            areaMeshRenderer.receiveGI = ReceiveGI.LightProbes;
        }
    
        /// <summary>
        /// 设置面积光属性
        /// </summary>
        private void SetAreaLight()
        {
            areaScale = transform.localScale;
            emissionColor = SetEmissionColor(color, intensity);
            if (areaMat) areaMat.SetColor(EmissionColor, emissionColor);
        }
    
        /// <summary>
        /// 隐藏Inspector面板Component组件
        /// </summary>
        private void SetComponentHideFlags()
        {
            areaMeshF.hideFlags = HideFlags.HideInInspector;
            areaMeshRenderer.hideFlags = HideFlags.HideInInspector;
            if (areaMat) areaMat.hideFlags = HideFlags.HideInInspector;
        }
    
        /// <summary>
        /// 绘图
        /// </summary>
        private void OnDrawGizmos()
        {
            DrawAreaFrame();
            DrawLightRay(emissionColor, areaScale.x * areaScale.y);
        }
    
        /// <summary>
        /// 画面积光边框
        /// </summary>
        private void DrawAreaFrame()
        {
            Color gizmosColor = Gizmos.color;
            Gizmos.color = emissionColor;
            Transform transform1 = gameObject.transform;
            Vector3 transformRight = transform1.right;
            Vector3 transformUp = transform1.up;
            Vector3 areaCenter = transform.TransformPoint(Vector3.zero);
            Vector3 pos1 = areaCenter - transformRight * areaScale.x/2f - transformUp * areaScale.y/2f;
            Vector3 pos2 = pos1 + transformUp * areaScale.y;
            Gizmos.DrawLine(pos1, pos2);
            pos1 = pos2 + transformRight * areaScale.x;
            Gizmos.DrawLine(pos1, pos2);
            pos2 = pos1 - transformUp * areaScale.y;
            Gizmos.DrawLine(pos1, pos2);
            pos1 = pos2 - transformRight * areaScale.x;
            Gizmos.DrawLine(pos1, pos2);
            Gizmos.color = gizmosColor;
        }
    
    }
}
