#include "Assets/Procedural/ProceduralLitBatch_VARIANT_0.hlsl"
//............................................................................................

void SetUp_InstancingData(uint i_id_,inout Varyings output)
{
    output.instance_IDs.x=i_id_;
    output.instance_IDs.y=_InstanceInfo_IDs[i_id_].y;
    output.instance_IDs.z=_InstanceInfo_IDs[i_id_].x;
    output.instance_IDs.w=_InstanceInfo_IDs[i_id_].w;    
}

Varyings ProceduralLitBatch_0_Vertex(Attributes input,uint id_:SV_InstanceID)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    SetUp_InstancingData(id_,output);
  // Set_UnityPerMaterialBufferData();
    
    VertexPositionInputs vertexInput = Get_VertexPosition_Inputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = Get_VertexNormal_Inputs(input.normalOS, input.tangentOS);
    half3 vertexLight = Vertex_Lighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = 0;
    TRSm_UVs0(input.texcoord,output.uv,id_);
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
    int lampid=_InstanceInfo_IDs[id_].z;
    output.lightMapUVs=lampid<0?(float2)0:(input.staticLightmapUV*_ScaleOffset_LightMap_Buffer[lampid].xy+_ScaleOffset_LightMap_Buffer[lampid].zw);
    output.positionCS = vertexInput.positionCS;
    return output;
}

half4 ProceduralLitBatch_0_Fragment(Varyings input):SV_Target
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