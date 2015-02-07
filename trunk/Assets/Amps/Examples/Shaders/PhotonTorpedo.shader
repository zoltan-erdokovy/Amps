Shader "Amps/PhotonTorpedo"
{
	Properties 
	{
		_MainTexture("Main texture", 2D) = "gray" {}
		_Color1("Color 1", Vector) = (1,0.2,0.01,1)
		_Color2("Color 2", Vector) = (1,0.8,0.7,1)
	}
	
	SubShader 
	{
		Pass
		{
			Tags
			{
				"Queue"="Transparent"
				"IgnoreProjector"="True"
				"RenderType"="Transparent"
			}

			Blend One One
			ColorMask RGBA
			Cull Back
			Lighting Off
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 3.0
			#pragma only_renderers d3d9 opengl
			#pragma multi_compile_particles
			#include "UnityCG.cginc"

			sampler2D _MainTexture;
			float4 _MainTexture_ST;
			float4 _Color1;
			float4 _Color2;

			struct vertexToFragment
			{
				float4 uv0 : TEXCOORD0;
				float4 position : SV_POSITION;
				float4 color : COLOR;
			};

			float2 TexCoordRotation(float2 texCoords, float time, float2 center, float speed)
			{
				float t = frac(time * 0.15915494 * speed);	// Adjusting to get behaviour similar to UDK.
				float2 adjustedCoords = texCoords - center;
				float2 finalCoords = float2(0, 0);
				finalCoords.r = (adjustedCoords.r * cos(t * 6.283185)) - (adjustedCoords.g * sin(t * 6.283185));
				finalCoords.g = (adjustedCoords.r * sin(t * 6.283185)) + (adjustedCoords.g * cos(t * 6.283185));
				finalCoords += center;

				return finalCoords;
			}

			float CustomSin(float v)
			{
				// Using a sine texture here could save many instructions.
				return ((sin(v * 6.283185) + 1) * 0.5);
			}

			float power(float base, float exponent)
			{
				return pow(abs(base), exponent);
			}

			vertexToFragment vert (appdata_full a)
			{
				vertexToFragment returnValue;

 				returnValue.position = mul(UNITY_MATRIX_MVP, a.vertex);
				returnValue.uv0 = float4( a.texcoord.xy, 0, 0);
 				returnValue.color = a.color;

				return returnValue;
			}

			float4 frag (vertexToFragment i) : COLOR
			{
				float3 debug = float3(0.0,0.0,0.0);
				float4 finalColor = float4(0.0,1.0,0.0,1.0);

				// Vertex colors
				float timeOffset = i.color.r;
				float finalTime = _Time.g + timeOffset;
				float deathAmount = i.color.g;
				float colorBlend = i.color.b;
				float fadeAmount = i.color.a;

				// UVs
				float2 baseUV = i.uv0.xy;
				float2 layer1UV = TexCoordRotation(baseUV, finalTime, float2(0.5, 0.5), 0.7);
				float2 layer2UV = TexCoordRotation(baseUV, finalTime, float2(0.5, 0.5), -5.1);
				float2 layer3UV = TexCoordRotation(baseUV, finalTime, float2(0.5, 0.5), -1.1);

				// Texture sampling
				float3 layer1 = tex2D(_MainTexture, layer1UV).rgb;
				float3 layer2 = tex2D(_MainTexture, layer2UV).rgb;
				float3 layer3 = tex2D(_MainTexture, layer3UV).rgb;

				finalColor.rgb = (layer1.r + layer3.r) * (layer2.b + layer3.g);
				finalColor.rgb = lerp(finalColor.rgb, layer3.g, deathAmount);
				finalColor.rgb = finalColor.rgb * lerp(0.5, 1, CustomSin(finalTime));
				finalColor.rgb = finalColor.rgb * lerp(_Color1, _Color2, colorBlend);
				finalColor.rgb = finalColor.rgb * fadeAmount;

				// finalColor.rgb = debug;
				// finalColor.a = 1;

				//finalColor.rgb = pow(abs(finalColor.rgb), 1/2.2);

				return finalColor;
			}
			ENDCG
		}
	}
}