#include "Assets/Procedural/ProceduralBatch_Utilities.hlsl"


//............................................................................................
FragmentData GetFragmentData(in Varyings input)
{
    FragmentData data=(FragmentData)0;
    data.instance_ID=(uint)input.instance_IDs.x;
    data.batch_ID=input.instance_IDs.y;
    data._Surface=2.0;
    data._Metallic=.5;
    data._Smoothness=.5;
    return data;
}

//............................................................................................
half4 SMPL_AlbedoAlpha(in FragmentData fragData_,in float2 uv)
{
   int slice=_BatchInfo_IDs0[fragData_.batch_ID].x;
    half4 clr=slice<0?(half4)0:SMPL_Any(slice,uv,MainTex_Array,sampler_MainTex_Array);
    return clr;
}
//............................................................................................
half3 Alpha_Modulate(half3 albedo, half alpha)
{
    #if defined(_ALPHAMODULATE_ON)
    return lerp(half3(1.0, 1.0, 1.0), albedo, alpha);
    #else
    return albedo;
    #endif
}
//............................................................................................

half4 SMPL_MetallicSpecGloss(float2 uv, half albedoAlpha,in FragmentData dt_)
{
    half4 specGloss;

    #ifdef _METALLICSPECGLOSSMAP
    specGloss = half4(SAMPLE_METALLICSPECULAR(uv));
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    specGloss.a = albedoAlpha * _Smoothness;
    #else
    specGloss.a *= _Smoothness;
    #endif
    #else // _METALLICSPECGLOSSMAP
    #if _SPECULAR_SETUP
    specGloss.rgb = _SpecColor.rgb;
    #else
    specGloss.rgb = dt_._Metallic.rrr;
    #endif

    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    specGloss.a = albedoAlpha * _Smoothness;
    #else
    specGloss.a = dt_._Smoothness;
    #endif
    #endif

    return specGloss;
}
inline void Initialize_StandardLitSurfaceData(float2 uv, inout SurfaceData outSurfaceData,in FragmentData fragData_)
{
    half4 albedoAlpha = SMPL_AlbedoAlpha(fragData_,uv);
    outSurfaceData.alpha =albedoAlpha.a;// Alpha(albedoAlpha.a, fragData_._BaseColor, fragData_._Cutoff);
    
    half4 specGloss = SMPL_MetallicSpecGloss(uv, albedoAlpha.a,fragData_);
    outSurfaceData.albedo = albedoAlpha.rgb * fragData_._BaseColor.rgb;
 //   outSurfaceData.albedo = Alpha_Modulate(outSurfaceData.albedo, outSurfaceData.alpha);

    #if _SPECULAR_SETUP
    outSurfaceData.metallic = half(1.0);
    outSurfaceData.specular = specGloss.rgb;
    #else
    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = half3(0.0, 0.0, 0.0);
    #endif

    outSurfaceData.occlusion= half(1.0);
    outSurfaceData.emission=(half3)0;
    outSurfaceData.smoothness = specGloss.a;
    outSurfaceData.normalTS =half3(0.0h, 0.0h, 1.0h);//no normal in this variant SMPL_Normal(fragData_.batch_ID,uv, fragData_._BumpScale);
   
    outSurfaceData.clearCoatMask       = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);
    
   
}

//............................................................................................//////////////////////////////
void Initialize_InputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
    #endif

    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    
    /*#if defined(_NORMALMAP) || defined(_DETAIL)
        float sgn = input.tangentWS.w;      // should be either +1 or -1
        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);

            #if defined(_NORMALMAP)
            inputData.tangentToWorld = tangentToWorld;
            #endif
        inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
    #else
    inputData.normalWS = input.normalWS;
    #endif*/

    inputData.normalWS = input.normalWS;
    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;


    inputData.shadowCoord = float4(0, 0, 0, 0);
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor);
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);   
}


//............................................................................................//////////////////////////////



