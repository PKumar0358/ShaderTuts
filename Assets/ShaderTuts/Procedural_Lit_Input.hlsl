#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
//#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED//000000000000000000000000000000000000000
//StructuredBuffer<int>_lightMapIndexes_array;
//StructuredBuffer<float4>_lightmapScaleOffsets_array;
StructuredBuffer<float4x4>_obj_2_world_matrix;
StructuredBuffer<float4x4>_world_2_obj_matrix;

/*#ifdef unity_ObjectToWorld
#undef unity_ObjectToWorld
#endif//-------------------
#ifdef unity_WorldToObject
#undef unity_WorldToObject
#endif//-----------------*/

void InstancingSetup()
{
    /*UNITY_DEFINE_INSTANCED_PROP(float4x4, unity_ObjectToWorld);
    UNITY_DEFINE_INSTANCED_PROP(float4x4, unity_WorldToObject);
    unity_ObjectToWorld=_obj_2_world_matrix_array[unity_InstanceID];
    unity_WorldToObject=_world_2_obj_matrix_array[unity_InstanceID];*/
}

//#endif//====================000000000000000000000000000000000000000000

/*int _lightMapIndex;
float4 _lightmapScaleOffsets;
float4x4 _world_2_obj_matrix;
float4x4 _obj_2_world_matrix;

#define GET_LIGHTMAP_INDEX() _lightMapIndexes
#define GET_LIGHTMAP_SCALE_OFFSETS() _lightmapScaleOffsets
#define GET_WORLD_TO_WORLD_MATRIX() _world_2_world_matrix
#define GET_OBJECT_TO_WORLD_MATRIX() _obj_2_world_matrix

#define LM_ID(_index) _lightMapIndexes_array[_index]
#define LM_SCALE_OFFSET(_index) _lightmapScaleOffsets_array[_index]
#define W2O_MTRx(_index) _world_2_obj_matrix_array[_index]
#define O2W_MTRx(_index) _obj_2_world_matrix_array[_index]*/




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
    float3 positionWS               : TEXCOORD1;   
    float3 normalWS                 : TEXCOORD2;
    half4 tangentWS                : TEXCOORD3;    // xyz: tangent, w: sign
    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};