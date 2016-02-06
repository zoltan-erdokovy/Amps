Shader "Amps/Vertex color display (lit)"
{
	Properties 
	{
		_MainTex("Diffuse map", 2D) = "gray" {}
		_SpecColor("Specular color", Vector) = (1,1,1,0)
		_Glossiness("Glossiness", Float) = 16
		_EmissiveStrength("Emissive strength", Float) = 0.1
	}
	
	SubShader 
	{
		Tags
		{
			"RenderType"="Opaque"
		}

		CGPROGRAM
		//#pragma surface surf BlinnPhongEditor vertex:vert
		#pragma surface surf BlinnPhong vertex:vert
		#pragma target 3.0
		#pragma only_renderers d3d9 opengl

		sampler2D _MainTex;
		//fixed4 _SpecColor;	// Already declared in Lighting.cginc.
		float _Glossiness;
		float _EmissiveStrength;
			
		struct Input
		{
			float2 uv_MainTex;
			float3 sWorldNormal;
			float3 viewDir;
			float4 vertexColor;
		};

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.sWorldNormal = mul((float3x3)_Object2World, SCALED_NORMAL);
			o.vertexColor = v.color;
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			o.Normal = float3(0.0,0.0,1.0);
			o.Alpha = 1.0;
			o.Albedo = 0.0;
			o.Emission = 0.0;
			o.Gloss = 0.0;
			o.Specular = 0.0;
			
			fixed4 vColor = IN.vertexColor;

			// ALBEDO
			float4 finalAlbedo = tex2D(_MainTex, IN.uv_MainTex) * vColor;

			// EMISSION
			float3 finalEmission = lerp(float3(0,0,0), finalAlbedo.rgb, _EmissiveStrength);

			// SPECULAR
			float3 finalSpecular = clamp(lerp((finalAlbedo.rgb * _SpecColor.rgb), _SpecColor.rgb, _SpecColor.a), 0, 64);

			float finalSpecularPower = _Glossiness;

			o.Albedo = finalAlbedo.rgb;
			o.Emission = finalEmission;
			o.Gloss = finalSpecular;
			o.Specular = finalSpecularPower;
			o.Normal = normalize(o.Normal);
		}
		ENDCG
	}
	Fallback "Diffuse"
}