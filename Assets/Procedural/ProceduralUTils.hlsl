void Set_Data(uint Instance_ID_)
{
    batch_ID=_InstanceDataArray[Instance_ID_].Batch_Index;
    transform_ID=_InstanceDataArray[Instance_ID_].Transform_Index;
    lightmap_ID=_InstanceDataArray[Instance_ID_].Lightmap_Index;
}
void Set_UnityPerMaterialBufferData()
{
    prk_scaleOffset_Main=_BaseMap_ST;
    prk_DetailAlbedoMap_ST=_DetailAlbedoMap_ST;
    prk_BaseColor=_BaseColor;
    prk_SpecColor=_SpecColor;
    prk_EmissionColor=_EmissionColor;
    prk_Cutoff=_Cutoff;
    prk_Smoothness=_Smoothness;
    prk_Metallic=_Metallic;
    prk_BumpScale=_BumpScale;
    prk_Parallax=_Parallax;
    prk_OcclusionStrength=_OcclusionStrength;
    prk_ClearCoatMask=_ClearCoatMask;
    prk_ClearCoatSmoothness=_ClearCoatSmoothness;
    prk_DetailAlbedoMapScale=_DetailAlbedoMapScale;
    prk_DetailNormalMapScale=_DetailNormalMapScale;
    prk_Surface=_Surface;
}
void MainUV(float2 uv,inout float2 mainuv)
{
    mainuv=uv*_BaseMap_ST.xy+_BaseMap_ST.zw;
}
float3 Transform_ObjectToWorldDir(float3 dirOS, bool doNormalize = true)
{
    float3 dirWS = mul((float3x3)unity_ObjectToWorld, dirOS);   
    if (doNormalize)
        return SafeNormalize(dirWS);
    return dirWS;
}
float3 Transform_ObjectToWorldNormal(float3 normalOS, bool doNormalize = true)
{
    #ifdef UNITY_ASSUME_UNIFORM_SCALING
    return Transform_ObjectToWorldDir(normalOS, doNormalize);
    #else
   
    float3 normalWS = mul(normalOS, (float3x3)unity_WorldToObject);
    if (doNormalize)
        return SafeNormalize(normalWS);
    return normalWS;
    #endif
}
float3 Transform_WorldToView(float3 positionWS)
{
    return mul(UNITY_MATRIX_V, float4(positionWS, 1.0)).xyz;
}
float4 Transform_WorldToHClip(float3 positionWS)
{
    return mul(UNITY_MATRIX_VP, float4(positionWS, 1.0));
}

VertexPositionInputs Get_VertexPositionInputs(float3 positionOS)
{
    VertexPositionInputs input;
    input.positionWS = mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;// TransformObjectToWorld(positionOS);
    input.positionVS =mul(UNITY_MATRIX_V, float4(input.positionWS, 1.0)).xyz;// Transform_WorldToView(input.positionWS);
    input.positionCS =mul(UNITY_MATRIX_VP, float4(input.positionWS, 1.0));// TransformWorldToHClip(input.positionWS);

    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;

    return input;
}

VertexNormalInputs Get_VertexNormalInputs(float3 normalOS, float4 tangentOS)
{
    VertexNormalInputs tbn;
    real sign = real(tangentOS.w) * GetOddNegativeScale();
    tbn.normalWS = Transform_ObjectToWorldNormal(normalOS);
    tbn.tangentWS = real3(Transform_ObjectToWorldDir(tangentOS.xyz));
    tbn.bitangentWS = real3(cross(tbn.normalWS, float3(tbn.tangentWS))) * sign;
    return tbn;
}

half3 Vertex_Lighting(float3 positionWS, half3 normalWS)
{
    half3 vertexLightColor = half3(0.0, 0.0, 0.0);

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
    uint lightsCount = GetAdditionalLightsCount();
    uint meshRenderingLayers = GetMeshRenderingLayer();

    LIGHT_LOOP_BEGIN(lightsCount)
        Light light = GetAdditionalLight(lightIndex, positionWS);

    #ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        #endif
    {
        half3 lightColor = light.color * light.distanceAttenuation;
        vertexLightColor += LightingLambert(lightColor, light.direction, normalWS);
    }

    LIGHT_LOOP_END
#endif

    return vertexLightColor;
}

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