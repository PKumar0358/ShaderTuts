#include "Packages/com.prk.procedural.experimental/ShaderUtils/TestLit_Structs_AndDeclarations.hlsl"
#include "Packages/com.prk.procedural.experimental/ShaderUtils/TestLitUTils.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// GLES2 has limited amount of interpolators
#if defined(_PARALLAXMAP) && !defined(SHADER_API_GLES)
#define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
#endif

#if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif




Varyings TestLitPassVertex(Attributes input,uint id_:SV_InstanceID)
{
    Varyings output = (Varyings)0;
    VertexPositionInputs vertexInput =(VertexPositionInputs)0;
    VertexNormalInputs normalInput = (VertexNormalInputs)0;
    VertexPassDataSetup(input,output,vertexInput,normalInput);

    half fogFactor = 0;
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.normalWS = normalInput.normalWS;
    
    #if defined(_USE_CUSTOM_LIGHTMAPS)&& defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    float4 tr=_Instance_Data_Buffer[id_].lightmapScaleOffset;
    output.lightmap_uv=input.staticLightmapUV*tr.xy+tr.zw;
    #endif//..
    
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
#endif//
    
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    output.tangentWS = tangentWS;
#endif//

    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
        half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
        half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
        output.viewDirTS = viewDirTS;
    #endif//
    
    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
    output.fogFactor = fogFactor;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    output.positionWS = vertexInput.positionWS;
#endif//
    output.positionCS = vertexInput.positionCS;
    return output;
}

half4 TestLitPassFragment(Varyings input):SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    #if defined(_USE_CUSTOM_LIGHTMAPS_ARRAY)
    return half4(1,0,0,1);
    #endif
    SurfaceData surfaceData;
    Initialize_StandardLitSurfaceData(input.uv, surfaceData);
    InputData inputData;
    Initialize_InputData(input, surfaceData.normalTS, inputData);
    half4 color = UniversalFragmentPBR(inputData, surfaceData);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));
    return color;
}

