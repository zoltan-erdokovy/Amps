Shader "Amps/Vertex color display"
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
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Lighting Off
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			struct vertexToFragment
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			vertexToFragment vert (appdata_full v)
			{
				vertexToFragment o;

				o.color = v.color;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

				return o;
			}

			fixed4 frag (vertexToFragment i) : COLOR
			{
				return i.color;
			}

			ENDCG
		}
	}
}