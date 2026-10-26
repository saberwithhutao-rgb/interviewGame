Shader "Custom/TelegraphCircle"
{
    Properties
    {
        _Color ("Circle Color", Color) = (1, 0, 0, 0.4)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // µ½Ô²ÐÄµÄ¾àÀë
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);

                // Ô²Íâ¶ªÆú
                if (dist > 0.5) discard;

                // ±ßÔµ¿¹¾â³Ý
                float alpha = smoothstep(0.5, 0.48, dist);

                return fixed4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
}