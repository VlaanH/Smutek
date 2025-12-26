Shader "Custom/Pixelate"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
      _PixelSize("Pixel Size", Float) = 0.0008


    }
    SubShader
    {
        Pass
        {
            ZWrite Off
            Cull Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _PixelSize;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                uv = floor(uv / _PixelSize) * _PixelSize;
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
