#include "Assets/Procedural/Procedural_Function_Defines.hlsl"
//............................................................................................

inline void Initialize_StandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData,in FragmentData fragData_)
{
    half4 albedoAlpha = SMPL_AlbedoAlpha(fragData_.batch_ID,uv);
    outSurfaceData.alpha = Alpha(albedoAlpha.a, fragData_._BaseColor, fragData_._Cutoff);
    
    half4 specGloss = SMPL_MetallicSpecGloss(fragData_.instance_ID,uv, albedoAlpha.a,fragData_);
    outSurfaceData.albedo = albedoAlpha.rgb * fragData_._BaseColor.rgb;
    outSurfaceData.albedo = Alpha_Modulate(outSurfaceData.albedo, outSurfaceData.alpha);

    #if _SPECULAR_SETUP
    outSurfaceData.metallic = half(1.0);
    outSurfaceData.specular = specGloss.rgb;
    #else
    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = half3(0.0, 0.0, 0.0);
    #endif

    outSurfaceData.smoothness = specGloss.a;
    outSurfaceData.normalTS = SMPL_Normal(fragData_.batch_ID,uv, _BumpScale);
    outSurfaceData.occlusion = SMPL_Occlusion(fragData_.batch_ID,uv);
    outSurfaceData.emission = SMPL_Emission(fragData_.instance_ID,uv, fragData_._EmissionColor.rgb);

    outSurfaceData.clearCoatMask       = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);
    
    #if defined(_DETAIL)
    half detailMask =0;// SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
    float2 detailUv = uv * fragData_._DetailAlbedoMap_ST.xy + fragData_._DetailAlbedoMap_ST.zw;
    outSurfaceData.albedo = Apply_DetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
    outSurfaceData.normalTS = Apply_DetailNormal(detailUv, outSurfaceData.normalTS, detailMask);
    #endif
}

//............................................................................................//////////////////////////////
void Initialize_InputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
    #endif

    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    #if defined(_NORMALMAP) || defined(_DETAIL)
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);

    #if defined(_NORMALMAP)
    inputData.tangentToWorld = tangentToWorld;
    #endif
    inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
    #else
    inputData.normalWS = input.normalWS;
    #endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;


    inputData.shadowCoord = float4(0, 0, 0, 0);
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor);
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);   
}


//............................................................................................//////////////////////////////
FragmentData GetFragmentData(in Varyings input)
{
    FragmentData data;
    data.instance_ID=input.instance_ID;
    data.batch_ID=_InstanceInfo_Buffer[input.instance_ID].Batch_ID;
    data._BaseMap_ST=_BaseMap_ST;
    data._DetailAlbedoMap_ST=_DetailAlbedoMap_ST;
    data._BaseColor=_BaseColor;
    data._SpecColor=_SpecColor;
    data._EmissionColor=_EmissionColor;
    data._Cutoff=_Cutoff;
    data._Smoothness=_Smoothness;
    data._Metallic=_Metallic;
    data._BumpScale=_BumpScale;
    data._DetailAlbedoMapScale=_DetailAlbedoMapScale;
    data._DetailNormalMapScale=_DetailNormalMapScale;
    data._Surface=_Surface;
    return data;
}

Varyings ProceduralBatch_Vertex(Attributes input,uint id_:SV_InstanceID)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    output.instance_ID = id_;
  // Set_UnityPerMaterialBufferData();
    
    VertexPositionInputs vertexInput = Get_VertexPosition_Inputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = Get_VertexNormal_Inputs(input.normalOS, input.tangentOS);
    half3 vertexLight = Vertex_Lighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = 0;
    Transform_UVs0(input.texcoord,output.uv,id_);
    output.normalWS = normalInput.normalWS;

    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
    #endif
    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    output.tangentWS = tangentWS;
    #endif
    
    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
    half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
    output.viewDirTS = viewDirTS;
    #endif

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
    output.fogFactor = fogFactor;

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    output.positionWS = vertexInput.positionWS;
    #endif

    output.positionCS = vertexInput.positionCS;
    
    return output;
}

half4 ProceduralBatch_Fragment(Varyings input):SV_Target
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
    color.a =OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));
    
    return color;
}