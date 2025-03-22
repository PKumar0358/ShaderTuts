#include "Assets/ShaderTuts/MyShaderUtils.hlsl"
#include "Assets/ShaderTuts/Procedural_UtilityFunctions.hlsl"


Varyings v2_f(Attributes input)
{
    Varyings output = (Varyings)0;
    
    return output;
}

half4 frag(Varyings input):SV_Target
{
    return  half4(1,0,0,1);
}