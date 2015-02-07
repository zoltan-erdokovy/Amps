Shader "Amps/UV display"
{
	Properties 
	{
		_MainTexture("Texture", 2D) = "gray" {}
		_TextureOpacity("Texture opacity", Float) = 0
	}
	
	SubShader 
	{
		Tags
		{
			"Queue"="Geometry"
			"IgnoreProjector"="True"
			"RenderType"="Opaque"
		}
		
		Cull Back
		ZWrite On
		ZTest LEqual
		ColorMask RGBA

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTexture;
			float4 _MainTexture_ST;
			float _TextureOpacity;

			struct vertexToFragment
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float3 normal : TEXCOORD3;
			};

			vertexToFragment vert (appdata_full v)
			{
				vertexToFragment o;

				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTexture);
				o.normal = v.normal;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

				return o;
			}

			fixed4 frag (vertexToFragment i) : COLOR
			{
				fixed4 finalColor = fixed4(1.0,1.0,1.0,1.0);

				float3 mainTexture = tex2D(_MainTexture, i.texcoord);
				float3 uvColor = float3(i.texcoord.x, i.texcoord.y, 0);
				finalColor.rgb = lerp(uvColor, mainTexture, _TextureOpacity);
				finalColor.a = 1.0;
				return finalColor;
			}

			ENDCG
		}
	}
	Fallback "Diffuse"
}