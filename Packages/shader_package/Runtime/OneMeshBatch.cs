using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.BaseCommands;
using UnityEngine;

namespace PRK_Procedural
{
    #if UNITY_EDITOR
    using UnityEditor;

    public partial class OneMeshBatch
    {
        [SerializeField,HideInInspector]
        private bool keywordOn = false;

        [SerializeField]
        private bool On = false;
        [ContextMenu("SaveMesh Group Data")]
        void SaveMeshGroupData()
        {
            var meshes=transform.GetComponentsInChildren<Renderer>(true);
            m_BatchConfig?.SaveToConfig(meshes);
            foreach (var x in meshes)
                x.gameObject.SetActive(false);
        }
        /*protected override void OnEditorDrawCommandInitialized()
        {
            if (!m_IsInitialized)
            {
               
                
            }
            m_IsInitialized = true;
        }

        protected override void OnEditorDrawCommand()
        {
            if (m_IsInitialized)
            {
               
                Graphics.RenderMeshIndirect(m_RenderParams,m_BatchConfig.m_Mesh,m_CommandBuffer,1);
            }
        }

        protected override void OnEditorDrawCommandDisposed()
        {
            if (m_IsInitialized)
            {
               
            }
            m_IsInitialized = false;
        }

        protected override void OnEditorUpdate()
        {
           
        }*/
    }
    #endif
    public partial class OneMeshBatch : MonoBehaviour
    {
        [SerializeField] private Texture2D[] lightMaps;
        [SerializeField] private Texture2D[] dirMaps;
        [SerializeField] private Material mat;
        [System.NonSerialized]private bool m_IsInitialized = false;
       
        [SerializeField]
        private RenderingBatchConfig m_BatchConfig;

        private Batch batch;
        void OnEnable()
        {
            lightMaps = new Texture2D[LightmapSettings.lightmaps.Length];
            dirMaps = new Texture2D[LightmapSettings.lightmaps.Length];
            
            for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
            {
                lightMaps[i] = LightmapSettings.lightmaps[i].lightmapColor;
                dirMaps[i] = LightmapSettings.lightmaps[i].lightmapDir;
            }
            mat.EnableKeyword("_USE_CUSTOM_LIGHTMAPS");
            SetTextureArray(lightMaps, mat,"_LightMaps");
            SetTextureArray(dirMaps, mat,"_DirMaps");
            
            var o=transform.GetComponentsInChildren<Renderer>(true);
            batch = new Batch(o, ref mat);
            foreach (var x in o)
            {
                x.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            batch.Dispose();
            batch = null;
        }


        private void Update()
        {
            batch.Render();
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

    public class Batch
    {
        private const int sizeof_instanceData =sizeof(int)+sizeof(float)*4+2*(sizeof(float)*16);
        private int instanceCount = 0;
        private int commandCount = 1;
        private GraphicsBuffer instanceDataBuffer;
        private GraphicsBuffer commandBuffer;
        private GraphicsBuffer.IndirectDrawIndexedArgs[] commandDataBuffer;
        private RenderParams renderparams;
        private Mesh mesh;
        public Batch(Renderer[]renderers_,ref Material mat_)
        {
            mesh = renderers_[0].GetComponent<MeshFilter>().sharedMesh;
            instanceCount = renderers_.Length;
            
            commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount,
                GraphicsBuffer.IndirectDrawIndexedArgs.size);
            commandDataBuffer = new GraphicsBuffer.IndirectDrawIndexedArgs[commandCount];
            commandDataBuffer[0].instanceCount = (uint)instanceCount;
            commandDataBuffer[0].indexCountPerInstance = mesh.GetIndexCount(0);
            commandBuffer.SetData(commandDataBuffer);
            
            instanceDataBuffer=new GraphicsBuffer(GraphicsBuffer.Target.Structured,instanceCount,sizeof_instanceData);
            
            InstanceData[] data = new InstanceData[instanceCount];

            for (int i = 0; i < instanceCount; i++)
            data[i] = new InstanceData(renderers_[i]);
            
            instanceDataBuffer.SetData(data);
            renderparams = new RenderParams(mat_);
            renderparams.worldBounds = new Bounds(Vector3.zero, Vector3.one*1000f);
            renderparams.material.SetBuffer("_Instance_Data_Buffer",instanceDataBuffer);
            renderparams.material.enableInstancing = true;
        }

        public void Render()
        {
            Graphics.RenderMeshIndirect(renderparams,mesh,commandBuffer,commandCount);
        }
        public void Dispose()
        {
            commandBuffer.Dispose();
            instanceDataBuffer.Dispose();
            commandBuffer=null;
            commandDataBuffer=null;
            instanceDataBuffer=null;
            mesh=null;
            instanceCount = 0;
        }
    }

    public struct InstanceData
    {
        public int lightmapIndex;
        public Vector4 lightmapScaleOffset;
        public Matrix4x4 objToWorld;
        public Matrix4x4 worldToObj;

        public InstanceData(Renderer renderer_)
        {
            lightmapScaleOffset=Vector4.zero;
            lightmapIndex=renderer_.lightmapIndex;
            if (lightmapIndex != -1)
                lightmapScaleOffset=renderer_.lightmapScaleOffset;
            objToWorld=Matrix4x4.TRS(renderer_.transform.position,renderer_.transform.rotation,renderer_.transform.lossyScale);
            worldToObj=objToWorld.inverse;
        }
    }
}
