Shader "Custom/Procedural_URPLit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        Pass
        {
             HLSLPROGRAM
             #pragma vertex v2_f
             #pragma fragment frag
            
            #pragma multi_compile_instancing
             
          //  #pragma instancing_options procedural:InstancingSetup
            #include "Assets/ShaderTuts/Procedural_Lit_URP.hlsl"

            ENDHLSL
        }
    }
}
