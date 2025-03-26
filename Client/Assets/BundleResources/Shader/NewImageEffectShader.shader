Shader "Custom/AtlasFlowEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _FlowTex ("Flow Texture", 2D) = "white" {}
        _FlowSpeed ("Flow Speed", Float) = 1.0
        _FlowColor ("Flow Color", Color) = (1,1,1,0.5)
        _FlowTiling ("Flow Tiling", Float) = 1.0
        _FlowIntensity ("Flow Intensity", Range(0, 1)) = 0.5
        
        // UI必需属性
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }
    
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float2 screenPos : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _FlowTex;
            fixed4 _Color;
            fixed4 _FlowColor;
            float _FlowSpeed;
            float _FlowTiling;
            float _FlowIntensity;
            float4 _ClipRect;
            float4 _MainTex_ST;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color;
                
                // 计算屏幕空间坐标
                float4 screenPos = ComputeScreenPos(OUT.vertex);
                OUT.screenPos = screenPos.xy / screenPos.w;
                
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 采样主贴图
                half4 color = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // 只在有颜色的区域显示扫光效果
                if (color.a > 0.01)
                {
                    // 计算扫光UV
                    float2 flowUV = IN.screenPos * _FlowTiling;
                    flowUV.x += _Time.y * _FlowSpeed;
                    
                    // 采样扫光贴图
                    half4 flowTex = tex2D(_FlowTex, flowUV);
                    
                    // 计算扫光效果
                    half flowEffect = flowTex.r * _FlowIntensity;
                    
                    // 混合扫光效果
                    color.rgb += _FlowColor.rgb * flowEffect * color.a;
                }
                
                // 应用UI裁剪
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
                return color;
            }
            ENDCG
        }
    }
}