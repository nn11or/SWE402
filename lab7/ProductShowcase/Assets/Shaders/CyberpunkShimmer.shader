
Shader "Custom/CyberpunkShimmer"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.05, 0.02, 0.1, 1)
        _EmissionColor ("Emission Color", Color) = (0.5, 0.0, 1.0, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 3.0
        _PulseSpeed ("Pulse Speed", Range(0.1, 5)) = 1.5
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0, 1)) = 0.8
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _EmissionColor;
                float _EmissionIntensity;
                float _PulseSpeed;
                float _Metallic;
                float _Smoothness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Pulsing emission based on sine of time
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;

                // Shimmer pattern based on world position
                float shimmer = sin(IN.positionWS.x * 5.0 + _Time.y * 2.0)
                              * sin(IN.positionWS.y * 5.0 + _Time.y * 1.5);
                shimmer = shimmer * 0.5 + 0.5;

                // Basic lighting
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(IN.normalWS, mainLight.direction));
                float3 diffuse = _BaseColor.rgb * mainLight.color * NdotL;
                float3 ambient = _BaseColor.rgb * 0.1;

                // Combine with animated emission
                float3 emission = _EmissionColor.rgb * _EmissionIntensity * pulse * shimmer;
                float3 finalColor = diffuse + ambient + emission;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
