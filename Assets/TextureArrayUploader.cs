using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class TextureArrayUploader : MonoBehaviour
{
    [SerializeField] private bool upload = false;
    [SerializeField] private Texture2D[] lightMaps;
    [SerializeField] private Texture2D[] dirMaps;
    [SerializeField] private Material mat;
    void Update()
    {
        if (upload)
        {
            lightMaps = new Texture2D[LightmapSettings.lightmaps.Length];
            dirMaps = new Texture2D[LightmapSettings.lightmaps.Length];
            
            for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
            {
                lightMaps[i] = LightmapSettings.lightmaps[i].lightmapColor;
                dirMaps[i] = LightmapSettings.lightmaps[i].lightmapDir;
            }
            // mat.EnableKeyword("_USE_CUSTOM_LIGHTMAPS");
            SetTextureArray(lightMaps, mat,"_LightMaps");
            SetTextureArray(dirMaps, mat,"_DirMaps");
        }
    }

   
    private static void SetTextureArray(Texture2D[]texures_,Material targetMaterial_,string arrayName_)
    {
        int depth=texures_.Length;
        Texture2D inTex = texures_[0];
        int width = inTex.width;
        int height = inTex.width;
        Texture2DArray outArray = new Texture2DArray(width, height, depth, inTex.format, true);
        for (int i = 0; i < depth; i++)
        {
            inTex = texures_[i];
            for (int mip = 0; mip < inTex.mipmapCount; ++mip)
            {
                int copyWidth = width >> mip;
                int copyHeight = height >> mip;
                Graphics.CopyTexture(inTex, 0, mip, 0, 0, copyWidth, copyHeight, outArray, i, mip, 0, 0);
            }
        }
        targetMaterial_.SetTexture(arrayName_,outArray);
    }
}
