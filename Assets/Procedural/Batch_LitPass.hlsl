#include "Assets/Procedural/UTils.hlsl"

half4 SMPL_AlbedoAlpha(in FragmentData fragData_,in float2 uv)
{
    int slice=_Batch_Info_IDs0[fragData_.batch_ID].x;
    half4 clr=slice<0?(half4)0:SMPL_Any(slice,uv,MainTex_Array,sampler_MainTex_Array);
    return clr;
}
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

FragmentData GetFragmentData(in Varyings input)
{
    FragmentData dt_=(FragmentData)0;
    dt_.instance_ID=(uint)input.instance_IDs.x;
    dt_.batch_ID=input.instance_IDs.y;
    dt_._Surface=2.0;
    dt_._Metallic=.5;
    dt_._Smoothness=.5;
    return dt_;
}


void SetUp_InstancingData(uint i_id_,inout Varyings output)
{
    output.instance_IDs.x=i_id_;
    output.instance_IDs.y=_InstanceInfo_IDs[i_id_].y;
    output.instance_IDs.z=_InstanceInfo_IDs[i_id_].x;
    output.instance_IDs.w=_InstanceInfo_IDs[i_id_].w;    
}


Varyings BatchLitPass_Vertex(Attributes input, uint instance_ID:SV_InstanceID)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    SetUp_InstancingData(instance_ID,output);
    VertexPositionInputs vertexInput = Get_VertexPosition_Inputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = Get_VertexNormal_Inputs(input.normalOS, input.tangentOS);
    half3 vertexLight = Vertex_Lighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = 0;
    TRSm_UVs0(input.texcoord,output.instance_IDs,output.uv);
    output.normalWS = normalInput.normalWS;

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
    output.fogFactor = fogFactor;
    output.positionCS = vertexInput.positionCS;
    return output;
}

half4 BatchLitPass__Fragment(Varyings input):SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    FragmentData fragdata=GetFragmentData(input);
    SurfaceData surfaceData;
    Initialize_StandardLitSurfaceData(input.uv, surfaceData,fragdata);

    InputData inputData;
    Initialize_InputData(input, surfaceData.normalTS, inputData);
    half4 color = UniversalFragmentPBR(inputData, surfaceData);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a =OutputAlpha(color.a, IsSurfaceTypeTransparent(fragdata._Surface));
    
    return color;
}