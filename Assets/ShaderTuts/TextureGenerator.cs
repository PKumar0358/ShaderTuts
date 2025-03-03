using UnityEngine;
using System.IO;

public class TextureGenerator : MonoBehaviour
{
    public int textureWidth = 256;
    public int textureHeight = 256;
    public Material sourceMaterial;// = Color.white;
    
    public void GenerateTexture()
    {
        RenderTexture renderTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);
        renderTexture.Create();

        // Create a temporary camera-less rendering
        Graphics.Blit(null, renderTexture, sourceMaterial);

        // Convert RenderTexture to Texture2D
        Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
        texture.Apply();
        RenderTexture.active = null;

        // Save the texture as PNG
        SaveTextureToFile(texture, "GeneratedMaterialTexture.png");

        // Cleanup
        Destroy(texture);
        renderTexture.Release();
        Destroy(renderTexture);
    }

    public void SaveTextureToFile(Texture2D texture, string fileName)
    {
        byte[] bytes = texture.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(path, bytes);
        Debug.Log("Texture saved to: " + path);
    }

    void Start()
    {
        GenerateTexture();
        //  Texture2D generatedTexture = GenerateTexture();
        // SaveTextureToFile(generatedTexture, "GeneratedTexture.png");
    }
}