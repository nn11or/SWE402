using UnityEngine;
using UnityEditor;

/// <summary>
/// Since Shader Graphs can't be created reliably via script,
/// this helper creates a fallback custom shader AND provides
/// step-by-step instructions for creating the Shader Graph manually.
///
/// Menu: Tools > Create Shimmer Shader (Code Fallback)
/// Menu: Tools > Shader Graph Instructions
/// </summary>
public class ShaderGraphHelper : Editor
{
    // Creates a code-based shimmer shader as a working fallback
    [MenuItem("Tools/Create Shimmer Shader (Code Fallback)")]
    static void CreateShimmerShader()
    {
        string shaderCode = @"
Shader ""Custom/CyberpunkShimmer""
{
    Properties
    {
        _BaseColor (""Base Color"", Color) = (0.05, 0.02, 0.1, 1)
        _EmissionColor (""Emission Color"", Color) = (0.5, 0.0, 1.0, 1)
        _EmissionIntensity (""Emission Intensity"", Range(0, 10)) = 3.0
        _PulseSpeed (""Pulse Speed"", Range(0.1, 5)) = 1.5
        _Metallic (""Metallic"", Range(0, 1)) = 0.5
        _Smoothness (""Smoothness"", Range(0, 1)) = 0.8
    }
    SubShader
    {
        Tags { ""RenderType""=""Opaque"" ""RenderPipeline""=""UniversalPipeline"" }
        LOD 200

        Pass
        {
            Name ""ForwardLit""
            Tags { ""LightMode""=""UniversalForward"" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl""
            #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl""

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
    FallBack ""Universal Render Pipeline/Lit""
}
";

        string path = "Assets/Shaders/CyberpunkShimmer.shader";
        System.IO.File.WriteAllText(
            System.IO.Path.Combine(Application.dataPath, "..", path),
            shaderCode
        );
        AssetDatabase.Refresh();

        // Create material from this shader
        Shader shader = Shader.Find("Custom/CyberpunkShimmer");
        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.SetColor("_BaseColor", new Color(0.05f, 0.02f, 0.1f, 1f));
            mat.SetColor("_EmissionColor", new Color(0.5f, 0f, 1f, 1f));
            mat.SetFloat("_EmissionIntensity", 3f);
            mat.SetFloat("_PulseSpeed", 1.5f);
            mat.SetFloat("_Metallic", 0.5f);
            mat.SetFloat("_Smoothness", 0.8f);

            AssetDatabase.CreateAsset(mat, "Assets/Materials/M_CyberpunkShimmer.mat");
            Debug.Log("Shimmer shader and material created! Assign M_CyberpunkShimmer to Product_ShaderGraphDiamond.");

            // Try to assign to the diamond object
            GameObject diamond = GameObject.Find("Product_ShaderGraphDiamond");
            if (diamond != null)
            {
                diamond.GetComponent<Renderer>().sharedMaterial = mat;
                Debug.Log("Auto-assigned to Product_ShaderGraphDiamond!");
            }
        }

        EditorUtility.DisplayDialog("Shimmer Shader Created",
            "Custom/CyberpunkShimmer shader and material created.\n\n" +
            "NOTE: The assignment requires a Shader Graph.\n" +
            "Use Tools > Shader Graph Instructions for how to\n" +
            "create the required URP Lit Shader Graph version.\n\n" +
            "The code shader is a working backup.",
            "OK");
    }

    [MenuItem("Tools/Shader Graph Instructions")]
    static void ShowShaderGraphInstructions()
    {
        EditorUtility.DisplayDialog("How to Create the Shader Graph",
            "REQUIRED: URP Lit Shader Graph with 3+ nodes\n\n" +
            "1. Right-click Assets/Shaders > Create > Shader Graph > URP > Lit Shader Graph\n" +
            "2. Name it 'CyberpunkShimmerGraph'\n" +
            "3. Double-click to open the Shader Graph editor\n" +
            "4. Add these nodes (right-click > Create Node):\n\n" +
            "   a) TIME node (search 'Time')\n" +
            "   b) SINE node (search 'Sine')\n" +
            "   c) MULTIPLY node (search 'Multiply')\n" +
            "   d) COLOR node (search 'Color') - set to HDR purple (0.5, 0, 1)\n\n" +
            "5. Connect:\n" +
            "   Time (Time output) -> Sine (In)\n" +
            "   Sine (Out) -> Multiply (A)\n" +
            "   Color (Out) -> Multiply (B)\n" +
            "   Multiply (Out) -> Fragment > Emission\n\n" +
            "6. Set Base Color on Fragment to dark (0.05, 0.02, 0.1)\n" +
            "7. Click 'Save Asset'\n" +
            "8. Right-click the graph > Create > Material\n" +
            "9. Assign to Product_ShaderGraphDiamond\n\n" +
            "This gives you 4 nodes (Time + Sine + Multiply + Color)\n" +
            "connected to produce an animated pulse effect.",
            "OK");
    }
}
