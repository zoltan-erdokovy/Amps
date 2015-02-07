Shader "Amps/ColorPicker_SV"
{
	Properties 
	{	
		_HSV("HSV", Vector) = (1,0.25,0.5,0)
		_MarkerSize("Marker size", Float) = 0.04
		_MarkerThickness("Marker thickness", Float) = 0.125
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

			float4 _HSV;
			float _MarkerSize;
			float _MarkerThickness;

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

			float3 RGBtoHSV(in float3 RGB)
			{
				float3 HSV = 0;

				HSV.z = max(RGB.r, max(RGB.g, RGB.b));
				float M = min(RGB.r, min(RGB.g, RGB.b));
				float C = HSV.z - M;

				if (C != 0)
				{
					HSV.y = C / HSV.z;
					float3 Delta = (HSV.z - RGB) / C;
					Delta.rgb -= Delta.brg;
					Delta.rg += float2(2,4);
					if (RGB.r >= HSV.z)
						HSV.x = Delta.b;
					else if (RGB.g >= HSV.z)
						HSV.x = Delta.r;
					else
						HSV.x = Delta.g;
					HSV.x = frac(HSV.x / 6);
				}
				return HSV;
			}

			float3 Hue(float H)
			{
				float R = abs(H * 6 - 3) - 1;
				float G = 2 - abs(H * 6 - 2);
				float B = 2 - abs(H * 6 - 4);
				return saturate(float3(R,G,B));
			}

			float3 HSVtoRGB(in float3 HSV)
			{
				return ((Hue(HSV.x) - 1) * HSV.y + 1) * HSV.z;
			}

			fixed4 frag (vertexToFragment i) : COLOR
			{
				fixed4 debug = fixed4(1.0,0.5,0.0,1.0);
				fixed4 finalColor = fixed4(1.0,1.0,0.0,1.0);
				float2 markerCoordinates = clamp(float2(_HSV.z, 1 - _HSV.y), 0, 1);

				finalColor.rgb = HSVtoRGB(float3(_HSV.x, 1 - i.uv0.g, i.uv0.r));

				float2 marker = abs(i.uv0 - markerCoordinates);
				marker = sqrt(pow(marker.x, 2) + pow(marker.y, 2));
				marker = ((marker / _MarkerSize) - 0.5) / _MarkerThickness;
				marker.x = clamp(marker.x * 4, 0, 1);
				marker.y = clamp(abs(marker.y), 0, 1);

				finalColor.rgb = lerp(marker.xxx, finalColor.rgb, marker.y);

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