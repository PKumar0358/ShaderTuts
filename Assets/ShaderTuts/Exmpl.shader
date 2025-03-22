Shader "Custom/IndirectURPShader"
{
    Properties
    {
        _BaseMap("Albedo", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Intstance_Data
            {
                int _Lightmap_Index;
                float4 _Lightmap_ScaleOffset;
                float4x4 _ObjectToWorld;
                float4x4 _WorldToObject;
            };
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : NORMAL;
                float2 uv          : TEXCOORD0;
                float2 uv2          : TEXCOORD1;
            };
            TEXTURE2D_ARRAY(_LightMaps);
            SAMPLER(sampler_LightMaps);
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            CBUFFER_END
          //  StructuredBuffer<float4x4>_obj_2_world_matrix;
          //  StructuredBuffer<float4x4>_world_2_obj_matrix;
            StructuredBuffer<Intstance_Data>_IntstanceDataBuffer;
            Varyings vert(Attributes IN,uint instanceID : SV_InstanceID)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                float4x4 modelMatrix =_IntstanceDataBuffer[instanceID]._ObjectToWorld;// UNITY_ACCESS_INSTANCED_PROP(Props, _ObjectToWorld);
                float4 worldPosition = mul(modelMatrix, float4(IN.positionOS, 1.0));
                OUT.positionHCS = TransformWorldToHClip(worldPosition.xyz);
                OUT.normalWS = normalize(mul((float3x3)modelMatrix, IN.normalOS));
                OUT.uv = IN.uv;
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                half4 texcolor=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,IN.uv);
                return texcolor;
                half4 finalcolor=texcolor* _BaseColor;
                half4 lightmapColor = SAMPLE_TEXTURE2D_ARRAY(_LightMaps, sampler_LightMaps, IN.uv, 4);
                return  lightmapColor;
                return finalcolor;
            }
            ENDHLSL
        }
    }
}
