Shader "Amps/Opaque"
{
	Properties 
	{
		_MainTexture("Diffuse map", 2D) = "gray" {}
		_MainColor("Diffuse color", Vector) = (1,1,1,0)
		_EmissiveMap("Emissive map", 2D) = "black" {}
		_EmissiveColor("Emissive color", Vector) = (1,1,1,0)
		_SpecularMap("Specular map", 2D) = "gray" {}
		_SpecularColor("Specular color", Vector) = (1,1,1,0)
		_GlossinessLow("Glossiness low", Float) = 16
		_GlossinessHigh("Glossiness high", Float) = 128
		_NormalMap("Normal map", 2D) = "bump" {}
	}
	
	SubShader 
	{
		Tags
		{
			"Queue"="Geometry-1"
			"IgnoreProjector"="True"
			"RenderType"="Opaque"
		}

		Cull Back
		ZWrite On
		ZTest LEqual
		ColorMask RGBA
		Fog{}

		CGPROGRAM
		#pragma surface surf BlinnPhong vertex:vert
		#pragma target 3.0
		#pragma only_renderers d3d9 opengl

		sampler2D _MainTexture;
		float4 _MainColor;
		sampler2D _EmissiveMap;
		float4 _EmissiveColor;
		sampler2D _SpecularMap;
		float4 _SpecularColor;
		float _GlossinessLow;
		float _GlossinessHigh;
		sampler2D _NormalMap;
		float _DiffuseContribution;

		struct EditorSurfaceOutput
		{
    		half3 Albedo;
			half3 Normal;
    		half3 Emission;
			half Specular;
			half3 Gloss;
			half Alpha;
		};
			
		struct Input
		{
			float2 uv_MainTexture;
			float3 sWorldNormal;
			float3 viewDir;
		};

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.sWorldNormal = mul((float3x3)_Object2World, SCALED_NORMAL);
		}
			

		void surf (Input IN, inout SurfaceOutput o)
		{
			o.Normal = float3(0.0,0.0,1.0);
			o.Alpha = 1.0;
			o.Albedo = 0.0;
			o.Emission = 0.0;
			o.Gloss = 0.0;
			o.Specular = 0.0;
			
			// ALBEDO
			float3 finalAlbedo = tex2D(_MainTexture, IN.uv_MainTexture);
			float finalAlbedoBrightness = (finalAlbedo.r + finalAlbedo.g + finalAlbedo.b) / 3;
			
			finalAlbedo = clamp(lerp((finalAlbedo * _MainColor.rgb), _MainColor.rgb, _MainColor.a), 0, 64);

			// EMISSION
			float3 finalEmission = tex2D(_EmissiveMap, IN.uv_MainTexture);
			finalEmission = clamp(lerp((finalEmission * _EmissiveColor.rgb), _EmissiveColor.rgb, _EmissiveColor.a), 0, 64);

			// SPECULAR
			float4 allSpecData = tex2D(_SpecularMap, IN.uv_MainTexture);
			float3 finalSpecular = allSpecData.rgb;
			finalSpecular = clamp(lerp((finalSpecular * _SpecularColor.rgb), _SpecularColor.rgb, _SpecularColor.a), 0, 64);

			float finalSpecularPower = lerp(_GlossinessLow, _GlossinessHigh, allSpecData.a);

			o.Albedo = finalAlbedo;
			o.Emission = finalEmission;
			o.Gloss = finalSpecular;
			o.Specular = finalSpecularPower;
			o.Normal = normalize(o.Normal);
		}
		ENDCG
	}
	Fallback "Diffuse"
}