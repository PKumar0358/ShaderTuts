Shader "Custom/URP_Lightmap_Directional"
{
    Properties
    {
        _BaseMap ("Main Texture", 2D) = "white" {}
        _BaseColor ("Main Color", Color) = (1,1,1,1)
        _LightmapTex ("Lightmap Texture", 2D) = "black" {}
        _DirectionalTex ("Directional Texture", 2D) = "gray" {}
        _LightmapScaleOffset ("Lightmap Scale Offset", Vector) = (1,1,0,0)
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uvLM : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvLM : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);    SAMPLER(sampler_BaseMap);
            TEXTURE2D(_LightmapTex); SAMPLER(sampler_LightmapTex);
            TEXTURE2D(_DirectionalTex); SAMPLER(sampler_DirectionalTex);

            float4 _BaseColor;
            float4 _LightmapScaleOffset;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.uvLM = IN.uvLM * _LightmapScaleOffset.xy + _LightmapScaleOffset.zw;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                float3 lightmap = SAMPLE_TEXTURE2D(_LightmapTex, sampler_LightmapTex, IN.uvLM).rgb;
                float3 directional = SAMPLE_TEXTURE2D(_DirectionalTex, sampler_DirectionalTex, IN.uvLM).rgb;
                return float4(baseColor.rgb * lightmap * directional, baseColor.a);
            }
            ENDHLSL
        }
    }
}
