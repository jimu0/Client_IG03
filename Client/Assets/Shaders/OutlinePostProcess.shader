Shader "Custom/OutlinePostProcess"
{
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

    TEXTURE2D_X(_MainTex);
    SAMPLER(sampler_MainTex);

    // 统一参数
    float4 _Color;
    float _Scale;
    float _DepthThreshold;
    float _NormalThreshold;
    float _DepthNormalThreshold;
    float _DepthNormalThresholdScale;
    float4x4 _ClipToView;

    struct Attributes
    {
        uint vertexID : SV_VertexID;
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
        float3 viewSpaceDir : TEXCOORD1;
    };

    // 顶点着色器
    Varyings Vert(Attributes input)
    {
        Varyings output;
        
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
        
        // 重建视图空间坐标
        float3 clipSpace = float3((output.uv * 2.0 - 1.0), 1.0);
        float4 viewSpace = mul(_ClipToView, float4(clipSpace, 1.0));
        output.viewSpaceDir = viewSpace.xyz / viewSpace.w;
        
        return output;
    }

    // 边缘检测核心函数
    float4 Frag(Varyings input) : SV_Target
    {
        // 采样参数
        float2 uv = input.uv;
        float halfScaleFloor = floor(_Scale * 0.5);
        float halfScaleCeil = ceil(_Scale * 0.5);
        float2 texelSize = _ScreenParams.zw - 1.0;

        // 深度检测
        float depth0 = SampleSceneDepth(uv);
        float3 normal0 = SampleSceneNormals(uv);

        // 初始化差异值
        float depthDiff = 0;
        float3 normalDiff = 0;

        // 采样周围像素
        [unroll(4)]
        for(float x = -halfScaleFloor; x <= halfScaleCeil; x++)
        {
            [unroll(4)]
            for(float y = -halfScaleFloor; y <= halfScaleCeil; y++)
            {
                float2 offset = float2(x, y) * texelSize;
                float2 sampleUV = uv + offset;

                float depth1 = SampleSceneDepth(sampleUV);
                float3 normal1 = SampleSceneNormals(sampleUV);

                depthDiff += abs(depth0 - depth1);
                normalDiff += abs(dot(normal0, normal1));
            }
        }

        // 计算平均值
        depthDiff /= (_Scale * _Scale);
        normalDiff /= (_Scale * _Scale);

        // 边缘强度计算
        float depthEdge = depthDiff * _DepthThreshold * 1000;
        depthEdge = saturate(depthEdge);
        
        float normalEdge = (1 - normalDiff) * _NormalThreshold * 100;
        normalEdge = saturate(normalEdge);

        // 组合边缘检测
        float edge = max(depthEdge, normalEdge);
        
        // 深度法线混合检测
        float depthNormalEdge = depthEdge * normalEdge * _DepthNormalThreshold;
        edge = saturate(edge + depthNormalEdge * _DepthNormalThresholdScale);

        // 最终颜色混合
        float4 color = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv);
        return lerp(color, _Color, edge);
    }
    ENDHLSL

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "OutlinePostProcess"
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}