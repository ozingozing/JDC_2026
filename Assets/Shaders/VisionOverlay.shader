Shader "Custom/VisionOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PlayerScreenPos ("Player Screen Position", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Vision Radius (viewport y)", Float) = 0.3
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
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _PlayerScreenPos;
            float  _Radius;
            float  _EdgeSoftness;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv         = i.uv;
                float2 playerPos  = _PlayerScreenPos.xy;

                // 화면 비율 보정 → 원이 타원이 되지 않도록
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 diff  = (uv - playerPos) * float2(aspect, 1.0);
                float dist   = length(diff);

                // 반지름 안쪽: 투명, 바깥쪽: 검정
                float alpha = smoothstep(_Radius - _EdgeSoftness, _Radius + _EdgeSoftness, dist);
                return fixed4(0.0, 0.0, 0.0, alpha);
            }
            ENDCG
        }
    }
}
