Shader "Amps/Vertex normal display"
{
	Properties 
	{
	}
	
	SubShader 
	{
		Tags
		{
			"Queue"="Geometry"
			"IgnoreProjector"="True"
			"RenderType"="Opaque"
		}
		LOD 100
		Cull Off
		Lighting Off
		ZWrite On

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			#pragma only_renderers d3d9

			struct vertexToFragment
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float3 normal : TEXCOORD0;
			};

			vertexToFragment vert (appdata_full v)
			{
				vertexToFragment o;

				o.color = v.color;
				o.normal = v.normal;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

				return o;
			}

			float4 frag (vertexToFragment i) : COLOR
			{
				float4 returnValue = float4(1);
				returnValue.rgb = (i.normal * 0.5) + 0.5;
				return returnValue;
			}

			ENDCG
		}
	}
}