Shader "Custom/URPTestLit"
{
    Properties
    {
                
        _BaseMapArray("MapArray", 2DArray) = "" {}
        [MainTexture] _BaseMap("Base Map (RGB) Smoothness / Alpha (A)", 2D) = "white" {}
        [MainColor]   _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _SpecColor("Specular Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _SmoothnessSource("Smoothness Source", Float) = 0.0
        _index("Index", Range(0, 100)) = 0.5
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        Pass
        {
             Tags
            {
                "LightMode" = "UniversalForward"
            }
             
        
            HLSLPROGRAM
            #pragma vertex URPTestLit_VertexFunction
            #pragma fragment URPTestLit_FragmentFunction
            #include "Assets/ShaderTuts/Runtime/ShaderLab/MyHLSL.hlsl"
            ENDHLSL
        }
    }
}
