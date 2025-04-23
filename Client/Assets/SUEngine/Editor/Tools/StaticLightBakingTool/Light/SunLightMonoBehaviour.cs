using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tools.StaticLightBakingTool.Light
{
    public class SunLightMonoBehaviour : StaticLightingLayoutClass
    {
        public enum ControlMode { Auto, Custom }
        public enum LightMode { Unity, Custom, All }
        public ControlMode controlMode = ControlMode.Custom;//控制模式
        private ControlMode prevControlMode;
        public LightMode lightMode = LightMode.Custom;//灯光模式
        private LightMode prevLightMode;
        
        [Range(0, 1)] public float time = 0.4f;
        [HideInInspector] public string timepiece = "00,00,00";
        
        [Range(0, 360)] public float azimuthAngle = 60;//方位角
        [Range(-180, 180)] public float altitudeAngle = 45;//海拔角
        [Range(0, 360)] public float axiaTiltAngle;//轴倾角
        public float radius = 10;//̫主光源半径
        public float distance = 100;//主光源距离
        public Vector3 center = Vector3.zero;
        public Color color = Color.white;
        public float intensity = 4;
        private float sunArea;//主光源单边面积
        private Vector3 sunVector;//̫主光源位置
        private Vector3 sunScale = new(10, 10, 10);//主光源比例
        private MeshFilter sunMeshF;
        private Mesh sunMesh;
        private Material sunMat;
        [HideInInspector] public MeshRenderer sunMeshRenderer;
        [HideInInspector] public UnityEngine.Light unityLight;
        [HideInInspector] public Color ambientColor;//自动模式ambientColor获取
        private UniversalAdditionalLightData unityLightData;
        public bool monitorLightEnabled;//监听Light组件enabled开关状态״̬
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            GetComponents();
            Modeling();
            SetComponentHideFlags();
            SetLightMode(controlMode,lightMode);
        }

        private void Start()
        {
            SetSunLight();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                SetSunLight();
            }
        }
        
        /// <summary>
        /// 绘图
        /// </summary>
        private void OnDrawGizmos()
        {
            DrawSunTrack();
            DrawLightRay(emissionColor, sunArea, center);
        }
        
        /// <summary>
        /// 设置主光源属性
        /// </summary>
        public void SetSunLight()
        {
            Transform transform1 = gameObject.transform;
            transform1.position = GetPosOfSunOrbit(azimuthAngle, altitudeAngle, axiaTiltAngle, distance);
            transform1.LookAt(center);

            sunScale.x = radius;
            sunScale.y = radius;
            sunScale.z = radius;
            transform1.localScale = sunScale;
            sunArea = 2f * Mathf.PI * radius * radius;
            
            emissionColor = SetEmissionColor(color, intensity);
            if(sunMat) sunMat.SetColor(EmissionColor, emissionColor);
        }
        
        /// <summary>
        /// 获取光源组件
        /// </summary>
        private void GetComponents()
        {
            unityLight = gameObject.GetComponent<UnityEngine.Light>();
            unityLightData = gameObject.GetComponent<UniversalAdditionalLightData>();
            sunMeshF = gameObject.GetComponent<MeshFilter>();
            sunMeshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (!unityLight) unityLight = gameObject.AddComponent<UnityEngine.Light>();
            if (!unityLightData) unityLightData = gameObject.AddComponent<UniversalAdditionalLightData>();
            if (!sunMeshF) sunMeshF = gameObject.AddComponent<MeshFilter>();
            if (!sunMeshRenderer) sunMeshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        
        /// <summary>
        /// 构建主光源
        /// </summary>
        private void Modeling()
        {
            unityLight.enabled = false;
            unityLight.lightmapBakeType = LightmapBakeType.Baked;
            unityLight.type = LightType.Directional;
            unityLight.useColorTemperature = true;
            unityLight.colorTemperature = 5000f;
            
            if(!sunMesh) sunMesh = CreateSphere("sunSphere", 16, 8, 1, true);
            sunMeshF.sharedMesh = sunMesh;
            if(!sunMat) sunMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            sunMat.name = "staticSunlightMat";
            sunMat.EnableKeyword("_EMISSION");
            sunMat.SetColor(BaseColor, Color.black);
            sunMat.SetColor(EmissionColor, Color.white * Mathf.Pow(2f, 5f));
            sunMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            sunMeshRenderer.material = sunMat;
            sunMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            sunMeshRenderer.receiveGI = ReceiveGI.LightProbes;
        }

        /// <summary>
        /// 设置灯光模式
        /// </summary>
        /// <param name="cm">编辑模式</param>
        /// <param name="lm">灯光模式</param>
        public void SetLightMode(ControlMode cm,LightMode lm)
        {
            if (prevControlMode == cm && prevLightMode == lm) return;
            switch (lm)
            {
                case LightMode.Unity:
                    sunMeshRenderer.enabled = false;
                    unityLight.enabled = true;
                    monitorLightEnabled = true;
                    unityLight.hideFlags = HideFlags.None;
                    break;
                case LightMode.Custom:
                    sunMeshRenderer.enabled = true;
                    unityLight.enabled = false;
                    monitorLightEnabled = false;
                    unityLight.hideFlags = HideFlags.HideInInspector;
                    break;
                case LightMode.All:
                    sunMeshRenderer.enabled = true;
                    unityLight.enabled = true;
                    monitorLightEnabled = true;
                    unityLight.hideFlags = HideFlags.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lm), lm, null);
            }
            if (cm == ControlMode.Auto)
            {
                unityLight.hideFlags = HideFlags.NotEditable;
            }
            prevControlMode = cm;
            prevLightMode = lm;

        }
        
        /// <summary>
        /// 隐藏Inspector面板Component组件
        /// </summary>
        private void SetComponentHideFlags()
        {
            transform.hideFlags = HideFlags.HideInInspector;
            sunMeshF.hideFlags = HideFlags.HideInInspector;
            sunMeshRenderer.hideFlags = HideFlags.HideInInspector;
            if (sunMat) sunMat.hideFlags = HideFlags.HideInInspector;
            if (lightMode != LightMode.Unity) unityLight.hideFlags = HideFlags.HideInInspector;
            unityLightData.hideFlags = HideFlags.HideInInspector;
        }
        
        /// <summary>
        /// 自动模式基本参数配置
        /// </summary>
        /// <param name="d">时间d</param>
        public void AutoLightingLayout(float d)
        {
            var a = 360 * d + 270;
            azimuthAngle = a >= 360 ? a - 360 : a;
            altitudeAngle = 20;
            //axiaTiltAngle = 0;
            distance = 200;
            radius = 15;
            SetSunLightColor(d);
        }
        
        /// <summary>
        /// 自动模式昼夜参数配置
        /// </summary>
        /// <param name="d">ʱ时间d</param>
        private void SetSunLightColor(float d)
        {
            Color moonlightColor = new Color(0.2f, 0.32f, 0.46f);
            //Color cg1 = SetColor(0, 0, 0, 1); 
            Color cg2 = SetColor(0.5f, 0.55f, 0.6f, 1);
            Color cg3 = SetColor(0.75f, 0.62f, 0.42f, 1);
            Color cg3S = SetColor(0.62f, 0.65f, 0.68f, 1);
            Color cg4 = SetColor(0.77f, 0.72f, 0.7f, 1);
            Color cg5 = SetColor(0.9f, 0.87f, 0.76f, 1);
            Color c = d switch
            {
                < 0.15f => Color.Lerp(cg2, cg2, d * 8),
                < 0.25f => Color.Lerp(cg2, cg3, (d - 0.15f) * 8),
                < 0.35f => Color.Lerp(cg3, cg5, (d - 0.25f) * 8),
                < 0.5f => Color.Lerp(cg5, cg5, (d - 0.35f) * 8),
                < 0.65f => Color.Lerp(cg5, cg4, (d - 0.5f) * 8),
                < 0.75f => Color.Lerp(cg4, cg3, (d - 0.65f) * 8),
                < 0.85f => Color.Lerp(cg3, cg2, (d - 0.75f) * 8),
                _ => Color.Lerp(cg2, cg2, (d - 0.85f) * 8)
            };
            float i = d switch
            {
                < 0.15f => Mathf.Lerp(2, 2, d * 8),
                < 0.25f => Mathf.Lerp(2, 3.9f, (d - 0.15f) * 8),
                < 0.35f => Mathf.Lerp(3.9f, 3.7f, (d - 0.25f) * 8),
                < 0.5f => Mathf.Lerp(3.7f, 3.7f, (d - 0.35f) * 8),
                < 0.65f => Mathf.Lerp(3.7f, 3.7f, (d - 0.5f) * 8),
                < 0.75f => Mathf.Lerp(3.7f, 3.9f, (d - 0.65f) * 8),
                < 0.85f => Mathf.Lerp(3.9f, 2, (d - 0.75f) * 8),
                _ => Mathf.Lerp(2, 2, (d - 0.85f) * 8)
            };
            Color e = d switch
            {
                < 0.15f => Color.Lerp(cg2, cg3, d * 8),
                < 0.25f => Color.Lerp(cg3, cg3S, (d - 0.15f) * 8),
                < 0.35f => Color.Lerp(cg3S, cg3S, (d - 0.25f) * 8),
                < 0.5f => Color.Lerp(cg3S, cg3S, (d - 0.35f) * 8),
                < 0.65f => Color.Lerp(cg3S, cg3S, (d - 0.5f) * 8),
                < 0.75f => Color.Lerp(cg3S, cg3S, (d - 0.65f) * 8),
                < 0.85f => Color.Lerp(cg3S, cg3, (d - 0.75f) * 8),
                _ => Color.Lerp(cg3, cg2, (d - 0.85f) * 8)
            };
            color = c;
            intensity = i;
            emissionColor = SetEmissionColor(c, i);
            unityLight.color = c;
            unityLight.intensity = i / 4;
            ambientColor = e *((moonlightColor+c)*i*0.16f);
            ambientColor.a = 1;
        }

        /// <summary>
        /// 获取太阳轨道位置只
        /// </summary>
        /// <param name="azAngle">方位角</param>
        /// <param name="alAngle">海拔角</param>
        /// <param name="axAngle">轴倾角</param>
        /// <param name="d">距离</param>
        /// <returns>返回基于centre的世界位置</returns>
        private Vector3 GetPosOfSunOrbit(float azAngle, float alAngle,float axAngle, float d)
        {
            
            var az = (90-azAngle) * Mathf.Deg2Rad;
            var al = (alAngle) * Mathf.Deg2Rad;
            //float ax = (axAngle) * Mathf.Deg2Rad;
            sunVector.x = d * Mathf.Sin(az);
            sunVector.y = d * Mathf.Cos(az) * Mathf.Cos(al);
            sunVector.z = d * Mathf.Cos(az) * Mathf.Sin(al);
            Quaternion pointRotation = Quaternion.Euler(0,axAngle ,0);
            Vector3 pos = center + pointRotation * sunVector;
            return pos;
        }
        
        /// <summary>
        /// 绘制主光源轨迹线
        /// </summary>
        private void DrawSunTrack()
        {
            Color gizmosColor = Gizmos.color;
            Vector3 point = GetPosOfSunOrbit(0, altitudeAngle, axiaTiltAngle, distance);
            for (var i = 0; i < 360; i++)
            {
                Gizmos.color = point.y>0 ? Color.white : Color.gray;
                var point2 = GetPosOfSunOrbit(i+1, altitudeAngle, axiaTiltAngle, distance);
                Gizmos.DrawLine(point, point2);
                point = point2;
                
            }
            Gizmos.color = gizmosColor;
        }

        /// <summary>
        /// 转换时间码
        /// </summary>
        /// <param name="i">1天百分比(0-1)</param>
        /// <returns>返回string(时,分,秒)</returns>
        public string beOnTime(float i)
        {
            // 将 0-1 范围内的变量转换为小时，分钟，秒钟
            var hours = Mathf.FloorToInt(i * 24f);
            var minutes = Mathf.FloorToInt((i * 24f - hours) * 60f);
            var seconds = Mathf.FloorToInt((((i * 24f - hours) * 60f) - minutes) * 60f);
            return $"{hours:00},{minutes:00},{seconds:00}";
            
        }
        
    }
}



