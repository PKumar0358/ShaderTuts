using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PRK_Procedural
{
#if UNITY_EDITOR
    using UnityEditor;

    public partial class RenderingBatchConfig
    {
        public void SaveToConfig(Renderer[] renderers_)
        {
            m_DataRaw =new Intstance_DataRaw[renderers_.Length];
            for (int i = 0; i < renderers_.Length; i++)
            m_DataRaw[i] = new Intstance_DataRaw(renderers_[i]);
            m_Mesh=renderers_[0].GetComponent<MeshFilter>().sharedMesh;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
            AssetDatabase.Refresh();
        }
    }
#endif
    public partial class RenderingBatchConfig
    {
        private const int sizeof_IntstanceData = 2 * (sizeof(float) * 16) + sizeof(float) * 4 + sizeof(int);
        [SerializeField]
       // [HideInInspector]
        private Intstance_DataRaw[] m_DataRaw;
        [HideInInspector]
        public Mesh m_Mesh;
        [SerializeField]
        private Material m_Material;
        
    }
    [CreateAssetMenu]
    public partial class RenderingBatchConfig : ScriptableObject
    {
        public void SetUp_BatchData(ref GraphicsBuffer.IndirectDrawIndexedArgs[] commandDataBuffer_,out GraphicsBuffer instanceDataBuffer_,out RenderParams rps_)
        {
            int instanceCount=m_DataRaw.Length;
            Intstance_Data[] dataArray=new Intstance_Data[instanceCount];
            instanceDataBuffer_=new GraphicsBuffer(GraphicsBuffer.Target.Structured, instanceCount,sizeof_IntstanceData);
            for (int i = 0; i < instanceCount; i++)
                dataArray[i]=new Intstance_Data(m_DataRaw[i]);
            instanceDataBuffer_.SetData(dataArray);
            commandDataBuffer_[0].instanceCount = (uint)instanceCount;
            commandDataBuffer_[0].indexCountPerInstance = m_Mesh.GetIndexCount(0);
            rps_=new RenderParams();
            rps_.material = new Material(m_Material);
            rps_.material.enableInstancing = true;
            rps_.material.EnableKeyword("_MyFeature");
            rps_.material.SetBuffer("_IntstanceDataBuffer", instanceDataBuffer_);
            rps_.worldBounds = new Bounds(Vector3.zero,Vector3.one*1000);
        }
    }

    [System.Serializable]
    public class Intstance_DataRaw
    {
        [HideInInspector]
        public int lightmapIndex=-1;
      //  [HideInInspector]
        public Vector3 position;
        [HideInInspector]
        public Vector3 angles;
     //   [HideInInspector]
        public Vector3 scale;
        [HideInInspector]
        public Vector4 scaleOffset;

        public Intstance_DataRaw(Renderer renderer_)
        {
            
            this.lightmapIndex = renderer_.lightmapIndex;
            this.position = renderer_.transform.position;
            this.angles = renderer_.transform.eulerAngles;
            this.scale = renderer_.transform.lossyScale;
            if(this.lightmapIndex != -1)
                this.scaleOffset=renderer_.lightmapScaleOffset;
            else
                this.scaleOffset=new Vector4(0,0,0,0);
        }
        public Intstance_DataRaw(Component comp_)
        {
            if (comp_.TryGetComponent(out Renderer renderer_))
            {
                this.lightmapIndex = renderer_.lightmapIndex;
                this.position = renderer_.transform.position;
                this.angles = renderer_.transform.eulerAngles;
                this.scale = renderer_.transform.lossyScale;
                if(this.lightmapIndex != -1)
                    this.scaleOffset=renderer_.lightmapScaleOffset;
                else
                    this.scaleOffset=new Vector4(0,0,0,0);
            }
        }
        public Matrix4x4 getMatrix=>Matrix4x4.TRS(position,Quaternion.Euler(angles),scale);
     
    }
    public struct Intstance_Data
    {
       public int Lightmap_Index;
       public Vector4 Lightmap_ScaleOffset;
       public Matrix4x4 ObjectToWorld;
       public Matrix4x4 WorldToObject;

       public Intstance_Data(Intstance_DataRaw raw_data_)
       {
           Lightmap_Index = raw_data_.lightmapIndex;
           Lightmap_ScaleOffset=raw_data_.scaleOffset;
           ObjectToWorld = raw_data_.getMatrix;
           WorldToObject=ObjectToWorld.inverse;
       }
    }
}
