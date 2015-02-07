Shader "Amps/ColorPicker_A"
{
	Properties 
	{	
		_Alpha("Alpha", Float) = 0
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

			float _Alpha;
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

			fixed4 frag (vertexToFragment i) : COLOR
			{
				fixed4 debug = fixed4(1.0,0.5,0.0,1.0);
				fixed4 finalColor = fixed4(1.0,1.0,0.0,1.0);

				finalColor.rgb = i.uv0.rrr;

				float marker = abs(i.uv0.r - _Alpha);
				marker = ((marker / _MarkerSize) - 0.5) / _MarkerThickness;
				float markerMask = clamp(abs(marker), 0, 1);
				marker = clamp(marker * 4, 0, 1);
				
				finalColor.rgb = lerp(marker.rrr, finalColor.rgb, markerMask);

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