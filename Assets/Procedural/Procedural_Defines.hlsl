#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

//_IsBatchRenderer
StructuredBuffer<float4x4>_ObjTo_WorldMatrix;
StructuredBuffer<float4x4>_WorldTo_ObjMatrix;
StructuredBuffer<float4>_MainColor_Buffer;
StructuredBuffer<float4>_SpecColor_Buffer;
StructuredBuffer<float4>_EmissionColor_Buffer;
StructuredBuffer<float4>_ScaleOffset_Main_Buffer;
StructuredBuffer<float4>_ScaleOffset_Detail_Buffer;
StructuredBuffer<float4>_ScaleOffset_LightMap_Buffer;
StructuredBuffer<float4>PackedData_0;
StructuredBuffer<float4>PackedData_1;
StructuredBuffer<float4>PackedData_2;

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
    float2 lightMapUVs              : TEXCOORD4;
    half  fogFactor                 : TEXCOORD5;

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD6;
    #endif

    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS                : TEXCOORD7;
    #endif

    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);
    nointerpolation  uint instance_ID:TEXCOORD9;
    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

TEXTURE2D_ARRAY(MainTex_Array);  SAMPLER(sampler_MainTex_Array);
TEXTURE2D_ARRAY(EmissionTex_Array);  SAMPLER(sampler_EmissionTex_Array);
TEXTURE2D_ARRAY(DetailTex_Array);  SAMPLER(sampler_DetailTexArray);
TEXTURE2D_ARRAY(NormalMap_Array);  SAMPLER(sampler_NormalMapArray);
TEXTURE2D_ARRAY(DetailMap_Array);  SAMPLER(sampler_DetailMapArray);
TEXTURE2D_ARRAY(OcculusionMap_Array);  SAMPLER(sampler_OcculusionMap_Array);

/*CBUFFER_START(UnityPerMaterial)
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
CBUFFER_END*/

struct FragmentData
{
    uint instance_ID;
    int batch_ID;
    half4 _BaseColor;
    half4 _SpecColor;
    half4 _EmissionColor;
    half _Cutoff;//PackedData_0.x
    half _Smoothness;//PackedData_0.y
    half _Metallic;//PackedData_0.z
    half _BumpScale;//PackedData_0.w
    half _DetailAlbedoMapScale;//PackedData_1.x
    half _DetailNormalMapScale;//PackedData_1.y
    half _Surface;//PackedData_1.z
};

struct  InstanceInfo_Data
{
    int Transform_ID;
    int Batch_ID;
    int Lightmap_ID;
};

struct BatchInfo_Data
{
    int MainColor_ID;
    int MainTex_ID;
    int DetailTex_ID;
    int NormalTex_ID;
    int DetailNormalTex_ID;
    int OcculusionMap_ID;
    int EmissionTex_ID;
};

StructuredBuffer<BatchInfo_Data>_BatchInfo_Buffer;
StructuredBuffer<InstanceInfo_Data>_InstanceInfo_Buffer;

