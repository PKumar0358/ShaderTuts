using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[ExecuteAlways]
#endif
public partial class ProceduralDrawTest : MonoBehaviour
{
    const int commandCount = 1;
    GraphicsBuffer commandBuf;
    GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    public Mesh mesh;
    public bool drawInEditMode = false;
    private bool IsInited = false;
    public Material m_Material;
    private RenderParams rps;
    void Update()
    {
        if (!Application.isPlaying)
        {
            if (drawInEditMode)
            {
                DrawProcedural();
                return;
            }
            else
            {
                
            }
        }
    }

    void DrawProcedural()
    {
        if (!IsInited)
        {
            commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
            commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[commandCount];
            
            m_Material.enableInstancing = true;
            Matrix4x4 mtrx=Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            m_Material.SetMatrix("_obj_2_world_matrix_array",mtrx);
            m_Material.SetMatrix("_world_2_obj_matrix",mtrx.inverse);
            var lightmapScale = transform.GetComponent<Renderer>().lightmapScaleOffset;   
            m_Material.SetVector("_lightmapScaleOffsets",lightmapScale);
            rps=new RenderParams();
            rps.worldBounds = new Bounds(Vector3.zero, 10000*Vector3.one); // use tighter bounds for better FOV culling
            rps.material=m_Material;
            
            commandData[0].indexCountPerInstance = mesh.GetIndexCount(0);
            commandData[0].instanceCount = 1;
            commandBuf.SetData(commandData);
            
            IsInited = true;
        }
        
      
        Graphics.RenderMeshIndirect(rps, mesh, commandBuf, commandCount);
    }
}
