#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif
#if defined(_DETAIL_MULX2) || defined(_DETAIL_SCALED)
#define _DETAIL
#endif

struct InstanceData
{
    int Batch_Index;
    int Transform_Index;
    int Lightmap_Index;  
};

struct BatchData
{
    float4 MainScaleOffset;
    float4 DetailScaleOffset;    
};

struct TransformData
{
    float4x4 _ObjToWorld;
    float4x4 _WorldToObj;
};
StructuredBuffer<InstanceData> _InstanceDataArray;
StructuredBuffer<TransformData>_TransformDataArray;
StructuredBuffer<float4>_LightmapScaleOffet;
struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 staticLightmapUV   : TEXCOORD1;
    float2 dynamicLightmapUV  : TEXCOORD2;    
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    float3 positionWS               : TEXCOORD1;
    #endif

    float3 normalWS                 : TEXCOORD2;
    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    half4 tangentWS                : TEXCOORD3;    // xyz: tangent, w: sign
    #endif

    half  fogFactor                 : TEXCOORD5;

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD6;
    #endif

    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS                : TEXCOORD7;
    #endif

    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);  
    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

static int batch_ID;
static int transform_ID;
static int lightmap_ID;

static float4 prk_scaleOffset_Main;
static float4 prk_DetailAlbedoMap_ST;
static half4 prk_BaseColor;
static half4 prk_SpecColor;
static half4 prk_EmissionColor;
static half prk_Cutoff;
static half prk_Smoothness;
static half prk_Metallic;
static half prk_BumpScale;
static half prk_Parallax;
static half prk_OcclusionStrength;
static half prk_ClearCoatMask;
static half prk_ClearCoatSmoothness;
static half prk_DetailAlbedoMapScale;
static half prk_DetailNormalMapScale;
static half prk_Surface;

CBUFFER_START(PerDrawData)  
float4 colortest;
CBUFFER_END

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float4 _DetailAlbedoMap_ST;
half4 _BaseColor;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;
half _Smoothness;
half _Metallic;
half _BumpScale;
half _Parallax;
half _OcclusionStrength;
half _ClearCoatMask;
half _ClearCoatSmoothness;
half _DetailAlbedoMapScale;
half _DetailNormalMapScale;
half _Surface;
CBUFFER_END

TEXTURE2D_ARRAY(_BaseMapArray);             SAMPLER(sampler_BaseMapArray);
TEXTURE2D_ARRAY(_DetailAlbedoMapArray);     SAMPLER(sampler_DetailAlbedoMapArray);
TEXTURE2D_ARRAY(_DetailNormalMapArray);     SAMPLER(sampler_DetailNormalMapArray);

TEXTURE2D(_DetailMask);         SAMPLER(sampler_DetailMask);
TEXTURE2D(_DetailAlbedoMap);    SAMPLER(sampler_DetailAlbedoMap);
TEXTURE2D(_DetailNormalMap);    SAMPLER(sampler_DetailNormalMap);
TEXTURE2D(_MetallicGlossMap);   SAMPLER(sampler_MetallicGlossMap);
TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);

#ifdef _SPECULAR_SETUP
    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv)
#else
    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)
#endif
