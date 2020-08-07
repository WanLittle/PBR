#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced '_WorldSpaceCameraPos.w' with '1.0'

Shader "VolumeRendering/VolumeRendering"
{
    Properties
    {
        _LightColor("LightColor", Color) = (1, 1, 1, 1)
        _Volume("Volume", 3D) = "" {}
        _Transfer("Transfer", 2D) = "White"{}
        _Intensity("Intensity", Range(1.0, 5.0)) = 1
        _Threshold("Threshold", Range(0.0, 1.0)) = 1
        _SliceMin("Slice min", Vector) = (0.0, 0.0, 0.0, 0.0)
        _SliceMax("Slice max", Vector) = (1.0, 1.0, 1.0, 0.0)
    }
    
    SubShader{
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha
        // ZTest Always
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            half4 _LightColor;
            sampler3D _Volume;
            sampler2D _Transfer;
            half _Intensity, _Threshold;
            half3 _SliceMin, _SliceMax;

            struct Ray {
                float3 origin;
                float3 dir;
            };

            struct AABB {   //Aligned Asix Bounding Box
                float3 min;
                float3 max;
            };

            float intersect(Ray r, AABB aabb)
            {
                float3 invR = 1.0 / r.dir;
                float3 tbot = invR * (aabb.min - r.origin);
                float3 ttop = invR * (aabb.max - r.origin);
                float3 tmax = max(ttop, tbot);
                float2 t = min(tmax.xx, tmax.yz);
                return min(t.x, t.y);
            }

            float4 sample_volume(float3 uv, float3 p)
            {
                float v = tex3D(_Volume, uv).r * _Intensity;
                float2 uvt = float2(0, v);
                float4 color = tex2D(_Transfer, uvt);

                float3 axis = p + 0.5f;//get uv
                float min = step(_SliceMin.x, axis.x) * step(_SliceMin.y, axis.y) * step(_SliceMin.z, axis.z);
                float max = step(axis.x, _SliceMax.x) * step(axis.y, _SliceMax.y) * step(axis.z, _SliceMax.z);

                return color * min * max;
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 local : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
	            v2f o;
	            o.vertex = UnityObjectToClipPos(v.vertex);
	            o.uv = v.uv;
	            o.local = v.vertex.xyz;
	            return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                Ray ray;
                ray.origin = i.local;
                //ray.dir = normalize(i.local - mul(unity_WorldToObject, _WorldSpaceCameraPos));
                ray.dir = normalize(i.local - mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1)).xyz);
                
                AABB aabb;
                aabb.min = float3(-0.5, -0.5, -0.5);
                aabb.max = float3(0.5, 0.5, 0.5);

                float dist = intersect(ray, aabb);
                float3 start = ray.origin;
                float3 end = ray.origin + ray.dir * dist;

                int times = floor(dist * 57.73672f);     // 57.73672 = 100 / sqrt(3)
                float3 ds = ray.dir * 0.01732f;        //0.01732 = sqrt(3) / 100

                float4 dst = float4(0, 0, 0, 0);
                float3 p = start;

                [unroll(100)]
                for (int iter = 0; iter < times; iter++)
                {
                    float3 uv = p + 0.5f; //get uv;
                    float4 src = _LightColor * sample_volume(uv, p);
                    // blend
                    dst = dst + (1 - dst.a) * src;
                    dst.a = dst.a + (1 - dst.a) * src.a;
                    
                    if (dst.a > _Threshold) break;
                    p += ds;
                }

                return saturate(dst);
            }

            ENDCG
        }
    }
}
