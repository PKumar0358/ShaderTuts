#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

//_IsBatchRenderer
StructuredBuffer<float4x4>_ObjTo_World_Buffer;
StructuredBuffer<float4x4>_WorldTo_Obj_Buffer;
StructuredBuffer<float4>_ScaleOffsets_Buffer;
StructuredBuffer<float4>_ScaleOffset_LightMap_Buffer;
StructuredBuffer<float4>_Colors_Buffer;
StructuredBuffer<int4>_InstanceInfo_IDs;//x=Transform id, y= batch id, z=light map id.,w=.scale offset main.
StructuredBuffer<int4>_BatchInfo_IDs0;//x=main tex id, y= main color id, z= normal map id, w= normal map scale id 
StructuredBuffer<int4>_BatchInfo_IDs1;//x= detail tex id, y= detail normal map id, z= detail normal map scale id, w= detail scale offset

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
    nointerpolation  int4 instance_IDs:TEXCOORD9;//x=instance id, y=batch id, z=transform id, w = scale offset main id
    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

TEXTURE2D_ARRAY(MainTex_Array);  SAMPLER(sampler_MainTex_Array);
//TEXTURE2D_ARRAY(EmissionTex_Array);  SAMPLER(sampler_EmissionTex_Array);
//TEXTURE2D_ARRAY(DetailTex_Array);  SAMPLER(sampler_DetailTexArray);
//TEXTURE2D_ARRAY(NormalMap_Array);  SAMPLER(sampler_NormalMapArray);
//TEXTURE2D_ARRAY(DetailMap_Array);  SAMPLER(sampler_DetailMapArray);
//TEXTURE2D_ARRAY(OcculusionMap_Array);  SAMPLER(sampler_OcculusionMap_Array);

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
  //  half4 _EmissionColor;
   // half _Cutoff;//PackedData_0.x
    half _Smoothness;//PackedData_0.y
    half _Metallic;//PackedData_0.z
  //  half _BumpScale;//PackedData_0.w
   // half _DetailAlbedoMapScale;//PackedData_1.x
  //  half _DetailNormalMapScale;//PackedData_1.y
    half _Surface;//PackedData_1.z
};



