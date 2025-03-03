Shader "Custom/URP_CustomLit_2"
{
  Properties
  {
    [MainTexture] _BaseMap("Base Map",2D)="white" {}
    [MainColor]_BaseColor("Base Color",Color)=(1,1,1,1)
    _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecGlossMap("Specular", 2D) = "white" {}
    [HideInInspector]_AlphaToMask("__alphaToMask", Float) = 0.0
  }
  SubShader
  {
    Tags
    {
      "RenderType"="Opaque"
      "RenderPipeline"="UniversalPipeline"
    }
    Pass
    {
      Name "ForwardLit"
        Tags
        {
          "LightMode" = "UniversalForward"
        }
        HLSLPROGRAM
         #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

         //   #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
           // #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            //#pragma multi_compile _ DYNAMICLIGHTMAP_ON
         
         #include "Assets/ShaderTuts/Runtime/ShaderLab/URP_Lit_ForwardPass.hlsl"
        ENDHLSL
    }

    Pass
    {
         Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            
         ZWrite On
         ZTest LEqual
         ColorMask 0
         Cull[_Cull]
         
            HLSLPROGRAM

            #pragma target 2.0

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

          
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            
            ENDHLSL
    }
  }
}
