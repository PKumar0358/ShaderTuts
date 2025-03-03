#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Macros.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
TEXTURE2D_ARRAY(_BaseArray);
SAMPLER(sampler_BaseArray);
struct Attributes
{
    float4 positionOS    : POSITION;
    float3 normalOS      : NORMAL;
    float4 tangentOS     : TANGENT;
    float2 texcoord      : TEXCOORD0;
    float2 staticLightmapUV    : TEXCOORD1;
    float2 dynamicLightmapUV    : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;
    float3 positionWS                  : TEXCOORD1;    // xyz: posWS
    float4 positionCS                  : SV_POSITION;
    half3  normalWS                : TEXCOORD2;
    float index      : TEXCOORD3;  // Pass index as a varying
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};


    CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        half4 _BaseColor;
        half _Smoothness;
        half _Metallic;
        half _index;
    CBUFFER_END

Varyings URPTestLit_VertexFunction(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.uv =TRANSFORM_TEX(input.texcoord,_BaseMap);

    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
    output.positionWS = positionInputs.positionWS;
    output.positionCS = positionInputs.positionCS;

    VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    output.normalWS = normalInputs.normalWS;
   
    return output;
}

void URPTestLit_FragmentFunction(Varyings input, out half4 outColor : SV_Target0)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);    

        // Sample base color texture
        half3 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;

        // Normalize world-space normal
        half3 normalWS = normalize(input.normalWS);

        // Get lighting data
        Light mainLight = GetMainLight();
        half3 lightDir = normalize(mainLight.direction);
        half3 lightColor = mainLight.color;

        // Compute lighting using Lambertian shading
        half NdotL = saturate(dot(normalWS, lightDir));

        // Specular reflection
        half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - input.positionWS);
        half3 halfVector = normalize(viewDir + lightDir);
        half NdotH = saturate(dot(normalWS, halfVector));
        half specular = pow(NdotH, _Smoothness * 100) * _Metallic;

        // Combine diffuse and specular lighting
        half3 finalColor = (baseColor * NdotL + specular) * lightColor;
    outColor = half4(finalColor, 1.0);
}


