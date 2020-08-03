// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "My/TestMRT" 
{
    Properties 
    {
        _MainTex ("", 2D) = "" {}
    }
   
    CGINCLUDE
   
    #include "UnityCG.cginc"
     
    struct v2f 
    {
        float4 pos : POSITION;
        float2 uv : TEXCOORD0;
    };
   
    struct PixelOutput 
    {
        float4 col0 : COLOR0;
        float4 col1 : COLOR1;
    };
   
    sampler2D _MainTex;
    sampler2D _Tex0;
    sampler2D _Tex1;
   
    v2f vert( appdata_img v )
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord.xy;
        return o;
    }
   
    PixelOutput fragTestMRT(v2f pixelData)
    {
        PixelOutput o;
        o.col0 = float4(1.0f, 0.0f, 0.0f, 1.0f);
        o.col1 = float4(0.0f, 1.0f, 0.0f, 1.0f);
        return o;
    }
   
    float4 fragShowMRT(v2f pixelData) : COLOR0
    {
        //return tex2D(_Tex0, pixelData.uv);
        return tex2D(_Tex1, pixelData.uv);
    }
   
    ENDCG
   
    Subshader 
    {
        Pass 
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }
 
            CGPROGRAM
            #pragma glsl
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment fragTestMRT
            #pragma target 3.0
            ENDCG
        }
        Pass 
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }
 
            CGPROGRAM
            #pragma glsl
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment fragShowMRT
            #pragma target 3.0
            ENDCG
        }
    }
 
    Fallback off
   
}