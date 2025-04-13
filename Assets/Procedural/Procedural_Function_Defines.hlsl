#include "Assets/Procedural/Procedural_Defines.hlsl"



void Transform_UVs0(float2 uv,out float2 uv0,uint i_ID_)
{
    InstanceInfo_Data i_info=_InstanceInfo_Buffer[i_ID_];
    float4 t=_ScaleOffset_Main_Buffer[i_info.Batch_ID];
    uv0=uv*t.xy+t.zw;
}

void Transform_LightmapUVs(float2 uv,out float2 uv0,uint i_ID_)
{
    InstanceInfo_Data i_info=_InstanceInfo_Buffer[i_ID_];
    float4 t=_ScaleOffset_LightMap_Buffer[i_info.Lightmap_ID];
    uv0=uv*t.xy+t.zw;
}

half3 Scale_DetailAlbedo(half3 detailAlbedo, half scale)
{    
    return half(2.0) * detailAlbedo * scale - scale + half(1.0);
}

float3 Transform_ObjectTo_WorldDir(float3 dirOS, bool doNormalize = true)
{
    float3 dirWS = mul((float3x3)unity_ObjectToWorld, dirOS);   
    if (doNormalize)
        return SafeNormalize(dirWS);
    return dirWS;
}
float3 Transform_ObjectTo_WorldNormal(float3 normalOS, bool doNormalize = true)
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
float3 Transform_WorldTo_View(float3 positionWS)
{
    return mul(UNITY_MATRIX_V, float4(positionWS, 1.0)).xyz;
}
float4 Transform_WorldTo_HClip(float3 positionWS)
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
    tbn.normalWS = Transform_ObjectTo_WorldNormal(normalOS);
    tbn.tangentWS = real3(Transform_ObjectTo_WorldDir(tangentOS.xyz));
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

half3 Alpha_Modulate(half3 albedo, half alpha)
{
    #if defined(_ALPHAMODULATE_ON)
    return lerp(half3(1.0, 1.0, 1.0), albedo, alpha);
    #else
    return albedo;
    #endif
}

half2 SMPL_ClearCoat(float2 uv)
{
    return half2(0.0, 1.0);
}

half3 SMPL_Emission(int b_ID_,float2 uv, half3 emissionColor)
{
    #ifndef _EMISSION
    return 0;
    #else
   // return SAMPLE_TEXTURE2D(emissionMap, sampler_emissionMap, uv).rgb * emissionColor;
    return SAMPLE_TEXTURE2D_ARRAY(EmissionTex_Array, sampler_EmissionTex_Array, uv,_BatchInfo_Buffer[b_ID_].EmissionTex_ID).rgb * emissionColor;
    #endif
}


half4 SMPL_AlbedoAlpha(int b_ID_,float2 uv)
{
 //   return (half4)0;
   return SAMPLE_TEXTURE2D_ARRAY(MainTex_Array,sampler_MainTex_Array,uv,_BatchInfo_Buffer[b_ID_].MainTex_ID);
   // int sliceid=_BatchInfo_Buffer[i_ID_].Batch_ID;
  //  return half4(SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv));
}

half3 SMPL_Normal(int b_ID_,float2 uv, half scale = half(1.0))
{
    #ifdef _NORMALMAP
    //half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
    half4 n = SAMPLE_TEXTURE2D_ARRAY(NormalMap_Array,sampler_NormalMapArray,uv,_BatchInfo_Buffer[b_ID_].NormalTex_ID);
    #if BUMP_SCALE_NOT_SUPPORTED
    return UnpackNormal(n);
    #else
    return UnpackNormalScale(n, scale);
    #endif
    #else
    return half3(0.0h, 0.0h, 1.0h);
    #endif
}

half SMPL_Occlusion(int b_ID_,float2 uv)
{
    #ifdef _OCCLUSIONMAP
   // half occ = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
    half occ = SAMPLE_TEXTURE2D_ARRAY(OcculusionMap_Array, sampler_OcculusionMap_Array, uv,_BatchInfo_Buffer[b_ID_].OcculusionMap_ID).g;
    return LerpWhiteTo(occ, _OcclusionStrength);
    #else
    return half(1.0);
    #endif
}

half4 SMPL_MetallicSpecGloss(int b_ID_,float2 uv, half albedoAlpha,in FragmentData fragData_)
{
    half4 specGloss;

    #ifdef _METALLICSPECGLOSSMAP
    specGloss = half4(SAMPLE_METALLICSPECULAR(uv));
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    specGloss.a = albedoAlpha * fragData_._Smoothness;
    #else
    specGloss.a *= fragData_._Smoothness;
    #endif
    #else // _METALLICSPECGLOSSMAP
    #if _SPECULAR_SETUP
    specGloss.rgb = fragData_._SpecColor.rgb;
    #else
    specGloss.rgb = fragData_._Metallic.rrr;
    #endif

    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    specGloss.a = albedoAlpha * fragData_._Smoothness;
    #else
    specGloss.a = fragData_._Smoothness;
    #endif
    #endif

    return specGloss;
}

half3 Apply_DetailAlbedo(int b_ID_,float2 detailUv, half3 albedo, half detailMask)
{
    #if defined(_DETAIL)
    half3 detailAlbedo = SAMPLE_TEXTURE2D_ARRAY(DetailTex_Array, sampler_DetailTexArray, detailUv,_BatchInfo_Buffer[b_ID_].DetailTex_ID).rgb;

    // In order to have same performance as builtin, we do scaling only if scale is not 1.0 (Scaled version has 6 additional instructions)
    #if defined(_DETAIL_SCALED)
    detailAlbedo = Scale_DetailAlbedo(detailAlbedo, _DetailAlbedoMapScale);
    #else
    detailAlbedo = half(2.0) * detailAlbedo;
    #endif

    return albedo * LerpWhiteTo(detailAlbedo, detailMask);
    #else
    return albedo;
    #endif
}



half3 Apply_DetailNormal(int b_ID_,float2 detailUv, half3 normalTS, half detailMask)
{
    #if defined(_DETAIL)
        #if BUMP_SCALE_NOT_SUPPORTED
            half3 detailNormalTS = UnpackNormal(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv));
        #else

        half3 detailNormalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv), _DetailNormalMapScale);
    #endif

    detailNormalTS = normalize(detailNormalTS);

    return lerp(normalTS, BlendNormalRNM(normalTS, detailNormalTS), detailMask); // todo: detailMask should lerp the angle of the quaternion rotation, not the normals
    #else
        return normalTS;
    #endif
}