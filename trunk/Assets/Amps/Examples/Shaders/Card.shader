Shader "Amps/Card"
{
    Properties
    {
      _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
      Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }

      // First render front of the card.
      Cull Back
      CGPROGRAM
      #pragma surface surf Lambert
      #pragma target 3.0

      struct Input
      {
          float2 uv_MainTex;
          float4 color : COLOR;
      };
      sampler2D _MainTex;
      
      float Quantize(float base, float steps)
      {
        return floor((base * steps) + 0.5) / steps;
      }

      void surf (Input IN, inout SurfaceOutput o)
      {
        float2 baseUV = IN.uv_MainTex.xy;

        // Opacity
        float cornerSharpness = 14;
        float2 uvForOpacity = pow(abs((baseUV - 0.5) * 1.92), cornerSharpness);
        float finalOpacity = uvForOpacity.x + uvForOpacity.y;
        finalOpacity = 1 - clamp(finalOpacity, 0, 1);
        clip(finalOpacity - 0.3333);

        // Diffuse
        float faceIndex = IN.color.r;
        float2 scaledUV = baseUV * float2(0.125, 1.96);

        float2 uvOffset = float2(Quantize(faceIndex, 8), 0);

        float2 scaledUVTop = scaledUV + float2(0, 0.01);
        float3 topHalf = tex2D(_MainTex, scaledUVTop + uvOffset).rgb;

        float2 scaledUVBottom = (-1 * scaledUV) + float2(0.125, 0);
        float3 bottomHalf = tex2D(_MainTex, scaledUVBottom + uvOffset).rgb;

        float halfMask = clamp((baseUV.g - 0.5) * 256, 0, 1);

        //o.Albedo = halfMask;
        o.Albedo = lerp(bottomHalf, topHalf, halfMask);
      }
      ENDCG
    } 
    Fallback "Diffuse"
  }