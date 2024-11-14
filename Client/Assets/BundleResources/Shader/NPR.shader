Shader "Unlit/NPR"
{
    
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _OutlineWidth ("Outline Width", Range(0.01, 1)) = 0.01
        _OutLineColor ("OutLine Color", Color) = (0.5,0.5,0.5,1)
        _RampStart("交界起始RampStart", Range(0.1, 1)) = 0.3
        _RampSize("交界大小RampSize", Range(0,1)) = 0.1
        [IntRange]_RampStep("交界段数RampStep",Range(1,10)) = 10
        _RampSmooth("交界柔和RampSmooth",Range(0.01, 1)) = 0.1
        _DarkColor("暗面",Color) = (0.4,0.4,0.4,1)
        _LightColor("亮面",Color) = (0.8,0.8,0.8,1)

        _SpecPow("光泽度", Range(0,1)) = 0.1
        _SpecularColor("高光", Color) = (1.0, 1.0, 1.0, 1)
        _SpecIntensity("高光强度", Range(0,1)) = 0
        _SpecSmooth("高光柔和度", Range(0,0.5)) = 0.1

        _RimColor("边缘光颜色", Color) = (1.0, 1.0, 1.0, 1)
        _RimThreshold("边缘光阈值", Range(0,1)) = 0.45
        _RimSmooth("边缘光柔和度", Range(0, 0.5)) = 0.1
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry" 
            "LightMode" = "ExtraPass_1" 
        }
        //LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOOR2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _RampStart;
            float _RampSize;
            float _RampStep;
            float _RampSmooth;
            float3 _DarkColor;
            float3 _LightColor;

            float _SpecPow;
            float3 _SpecularColor;
            float _SpecIntensity;
            float _SpecSmooth;

            float3 _RimColor;
            float _RimThreshold;
            float _RimSmooth;


            float linearstep(float min, float max, float t)
            {
                return saturate((t - min) / (max - min));
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                //UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float3 normal = normalize(i.worldNormal);
                float3 worldLightDir = UnityWorldSpaceLightDir(i.worldPos);
                float NoL = dot(i.worldNormal, worldLightDir);
                float halfLambert = NoL * 0.5 + 0.5;
                
                //计算高光
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz); //视向量
                float3 halfDir = normalize(viewDir + worldLightDir);
                float NoH = dot(normal, halfDir);
                float blinnPhone = pow(max(0, NoH), _SpecPow * 128.0);
                //柔和化
                float3 specularColor = smoothstep(0.7 - _SpecSmooth / 2, 0.7 + _SpecSmooth / 2, blinnPhone) *
                    _SpecularColor + _SpecIntensity;
                
                //边缘光
                float NoV = dot(i.worldNormal, viewDir);
                float rim = (1 - max(0, NoV)) * NoL;
                float3 rimColor = smoothstep(_RimThreshold - _RimSmooth / 2, _RimThreshold + _RimSmooth / 2, rim) *
                    _RimColor;
                
                
                float ramp = linearstep(_RampStart, _RampStart + _RampSize, halfLambert);
                float step = ramp * _RampStep; //色阶大小为一
                float gridStep = floor(step); //当前色阶段
                float smoothStep = smoothstep(gridStep, gridStep + _RampSmooth, step) + gridStep;
                ramp = smoothStep / _RampStep;
                //得到最终的色彩
                float3 rampColor = lerp(_DarkColor, _LightColor, ramp);
                //返回一下
                rampColor *= col;
                
                //混合颜色
                float3 finalColor = saturate(rampColor + specularColor + rimColor);
                return float4(finalColor, 1);
                
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                //return col * halfLambert;

                // fixed4 col = tex2D(_MainTex, i.uv);
                // return col;
            }
            ENDCG
        }


        // 轮廓描边Pass
        Pass
        {
            Name "OutlinePass"
            Tags 
            { 
                "Queue"="Overlay" 
                "LightMode" = "ExtraPass_2" 
            }  
            // 渲染顺序确保描边在上层
            Cull Front  // 剔除前面，保留背面用于描边
            ZWrite On
            ZTest LEqual
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #include "UnityCG.cginc"

            float _OutlineWidth;
            float4 _OutLineColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vertOutline(appdata v)
            {
                v2f o;
                // 通过法线方向偏移实现描边效果
                float3 offset = v.normal * _OutlineWidth;
                o.vertex = UnityObjectToClipPos(v.vertex + float4(offset, 0));
                return o;
            }

            fixed4 fragOutline(v2f i) : SV_Target
            {
                return _OutLineColor;  // 使用自定义的轮廓颜色
            }
            ENDCG
        }
    }
}
