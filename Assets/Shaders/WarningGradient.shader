Shader "Custom/WarningGradient"
{
    Properties
    {
        _MainTex         ("MainTex (UI 필수)", 2D) = "white" {}
        _ColorTex        ("Color Texture (RGB)", 2D) = "white" {}
        _AlphaTex        ("Alpha Texture (Grayscale)", 2D) = "white" {}
        _PlayerScreenPos ("Player Screen Position", Vector) = (0.5, 0.5, 0, 0)
        _Radius          ("Vision Radius", Float) = 0.3
        _EdgeSoftness    ("Edge Softness", Float) = 0.02
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
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
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 screenPos: TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _ColorTex;
            sampler2D _AlphaTex;
            float4    _ColorTex_ST;
            float4    _PlayerScreenPos;
            float     _Radius;
            float     _EdgeSoftness;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos       = UnityObjectToClipPos(v.vertex);
                o.uv        = TRANSFORM_TEX(v.uv, _ColorTex);
                o.color     = v.color;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // ① 색상 텍스처 샘플링
                fixed4 col = tex2D(_ColorTex, i.uv) * i.color;

                // ② 알파 텍스처로 투명도 결정 (흰색=불투명, 검정=투명)
                float gradAlpha = tex2D(_AlphaTex, i.uv).r;

                // ③ 시야 밖 마스크
                float2 screenUV  = i.screenPos.xy / i.screenPos.w;
                float2 playerPos = _PlayerScreenPos.xy;
                float  aspect    = _ScreenParams.x / _ScreenParams.y;
                float  dist      = length((screenUV - playerPos) * float2(aspect, 1.0));
                float  outsideMask = smoothstep(_Radius - _EdgeSoftness, _Radius + _EdgeSoftness, dist);

                // i.color.a = WarningUITracker의 FadeRoutine이 줄여주는 fade 값
                return fixed4(col.rgb, gradAlpha * outsideMask * i.color.a);
            }
            ENDCG
        }
    }
}
