Shader "UI/RadialRevealBlur"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _BlurTex ("Blur Tex", 2D) = "white" {}
        _Progress ("Progress", Range(0,1)) = 0.0
        _Softness ("Softness", Range(0,0.5)) = 0.08
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _BlurTex;
            float _Progress;
            float _Softness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 center = float2(0.5, 0.5);
                float2 toCenter = uv - center;
                float dist = length(toCenter);

                // normalize by max possible distance from center to corner
                const float maxDist = 0.70710678; // sqrt(2)/2
                float nd = dist / maxDist;

                // progress controls how far blur has moved inward from edges (0: none, 1: full)
                float threshold = lerp(1.0, 0.0, saturate(_Progress));

                // smooth edge between blurred/unblurred
                float edge = smoothstep(threshold - _Softness, threshold + _Softness, nd);

                fixed4 cMain = tex2D(_MainTex, uv);
                fixed4 cBlur = tex2D(_BlurTex, uv);

                fixed4 outc = lerp(cMain, cBlur, edge);
                // Preserve alpha from main texture so UI layout behaves
                outc.a = lerp(cMain.a, cBlur.a, edge);
                return outc;
            }
            ENDCG
        }
    }
}
