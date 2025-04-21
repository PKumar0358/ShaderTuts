#include "Assets/Procedural/Batch_Defines.hlsl"

void TRSm_UVs0(in float2 texcord0,in int4 instance_IDs_,out float2 uv)
{
    uv=texcord0*_ScaleOffsets_Buffer[instance_IDs_.w].xy+_ScaleOffsets_Buffer[instance_IDs_.w].zw;
}
half3 Scale_DetailAlbedo(half3 detailAlbedo, half scale)
{    
    return half(2.0) * detailAlbedo * scale - scale + half(1.0);
}

float3 TRSm_ObjectTo_WorldDir(float3 dirOS, bool doNormalize = true)
{
    float3 dirWS = mul((float3x3)unity_ObjectToWorld, dirOS);   
    if (doNormalize)
        return SafeNormalize(dirWS);
    return dirWS;
}

float3 TRSm_ObjectTo_WorldNormal(float3 normalOS, bool doNormalize = true)
{
    #ifdef UNITY_ASSUME_UNIFORM_SCALING
    return Transform_ObjectToWorldDir(normalOS, doNormalize);
    #else
   
    float3 normalWS = mul(normalOS, (float3x3)unity_WorldToObject);
    if (doNormalize)
        return SafeNormalize(normalWS);
    return normalWS;
    #endif
}

float3 TRSm_WorldTo_View(float3 positionWS)
{
    return mul(UNITY_MATRIX_V, float4(positionWS, 1.0)).xyz;
}

float4 TRSm_WorldTo_HClip(float3 positionWS)
{
    return mul(UNITY_MATRIX_VP, float4(positionWS, 1.0));
}

VertexPositionInputs Get_VertexPosition_Inputs(float3 positionOS)
{
    VertexPositionInputs input;
    input.positionWS = mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;// TransformObjectToWorld(positionOS);
    input.positionVS =mul(UNITY_MATRIX_V, float4(input.positionWS, 1.0)).xyz;// Transform_WorldToView(input.positionWS);
    input.positionCS =mul(UNITY_MATRIX_VP, float4(input.positionWS, 1.0));// TransformWorldToHClip(input.positionWS);

    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;
    return input;
}

VertexNormalInputs Get_VertexNormal_Inputs(float3 normalOS, float4 tangentOS)
{
    VertexNormalInputs tbn;
    real sign = real(tangentOS.w) * GetOddNegativeScale();
    tbn.normalWS = TRSm_ObjectTo_WorldNormal(normalOS);
    tbn.tangentWS = real3(TRSm_ObjectTo_WorldDir(tangentOS.xyz));
    tbn.bitangentWS = real3(cross(tbn.normalWS, float3(tbn.tangentWS))) * sign;
    return tbn;
}

half3 Vertex_Lighting(float3 positionWS, half3 normalWS)
{
    half3 vertexLightColor = half3(0.0, 0.0, 0.0);

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
    uint lightsCount = GetAdditionalLightsCount();
    uint meshRenderingLayers = GetMeshRenderingLayer();

    LIGHT_LOOP_BEGIN(lightsCount)
        Light light = GetAdditionalLight(lightIndex, positionWS);

    #ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        #endif
    {
        half3 lightColor = light.color * light.distanceAttenuation;
        vertexLightColor += LightingLambert(lightColor, light.direction, normalWS);
    }

    LIGHT_LOOP_END
#endif

    return vertexLightColor;
}

half4 SMPL_Any(in int slice_,in float2 uv,TEXTURE2D_ARRAY_PARAM(array_Name,sampler_Name))
{
    return SAMPLE_TEXTURE2D_ARRAY(array_Name,sampler_Name,uv,slice_);
}