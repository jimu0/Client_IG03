Shader "StarUnion/Efficient Animation Player Base"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}
        [HideInInspector]_ColNum("Col Number", Int) = 16
    	[HideInInspector]_RowNum("Row Number", Int) = 16
        [HideInInspector]_ColBlank("Col Blank", float) = 0.0
        [HideInInspector]_RowBlank("Row Blank", float) = 0.0
        _CutOff("Cut Off", Range(0.0,1.0)) = 0.1
        [Enum(Off, 0, On, 1)]_ZWriteMode("ZWrite", float) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode("ZTest", Float) = 4
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100
        
        Cull Off
        ZWrite [_ZWriteMode]
        ZTest [_ZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color  : COLOR;
                float2 uv : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float4 uv3 : TEXCOORD3;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color  : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            int _ColNum;
			int _RowNum;
            float _ColBlank;
            float _RowBlank;
			float _CutOff;

            v2f vert (appdata v)
            {
                v2f o;
                o.color = v.color;
                o.vertex = UnityObjectToClipPos(v.vertex);
                int Frame1 = fmod(_Time.y * v.uv1.w + v.uv2.y,v.uv1.z+1);
				int Frame2 = clamp((_Time.y - v.uv2.z) * v.uv1.w + v.uv2.w,0,v.uv1.z);
				int Frame = lerp(Frame1,Frame2,step(0.5,v.uv2.x));
				int ColIndex = floor(v.uv1.x)  + Frame;
				int RowIndex = floor(v.uv1.y);
				RowIndex = RowIndex + floor(ColIndex / (float)_ColNum);
				ColIndex = fmod(ColIndex,_ColNum);
                v.uv.x = clamp(v.uv.x - _ColBlank,0,1);
                v.uv.y = clamp(v.uv.y - _RowBlank,0,1);
				o.uv.x = v.uv.x / _ColNum + ColIndex * (1.0 - _ColBlank) / _ColNum;
				o.uv.y = v.uv.y / _RowNum + RowIndex * (1.0 - _RowBlank) / _RowNum;
                o.uv.xy = o.uv.xy * v.uv3.zw + v.uv3.xy;
               
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float clipValue = col.a - _CutOff;
                col = col * i.color;
                clip(clipValue);
                return col;
            }
            ENDCG
        }
    }
}
