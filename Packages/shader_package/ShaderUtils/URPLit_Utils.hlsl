#include "Packages/com.prk.procedural.experimental/ShaderUtils/URPLitInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

float3 Calculate_GI(half4 lightmap_,half4 dirmap_,inout half3 normalWS)
{
    half4 decodeInstructions = half4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h);
    real4 direction = dirmap_;
    real4 encodedIlluminance = lightmap_.rgba;
    real3 illuminance = DecodeLightmap(encodedIlluminance, decodeInstructions);
    real halfLambert = dot(normalWS, direction.xyz - 0.5) + 0.5;
    float3 bakedGI_ = illuminance * halfLambert / max(1e-4, direction.w);
    return bakedGI_;
}


void Sample_LMaps(float2 transformed_uv,inout half3 normal_ws,inout float3 bakedgi)
{
    half4 lightmap_= SAMPLE_TEXTURE2D(_LightMap,sampler_LightMap,transformed_uv);
    half4 dirmap_= SAMPLE_TEXTURE2D(_LightMap,sampler_LightMap,transformed_uv);   
  //  #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
   // int slice_=_Instance_Data_Buffer[unity_InstanceID].lightmapIndex;
  //  half4 lightmap_= SAMPLE_TEXTURE2D_ARRAY(_LightMaps,sampler_LightMaps,transformed_uv,slice_);
   // half4 dirmap_= SAMPLE_TEXTURE2D_ARRAY(_LightMaps,sampler_LightMaps,transformed_uv,slice_);    
    bakedgi=Calculate_GI(lightmap_,dirmap_,normal_ws);   
  //  #endif
}