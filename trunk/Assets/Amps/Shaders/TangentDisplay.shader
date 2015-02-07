Shader "Amps/Vertex tangent display"
{
	Properties 
	{
	}
	
	SubShader 
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Back
		Lighting Off
		ZWrite Off

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
				float4 tangent : TEXCOORD4;
			};

			vertexToFragment vert (appdata_full v)
			{
				vertexToFragment o;

				o.color = v.color;
				o.normal = v.normal;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.tangent = v.tangent;

				return o;
			}

			float4 frag (vertexToFragment i) : COLOR
			{
				float4 returnValue = float4(1);
				//returnValue.rgb = (i.tangent.rgb * 0.5) + 0.5;
				returnValue = i.tangent;
				return returnValue;
			}

			ENDCG
		}
	}
}