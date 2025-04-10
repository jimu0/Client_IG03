Shader "Custom/HalfLambert"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _RampTex ("Ramp Texture", 2D) = "white" {}
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            //Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_RampTex);
            SAMPLER(sampler_RampTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _ShadowStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.viewDirWS = normalize(_WorldSpaceCameraPos - output.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 获取主光源
                Light mainLight = GetMainLight();
                
                // 计算法线和光照方向的点积
                float NdotL = dot(input.normalWS, mainLight.direction);
                
                // 计算半兰伯特光照
                float halfLambert = (NdotL * 0.5 + 0.5);
                halfLambert = pow(halfLambert, 2.0); // 增加对比度
                
                // 计算阴影衰减
                float shadowAttenuation = mainLight.shadowAttenuation;
                
                // 使用渐变纹理
                float2 rampUV = float2(halfLambert, 0.5);
                half3 rampColor = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, rampUV).rgb;
                
                // 采样主纹理
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // 计算环境光
                half3 ambient = SampleSH(input.normalWS);
                
                // 组合颜色
                half3 diffuse = col.rgb * _Color.rgb * rampColor * mainLight.color;
                half3 finalColor = lerp(ambient * col.rgb, diffuse, halfLambert * shadowAttenuation);
                
                // 应用阴影强度
                finalColor = lerp(finalColor, finalColor * _ShadowStrength, 1 - shadowAttenuation);
                
                return half4(finalColor, col.a);
            }
            ENDHLSL
        }
    }
} 