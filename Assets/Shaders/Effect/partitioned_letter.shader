Shader "My/Effect/partitioned_letter"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_UpColor ("Up Color", Color) = (0,0,0,1)
		_LowerColor ("Lower Color", Color) = (1,0,1,1)
		_BlurSize ("Blur Size", Float) = 1.0

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}

	CGINCLUDE

		#include "UnityCG.cginc"
		#include "UnityUI.cginc"

	ENDCG
	
	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Pass
		{
			Name "Low"
			
			Stencil
			{
				Ref 8
				Comp NotEqual
				Pass Keep
			}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest [unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			//#pragma enable_d3d11_debug_symbols

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float4 worldPosition : TEXCOORD0;
				float2 texcoord  : TEXCOORD1;
			};

			v2f vert(appdata_img IN)
			{
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				
				return OUT;
			}

			sampler2D _MainTex;
			fixed4 _LowerColor;

			fixed4 _TextureSampleAdd;
			float4 _ClipRect;

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = half4(0, 0, 0, 0);
				color.a = tex2D(_MainTex, IN.texcoord).a;
				color.xyz = _LowerColor.xyz;

				#ifdef UNITY_UI_CLIP_RECT
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				#endif
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				return color;
			}
		ENDCG
		}

		Pass
		{
			Name "UpAlphaWrite"

			Stencil
			{
				Ref 8
				Comp Equal
				Pass Keep
			}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest [unity_GUIZTestMode]
			Blend Zero One, DstAlpha Zero

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			//#pragma enable_d3d11_debug_symbols
			
			float _BlurSize;
			float4 _MainTex_TexelSize;

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float4 texcoords[4] : TEXCOORD1;
			};
			
			v2f vert(appdata_img IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);

				float2 uv = IN.texcoord;
				// blur
				OUT.texcoord = uv;
				OUT.texcoords[0].xy = uv + _MainTex_TexelSize * _BlurSize * float2( 1,   1);
				OUT.texcoords[0].zw = uv + _MainTex_TexelSize * _BlurSize * float2( 1,  -1);
				OUT.texcoords[1].xy = uv + _MainTex_TexelSize * _BlurSize * float2(-1,  -1);
				OUT.texcoords[1].zw = uv + _MainTex_TexelSize * _BlurSize * float2(-1,  1);
				OUT.texcoords[2].xy = uv + _MainTex_TexelSize * _BlurSize * float2( 2,   2);
				OUT.texcoords[2].zw = uv + _MainTex_TexelSize * _BlurSize * float2( 2,  -2);
				OUT.texcoords[3].xy = uv + _MainTex_TexelSize * _BlurSize * float2(-2,  -2);
				OUT.texcoords[3].zw = uv + _MainTex_TexelSize * _BlurSize * float2(-2,  2);
				return OUT;
			}
			
			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{		
				half4 color = half4(0, 0, 0, 0);
				color.a += tex2D(_MainTex, IN.texcoord).a * 2;
				color.a += tex2D(_MainTex, IN.texcoords[0].xy).a;
				color.a += tex2D(_MainTex, IN.texcoords[0].zw).a;
				color.a += tex2D(_MainTex, IN.texcoords[1].xy).a;
				color.a += tex2D(_MainTex, IN.texcoords[1].zw).a;
				color.a += tex2D(_MainTex, IN.texcoords[2].xy).a;
				color.a += tex2D(_MainTex, IN.texcoords[2].zw).a;
				color.a += tex2D(_MainTex, IN.texcoords[3].xy).a;
				color.a += tex2D(_MainTex, IN.texcoords[3].zw).a;
				color.a *= 0.1;
				return color;
			}
		ENDCG
		}
	
		Pass
		{
			Name "Up"

			Stencil
			{
				Ref 8
				Comp Equal
				Pass Keep
			}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest [unity_GUIZTestMode]
			Blend DstAlpha OneMinusDstAlpha

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			//#pragma enable_d3d11_debug_symbols

			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			#pragma multi_compile __ UNITY_UI_CLIP_RECT

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float4 worldPosition : TEXCOORD0;
				float2 texcoord  : TEXCOORD1;
			};
			
			fixed4 _UpColor;
			float4 _MainTex_TexelSize;

			v2f vert(appdata_img IN)
			{
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				
				OUT.texcoord = IN.texcoord;

				return OUT;
			}

			sampler2D _MainTex;

			fixed4 _TextureSampleAdd;
			float4 _ClipRect;

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = half4(0, 0, 0, 0);
				color.a += tex2D(_MainTex, IN.texcoord).a;
				color.xyz = _UpColor.xyz;

				#ifdef UNITY_UI_CLIP_RECT
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				#endif
			
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				return color;
			}
		ENDCG
		}
	
	} // end of SubShader
}
