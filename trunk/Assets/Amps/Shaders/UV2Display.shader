Shader "Amps/UV2 display"
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
				o.texcoord = v.texcoord1; //TRANSFORM_TEX(v.texcoord1,_MainTexture);
				o.normal = v.normal;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

				return o;
			}

			// float mod(float x, float y)
			// {
  			//	return x - y * floor(x/y);
			// }

			float Pack(float2 input)
			{
				float precision = 1024;
				float2 output = input;
				output.x = floor(output.x * (precision - 1));
				output.y = floor(output.y * (precision - 1));

				return (output.x * precision) + output.y;
			}

			float2 Unpack(float input)
			{
				float precision = 1024;
				float2 output = float2(0,0);

    			output.y = fmod(input, precision) / (precision - 1);
    			output.x = floor(input / precision) / (precision - 1);

    			// output.x = floor(input / precision);
				// output.y = input - floor(input);

    			return output;
			}

			float4 frag (vertexToFragment i) : COLOR
			{
				float4 finalColor = float4(0.0,0.0,0.0,0.0);

				float4 uvColor = float4(0,0,0,0);
				//float2 temp = Unpack(i.texcoord.x);
				//uvColor.x = temp.x;
				//uvColor.y = temp.y;
				uvColor.x = i.texcoord.x;
				uvColor.y = i.texcoord.y;

				//float2 temp = Unpack(Pack(float2(0.5, 0.0012)));
				//temp = Unpack(i.texcoord.y);
				//uvColor.z = temp.x;
				//uvColor.w = temp.y;

				finalColor = uvColor;
				return finalColor;
			}

			ENDCG
		}
	}
	Fallback "Diffuse"
}