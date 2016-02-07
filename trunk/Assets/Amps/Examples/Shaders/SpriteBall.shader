Shader "Amps/Sprite ball"
{
	Properties 
	{	
		_BaseColor("Base color", Vector) = (0.059,0.118,0.1,1)
		_FresnelColor("Fresnel color", Vector) = (1,2,0.2,1)
		_FresnelPower("Fresnel power", Float) = 4
		_Texture("Texture", 2D) = "white" {}
		_TextureStrength("Texture strength", Float) = 0.35
		_Light1Color("Light 1 color", Vector) = (1,1,0.9,1)
		_Light1Direction("Light 1 direction", Vector) = (1,0.85,0,1)
		// _Light2Color("Light 2 color", Vector) = (0.1,0.1,0.15,1)
		// _Light2Direction("Light 2 direction", Vector) = (-1,0.85,0,1)
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

			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGBA
			Cull Back
			Lighting Off
			ZWrite On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 3.0
			#pragma only_renderers d3d9 opengl
			#pragma multi_compile_particles
			#include "UnityCG.cginc"

			float4 _BaseColor;
			float4 _FresnelColor;
			float _FresnelPower;
			sampler2D _Texture;
			float _TextureStrength;
			float4 _Texture_ST;
			float4 _Light1Color;
			float4 _Light1Direction;
			// float4 _Light2Color;
			// float4 _Light2Direction;

			float Distance2D(float2 p1, float2 p2)
			{
				return sqrt(pow(p2.x-p1.x, 2) + pow(p2.y-p1.y, 2));
			}

			float3 TangentNormalToWorld(float3 normalT, float3 normalW, float3 tangentW, float3 binormalW)
			{
				float3x3 local2WorldTranspose = float3x3(tangentW, binormalW, normalW);
				return normalize(mul(normalT, local2WorldTranspose));
			}

			float power(float base, float exponent)
			{
				return pow(abs(base), exponent);
			}

			float AngularGradient(float2 uv)
			{
				float returnValue;

				returnValue = atan2(uv.x - 0.5, uv.y - 0.5).r;
				returnValue = (returnValue + 3.141592) / 6.283185;

				return returnValue;
			}

			struct vertexToFragment
			{
				float4 uv : TEXCOORD0;
				float4 position : SV_POSITION;
				float4 positionWorld : TEXCOORD1;
				float4 color : COLOR;
				float3 normal : TEXCOORD2;
				float3 normalWorld : TEXCOORD3;
				float4 tangent : TEXCOORD4;
				float3 tangentWorld : TEXCOORD5;
				float3 binormalWorld : TEXCOORD6;
			};

			vertexToFragment vert (appdata_full a)
			{
				vertexToFragment returnValue;

				float4x4 modelMatrix = _Object2World;
				float4x4 modelMatrixInverse = _World2Object; 
 				
 				returnValue.position = mul(UNITY_MATRIX_MVP, a.vertex);
				returnValue.positionWorld = mul(modelMatrix, a.vertex);
				returnValue.uv = float4( a.texcoord.xy, a.texcoord1.xy);
 				returnValue.color = a.color;
				returnValue.tangent = a.tangent;
				returnValue.tangentWorld = normalize(mul(modelMatrix, float4(a.tangent.x, a.tangent.y, a.tangent.z, 0.0)).xyz);
				returnValue.normal = a.normal;
				returnValue.normalWorld = mul( _Object2World, float4( a.normal, 0.0 )).xyz;
				returnValue.binormalWorld = normalize(cross(returnValue.normalWorld, returnValue.tangentWorld) * a.tangent.w);

				return returnValue;
			}

			float4 frag (vertexToFragment i) : COLOR
			{
				float3 debug = float3(0.0,0.0,0.0);
				float4 finalColor = float4(0.0,1.0,0.0,1.0);

				// Extra data
				float height = i.uv.z;
				float textureRotation = i.uv.w;
				float finalTime = _Time.g;
				float4 vertexColor = i.color;

				// Vertical stretch
				float flatness = lerp(512, 1, power(height, 0.003906));
				float2 finalVerticalSquish = float2(1,1);
				float flatnessMask = power(clamp(i.normalWorld.y,0,1), 1/flatness);
				finalVerticalSquish.y = lerp(flatness, 1, flatnessMask);

				// Final normal
				float3 finalNormal = float3(0,0,0);
				finalNormal.xy = ((i.uv.xy * 2) * finalVerticalSquish.xy) - finalVerticalSquish.xy;
				float bubbleGradient = Distance2D(finalVerticalSquish.xy, (i.uv.xy * 2) * finalVerticalSquish.xy);
				finalNormal.z = sqrt(1 - power(bubbleGradient, 2));
				finalNormal = TangentNormalToWorld(finalNormal, i.normalWorld, i.tangentWorld, i.binormalWorld);

				// Opacity
				float finalOpacity = clamp(1 - power(bubbleGradient, 64), 0, 1);
				clip(finalOpacity - 0.5);

				// Lighting
				float light1Gradient = dot(normalize(_Light1Direction.xyz), finalNormal);
				float3 lighting = clamp(light1Gradient, 0, 1) * _Light1Color.rgb;
				//lighting = lighting + (clamp(dot(normalize(_Light2Direction.xyz), finalNormal), 0, 1) * _Light2Color.rgb);
				float3 desatVertexColor = (vertexColor.r * 0.3) + (vertexColor.g * 0.59) + (vertexColor.b * 0.11);
				desatVertexColor = lerp(vertexColor.rgb, desatVertexColor, 0.75);
				lighting = lighting * desatVertexColor;

				// Diffuse color
				float finalFresnelMask = power(bubbleGradient, _FresnelPower);
				float3 diffuseColor = lerp(_BaseColor.rgb * _BaseColor.a, _BaseColor.rgb, finalFresnelMask);
				diffuseColor = lerp(diffuseColor, _FresnelColor.rgb, clamp(1 - light1Gradient, 0, 1) * finalFresnelMask);
				diffuseColor = diffuseColor * vertexColor.rgb;

				// Texturing
				float latitude = ((atan2(finalNormal.x, finalNormal.z) + 3.141592) / 6.283183).r;
				latitude = frac(latitude + textureRotation);
				float longitude = dot(float3(0,1,0), finalNormal);
				float2 finalUV = float2(latitude, longitude) * float2(4,1);
				float finalTexture = lerp(1, tex2D(_Texture, finalUV).r, _TextureStrength);

				finalColor.rgb = (diffuseColor + lighting) * finalTexture;
				finalColor.a = 1;
				
				//finalColor.rgb = debug;
				//finalColor.a = 1;

				finalColor.rgb = pow(abs(finalColor.rgb), 1/2.2);

				return finalColor;
			}
			ENDCG
		}
	}
}