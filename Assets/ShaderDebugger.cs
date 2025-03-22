using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderDebugger : MonoBehaviour
{
    RenderTexture globalRT;
    [SerializeField]
    private ComputeBuffer _buffer;
    public int dataIndexToPrint=-1;
   
    void Start()
    {
        globalRT = new RenderTexture(16, 16, 0, RenderTextureFormat.ARGBFloat);
        globalRT.enableRandomWrite = true;
        globalRT.Create();
        Shader.SetGlobalTexture("_GlobalDbugTexture", globalRT);
        
          StartCoroutine(DisplayData());
    }

    IEnumerator DisplayData()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
           
            if (dataIndexToPrint != -1)
            {
                Vector4 v = Vector4.zero;
                Debug.Log($"x {v.x} y {v.y} z {v.z}  {v.w}");
                
                RenderTexture.active = globalRT;
                Texture2D tex = new Texture2D(globalRT.width, globalRT.height, TextureFormat.RGBAFloat, false);

                tex.ReadPixels(new Rect(0, 0, 1, 1), dataIndexToPrint, dataIndexToPrint);
                tex.Apply();

                Color pixelColor = tex.GetPixel(dataIndexToPrint, dataIndexToPrint);
                Debug.Log($"Pixel (0,0) color: {pixelColor}");

                RenderTexture.active = null;
            }
        }
    }
}
