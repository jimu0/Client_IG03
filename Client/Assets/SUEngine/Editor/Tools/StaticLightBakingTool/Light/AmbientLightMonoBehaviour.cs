using UnityEngine;
using UnityEngine.Rendering;

namespace Tools.StaticLightBakingTool.Light
{
    public class AmbientLightMonoBehaviour : StaticLightingLayoutClass
    { 
        public enum Mode { Color,Texture }
        public Mode mode = Mode.Color;
        public Texture2D texture;
        [Range(0, 360)] public float axiaTiltAngle;//轴倾角
        public float radius = 110;//穹顶半径
        public SunLightMonoBehaviour mySunLight;
        public bool autoCling;//紧贴着太阳

        public Color color = new(0.07f, 0.08f, 0.09f);
        public float intensity;
        private MeshFilter ambientMeshF;
        private Mesh ambientMesh;
        private Material ambientMat;
        private MeshRenderer ambientMeshRenderer;
    
        public Vector3 center = Vector3.zero;
        private Quaternion ambientRotation;
        private Vector3 ambientScale = new Vector3(110, 110, 110);
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");

        private void Awake()
        {
            GetComponents();
            Modeling();
            SetComponentHideFlags();
        }

        private void Start()
        {
            SetAmbientLight();
        }


        private void Update()
        {
            if (!Application.isPlaying)
            {
                SetAmbientLight();
            
            }
        
        }
        
        /// <summary>
        /// 获取组件
        /// </summary>
        private void GetComponents()
        {
            ambientMeshF = gameObject.GetComponent<MeshFilter>();
            ambientMeshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (!ambientMeshF) ambientMeshF = gameObject.AddComponent<MeshFilter>();
            if (!ambientMeshRenderer) ambientMeshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        
        /// <summary>
        /// 构建
        /// </summary>
        private void Modeling()
        {
            if(!ambientMesh) ambientMesh = CreateSphere("SkySphere", 32, 16, 1, false);
            ambientMeshF.mesh = ambientMesh;
            if(!ambientMat) ambientMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            ambientMat.name = "staticAmbientLightMat";
            ambientMat.EnableKeyword("_EMISSION");
            ambientMat.SetColor(BaseColor, Color.black);
            ambientMat.SetColor(EmissionColor, Color.white * Mathf.Pow(2f, 0f));
            ambientMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            ambientMeshRenderer.material = ambientMat;
            ambientMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            ambientMeshRenderer.receiveGI = ReceiveGI.LightProbes;
        }

        /// <summary>
        /// 设置环境属性
        /// </summary>
        public void SetAmbientLight()
        {
            if (mySunLight)
            {
                SetRadiusMinValue();
                if (mySunLight.controlMode == 0)
                {
                    mode = Mode.Color;
                    autoCling = true;
                    if (mySunLight)
                    {
                        color = mySunLight.ambientColor;
                        intensity = 0;
                    }
                }
            }
            ambientRotation = Quaternion.Euler(0, axiaTiltAngle, 0);
            ambientScale.x = radius;
            ambientScale.y = radius;
            ambientScale.z = radius;
            transform.localScale = ambientScale;
            transform.SetPositionAndRotation(center, ambientRotation);
            //ambientMat = ambientMeshRenderer.sharedMaterials[0];
            emissionColor = SetEmissionColor(color, intensity);
            if (ambientMat)
            {
                ambientMat.SetColor(EmissionColor, emissionColor);
                texture = mode == Mode.Texture ? texture : null;
                ambientMat.SetTexture(EmissionMap, texture);
            }
        }

        /// <summary>
        /// 隐藏Inspector面板Component组件
        /// </summary>
        private void SetComponentHideFlags()
        {
            transform.hideFlags = HideFlags.HideInInspector;
            ambientMeshF.hideFlags = HideFlags.HideInInspector;
            ambientMeshRenderer.hideFlags = HideFlags.HideInInspector;
            if (ambientMat) ambientMat.hideFlags = HideFlags.HideInInspector;
        }
    
        /// <summary>
        /// 设置自适应最小半径
        /// </summary>
        private void SetRadiusMinValue()
        {
        
            var minimumRange = Mathf.Abs(mySunLight.distance) + Mathf.Abs(mySunLight.radius);
            if (autoCling)
            {
                center = mySunLight.center;
                radius = minimumRange;
            }
            else
            {
                var distance = Vector3.Distance(center,mySunLight.center);
                radius = minimumRange + distance;
            }
        }
    
    }
}

