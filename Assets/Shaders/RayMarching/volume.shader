Shader "My/RayMarching/VolumeShader"
{
    Properties
    {
        _MainTex ("Texture", 3D) = "white" {}
        _Alpha ("Alpha", float) = 0.02
        _StepSize ("Step Size", float) = 0.01
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define MAX_STEP_COUNT 128

            #define EPSILON 0.00001f

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 objectVertex : TEXCOORD0;
                float3 vectorToSurface : TEXCOORD1;
            };

            sampler3D _MainTex;
            float4 _MainTex_ST;
            float _Alpha;
            float _StepSize;

            v2f vert (appdata v)
            {
                v2f o;

                o.objectVertex = v.vertex;

                float3 worldVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vectorToSurface = worldVertex - _WorldSpaceCameraPos;

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 BlendUnder(float4 color, float4 newColor)
            {
                color.rgb += (1.0 - color.a) * newColor.a * newColor.rgb;
                color.a += (1.0 - color.a) * newColor.a;
                return color;
            }
 
            fixed4 frag(v2f i) : SV_Target
            {
                float4 color = float4(0, 0, 0, 0);

                // 在世界空间，从表面顶点触发沿着视线方向 Ray Marching
                float3 rayOrigin = i.objectVertex;
                float3 rayDirection = mul(unity_WorldToObject, float4(normalize(i.vectorToSurface), 1));

                float3 samplePosition = rayOrigin;

                for (int i = 0; i < MAX_STEP_COUNT; i++)
                {
                    // 只在 Cube 包围盒中累计颜色
                    if(max(abs(samplePosition.x), max(abs(samplePosition.y), abs(samplePosition.z))) < 0.5f + EPSILON)
                    {
                        float4 sampledColor = tex3D(_MainTex, samplePosition + float3(0.5f, 0.5f, 0.5f));
                        sampledColor.a *= _Alpha;
                        color = BlendUnder(color, sampledColor);
                        samplePosition += rayDirection * _StepSize;
                    }
                }

                return color;
            }
            ENDCG
        }
    }
}