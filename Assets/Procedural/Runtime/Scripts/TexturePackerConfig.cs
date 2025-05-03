using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PRK.Procedural
{
    #if UNITY_EDITOR
    using UnityEditor;
    
    public partial class TexturePackerConfig
    {
        public int atlasSize = 1024;
        public Texture2D[] texturesToPack;
        public Texture2D atlas;
        public Rect[]packedRects;
        [ContextMenu("Pack Textures")]
        void PackIntoAtlas()
        {
           // SetReadWrite(true);
            atlas = new Texture2D(atlasSize, atlasSize);
            packedRects = atlas.PackTextures(texturesToPack, 2, atlasSize);
            string projectPath=System.IO.Directory.GetCurrentDirectory();
            string assetsPath=System.IO.Path.Combine(projectPath, "Assets");
            string texturePackPath=System.IO.Path.Combine(assetsPath, "PackedTex.png");
            atlas.Apply();
            System.IO.File.WriteAllBytes(texturePackPath, atlas.EncodeToPNG());
           // SetReadWrite(false);
           AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            atlas=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/PackedTex.png");
            AssetDatabase.Refresh();
        }

        [ContextMenu("Toggle ReadWrite")]
        void SetReadWrite()
        {
            SetReadWrite(!texturesToPack[0].isReadable);
        }
        private void SetReadWrite(bool isReadable_)
        {
            for (int i = 0; i < texturesToPack.Length; i++)
            {
                if (texturesToPack[i].isReadable != isReadable_)
                {
                    string pth=AssetDatabase.GetAssetPath(texturesToPack[i]);
                    TextureImporter t= AssetImporter.GetAtPath(pth) as TextureImporter;
                    t.isReadable = isReadable_;
                   // TextureImporterPlatformSettings platformSettings = t.GetPlatformTextureSettings();
                   // platformSettings.format = TextureImporterFormat.ASTC_6x6;
                   // platformSettings.overridden = true;
                   // t.SetPlatformTextureSettings(platformSettings);
                    t.SaveAndReimport();
                }
            }
            AssetDatabase.Refresh();
        }
    }
    
    #endif
    [CreateAssetMenu]
    public partial class TexturePackerConfig : ScriptableObject
    {
        
    }
}
