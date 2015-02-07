Shader "Amps/ColorPreview"
{
	Properties 
	{	
		_Color("Color", Vector) = (0,0.25,1,0.75)
		_CheckerTiling("Checker tiling", Vector) = (20,2,0,0)
		_CheckerContrast("Checker contrast", Float) = 0.1
	}
	
	SubShader 
	{
		Pass
		{
			Tags
			{
				"Queue"="Opaque"
				"IgnoreProjector"="True"
				"RenderType"="Opaque"
			}

			Blend One Zero
			AlphaTest Greater .01
			ColorMask RGB
			Cull Off
			Lighting Off
			ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 3.0
			#pragma multi_compile_particles
			#include "UnityCG.cginc"

			float4 _Color;
			float4 _CheckerTiling;
			float _CheckerContrast;

			struct appdata
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float4 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			struct vertexToFragment
			{
				float4 vertexPosition : SV_POSITION;
				float4 vertexColor : COLOR;
				float4 uv0: TEXCOORD0;
			};


			vertexToFragment vert (appdata_full a)
			{
				vertexToFragment v;
				
				v.vertexColor = a.color;
				v.uv0 = float4( a.texcoord.xy, 0, 0 );
				v.vertexPosition =  mul (UNITY_MATRIX_MVP, a.vertex);
				
				return v;
			}

			fixed4 frag (vertexToFragment i) : COLOR
			{
				fixed4 debug = fixed4(1.0,0.5,0.0,1.0);
				fixed4 finalColor = fixed4(1.0,1.0,0.0,1.0);

				float mask1 = clamp((i.uv0.r - (1.0/3.0)) * 256, 0, 1);
				float mask2 = clamp((i.uv0.r - (2.0/3.0)) * 256, 0, 1);
				float3 clampedRGB = clamp(_Color.rgb, 0, 1);
				float3 tonemappedRGB = _Color.rgb;
				float maxRGB = max(_Color.r, max(_Color.g, _Color.b));
				if (maxRGB > 1)
				{
					tonemappedRGB = _Color.rgb / maxRGB.rrr;
				}
				float3 splitRGB = lerp(clampedRGB, tonemappedRGB, mask1);

				float2 checkers = frac(i.uv0.rg * _CheckerTiling.rg) - float2(0.5, 0.5);
				checkers = clamp(checkers * 256, 0, 1);
				float checkersMask = abs(checkers.r - checkers.g);
				float3 checkersBackground = lerp(0.5 - _CheckerContrast, 0.5 + _CheckerContrast, checkersMask);

				float3 splitA = lerp(checkersBackground, splitRGB, _Color.a);

				finalColor.rgb = lerp(splitRGB, splitA, mask2);

				finalColor.a = 1;
				return finalColor;
			}
			ENDCG
		}
	}
	SubShader
	{
		Pass
		{
		}
	}
}