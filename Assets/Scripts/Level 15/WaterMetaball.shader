Shader "Custom/WaterMetaball"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        _Color ("Water Color", Color) = (0, 0.5, 1, 1)
        _OutlineColor ("Outline Color", Color) = (0, 0.2, 0.8, 1)
        _OutlineSize ("Outline Size", Range(0, 0.2)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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
                float4 color : COLOR; // Support vertex colors (UI support)
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float _Cutoff;
            float4 _Color;
            float4 _OutlineColor;
            float _OutlineSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the RenderTexture (blurred water drops)
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // Metaball Logic: Thresholding the alpha
                if (col.a < _Cutoff)
                {
                    discard; // Pixel is too light, make it completely transparent (air)
                }
                else if (col.a < _Cutoff + _OutlineSize)
                {
                    return _OutlineColor; // Pixel is on the very edge, make it outline color
                }
                
                return _Color; // Pixel is solid, make it the main water color
            }
            ENDCG
        }
    }
}
