
using System;
using UnityEngine;

public class ProceduralRenderer : MonoBehaviour
{
    [SerializeField] private Texture2D[] maps;
    private bool m_IsDataInitialized = false;
    private const int _4x4Size=sizeof(float) * 16;
    private GraphicsBuffer _obj_2_world_buffer;
    private GraphicsBuffer _world_2_obj_buffer;
    private GraphicsBuffer commandBuffer;
    private GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    private RenderParams rps;
    private Mesh meshToRender;
    [SerializeField]
    private Material m_Material;
    void Start()
    {
        GetMatrixData(out Mesh mesh_,out Matrix4x4[]m1,out Matrix4x4[]m2);
        int c = m1.Length;
        meshToRender = mesh_;
        PrepareCommandData_Part1(mesh_);
        PrepareCommandData_Part2((uint)c);
        commandBuffer.SetData(commandData);
        
        m_Material.enableInstancing = true;
        
        _obj_2_world_buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, c,_4x4Size);
        _world_2_obj_buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, c,_4x4Size);
        _obj_2_world_buffer.SetData(m1);
        _world_2_obj_buffer.SetData(m2);
        m_Material.SetBuffer("_obj_2_world_matrix", _obj_2_world_buffer);
        m_Material.SetBuffer("_world_2_obj_matrix", _world_2_obj_buffer);
        rps = new RenderParams(m_Material);
       // rps.matProps.SetMatrixArray("_ObjectToWorld",m1);
       // rps.matProps.SetMatrixArray("_WorldToObject",m2);
        rps.worldBounds = new Bounds(Vector3.zero, Vector3.one*1000f);
      //  UploadLightMaps();
        m_IsDataInitialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_IsDataInitialized)
            Graphics.RenderMeshIndirect(rps,meshToRender,commandBuffer,1);
    }

    private void OnDestroy()
    {
        Dispose();
    }

    void GetMatrixData(out Mesh mesh_, out Matrix4x4[] trss_, out Matrix4x4[] invtrss_)
    {
        var ms = transform.GetComponentsInChildren<Renderer>(true);
        trss_ = new Matrix4x4[ms.Length];
        invtrss_ = new Matrix4x4[ms.Length];
        mesh_ = ms[0].GetComponent<MeshFilter>().sharedMesh;
        for (int i = 0; i < ms.Length; i++)
        {
            Transform t = ms[i].transform;
            trss_[i] = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
            invtrss_[i] = trss_[i].inverse;
            t.gameObject.SetActive(false);
        }
    }
    
    public void Dispose()
    {
        commandBuffer.Dispose();
        _obj_2_world_buffer.Dispose();
        _world_2_obj_buffer.Dispose();
        _obj_2_world_buffer=null;
        _world_2_obj_buffer=null;
        commandData = null;
        commandBuffer = null;
        m_IsDataInitialized = false;
    }
    void PrepareCommandData_Part1(params Mesh[] meshes_)
    {
        int count = meshes_.Length;
        commandData=new GraphicsBuffer.IndirectDrawIndexedArgs[count];
        for (int i = 0; i < count; i++)
            commandData[i].indexCountPerInstance = meshes_[i].GetIndexCount(0);
    }

    void PrepareCommandData_Part2(params uint[] instanceCounts_)
    {
        int count = instanceCounts_.Length;
        commandBuffer=new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments,count,GraphicsBuffer.IndirectDrawIndexedArgs.size);
        for (int i = 0; i < count; i++)
            commandData[i].instanceCount  = instanceCounts_[i];
        
    }

    void UploadLightMaps()
    {
        int textureSize = maps[0].width; // Example size
        int textureCount = maps.Length;   // Number of textures in the array
        Texture2DArray  textureArray = new Texture2DArray(textureSize, textureSize, textureCount, TextureFormat.RGBA32, false);

        for (int i = 0; i < textureCount; i++)
        {
            Texture2D tex = new Texture2D(textureSize, textureSize);
            // Fill texture with data
            textureArray.SetPixels(maps[i].GetPixels(), i);
        }

        textureArray.Apply();
        Shader.SetGlobalTexture("_LightMaps", textureArray);
    }
}
