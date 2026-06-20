Shader "Custom/WarningOutside"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _PlayerScreenPos ("Player Screen Position", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Vision Radius", Float) = 0.3
        _EdgeSoftness ("Edge Softness", Float) = 0.02
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
                float4 pos       : SV_POSITION;
                fixed4 color     : COLOR;
                float2 uv        : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            fixed4    _Color;
            float4    _PlayerScreenPos;
            float     _Radius;
            float     _EdgeSoftness;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos       = UnityObjectToClipPos(v.vertex);
                o.uv        = TRANSFORM_TEX(v.uv, _MainTex);
                o.color     = v.color * _Color;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 이 픽셀의 실제 스크린 UV (0~1)
                float2 screenUV  = i.screenPos.xy / i.screenPos.w;
                float2 playerPos = _PlayerScreenPos.xy;

                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 diff  = (screenUV - playerPos) * float2(aspect, 1.0);
                float dist   = length(diff);

                // 시야 안 → 0 (투명), 시야 밖 → 1 (보임)
                float outsideMask = smoothstep(_Radius - _EdgeSoftness, _Radius + _EdgeSoftness, dist);

                fixed4 texColor = tex2D(_MainTex, i.uv) * i.color;
                return fixed4(texColor.rgb, texColor.a * outsideMask);
            }
            ENDCG
        }
    }
}
