Shader "Custom/GlitchCensorUniversal"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1,1,1,1)
        
        [Header(Glitch Settings)]
        _EnableGlitch ("Enable Glitch", Float) = 1
        _PixelSize ("Pixel Size", Range(1, 100)) = 10
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.1
        _Speed ("Animation Speed", Range(0, 10)) = 1
        
        [Header(Colors)]
        _GlitchColor1 ("Glitch Color 1", Color) = (1,0,0,1)
        _GlitchColor2 ("Glitch Color 2", Color) = (0,1,0,1)
        _GlitchColor3 ("Glitch Color 3", Color) = (0,0,1,1)
        
        [Header(Blending)]
        _GlitchBlend ("Glitch Blend", Range(0, 1)) = 0.8
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            
            float _EnableGlitch;
            float _PixelSize;
            float _GlitchIntensity;
            float _Speed;
            fixed4 _GlitchColor1;
            fixed4 _GlitchColor2;
            fixed4 _GlitchColor3;
            float _GlitchBlend;
            
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898,78.233))) * 43758.5453123);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Базовая текстура
                fixed4 baseColor = tex2D(_MainTex, i.uv) * _Color;
                
                if (_EnableGlitch < 0.5)
                    return baseColor;
                
                float time = _Time.y * _Speed;
                
                // Пиксельная сетка
                float2 pixelUV = floor(i.uv * _PixelSize) / _PixelSize;
                
                // Шум для глитча
                float noise1 = random(pixelUV + time);
                float noise2 = random(pixelUV + time * 1.3);
                float noise3 = random(pixelUV + time * 0.7);
                
                // Выбор цвета глитча
                fixed4 glitchColor = _GlitchColor1;
                if (noise1 > 0.66) glitchColor = _GlitchColor2;
                else if (noise1 > 0.33) glitchColor = _GlitchColor3;
                
                // Маска для применения глитча
                float glitchMask = step(1.0 - _GlitchIntensity, noise2);
                
                // Смешивание базового цвета с глитчем
                return lerp(baseColor, glitchColor, glitchMask * _GlitchBlend);
            }
            ENDCG
        }
    }
}