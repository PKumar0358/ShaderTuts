using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace PRK.Procedural
{
#if UNITY_EDITOR
    using UnityEditor;
    public partial class RenderBatchData_Config
    {
        private static Texture2D t0;
        private static Texture2D t1;
        private static Texture2D t2;
        private static Texture2D t3;
        public static HashSet<Vector2> scales;
        public static HashSet<Vector2> offsets;
        public static HashSet<Texture2D> texHas;
        public void SaveBatchData(Transform transform_)
        {
            texHas = new HashSet<Texture2D>();
            scales = new HashSet<Vector2>();
            offsets = new HashSet<Vector2>();
            mesh_Data = new List<Mesh>();
            Dictionary<int,int>mesh_IDs_n_BatchIDs_=new Dictionary<int,int>();
            Dictionary<Mesh,int>mesh_n_MeshIDs_ = new Dictionary<Mesh, int>();
            Dictionary<Material, int> materials_n_BatchIDs = new Dictionary<Material, int>();
           instanceDataList=new List<InstanceData>();
            var objs=transform_.GetComponentsInChildren<MeshRenderer>();
            TransformData[]trs=new TransformData[objs.Length];
            int submeshCount=0;
            int totalSubmesh=0;
            int mesh_ID = -1;
            int Batch_ID_ = -1;
            int mesh_Counter_ = -1;//all meshes including submeshes
            
            List<int[]>SubMesh_IDs_=new List<int[]>();
            
            for (int i = 0; i <objs.Length; i++)
            {
                if (objs[i].TryGetComponent(out MeshFilter ms))
                {
                    if (!mesh_n_MeshIDs_.ContainsKey(ms.sharedMesh))
                    {
                        submeshCount=ms.sharedMesh.subMeshCount;
                        mesh_Data.Add(ms.sharedMesh);
                        totalSubmesh+=submeshCount;
                        int[] submesh_IDsArray_=new int[submeshCount];
                        for (int idx = 0; idx < submeshCount; idx++)
                        {
                            submesh_IDsArray_[idx]=++mesh_Counter_;
                            if (!materials_n_BatchIDs.ContainsKey(objs[i].sharedMaterials[idx]))
                            {
                                materials_n_BatchIDs.Add(objs[i].sharedMaterials[idx],++Batch_ID_);
                            }
                            mesh_IDs_n_BatchIDs_.Add(mesh_Counter_,Batch_ID_);
                        }
                        SubMesh_IDs_.Add(submesh_IDsArray_);
                        mesh_n_MeshIDs_.Add(ms.sharedMesh, ++mesh_ID);
                    }

                    trs[i] = new TransformData(objs[i].transform);
                    
                    for (int idx = 0; idx < submeshCount; idx++)
                    {
                        InstanceData i_data=new InstanceData().Init();
                        i_data.Transform_ID = i;
                        i_data.Lightmap_ID=objs[i].lightmapIndex;
                       // i_data.Batch_ID=SubMesh_IDs_[mesh_ID][idx];
                        i_data.Batch_ID=mesh_IDs_n_BatchIDs_[SubMesh_IDs_[mesh_ID][idx]];
                        instanceDataList.Add(i_data);
                    }
                }
            }

         
            m_Materials = materials_n_BatchIDs.Keys.ToArray();
            int m_id_=0, mainTex_ID_=-1, normalMap_ID_=-1,detailTex_ID_=-1,detailMap_ID_=-1;
            m_Batch_Data=new BatchData[materials_n_BatchIDs.Count];
           
            foreach (var m in materials_n_BatchIDs)
            {
                var x = m.Key;
                
                BatchData b=new BatchData().Init();
                SetNewBatchData(x,ref mainTex_ID_,ref normalMap_ID_,ref detailTex_ID_,ref detailMap_ID_,ref b);
              //  GetScaleOffst($"_BaseMap",x);
               // GetScaleOffst($"_DetailAlbedoMap",x);
                AddToTexHas(t0);
                AddToTexHas(t1);
                AddToTexHas(t2);
                AddToTexHas(t3);
                
                b.scaleOffset_Main = BatchData.GetScaleOffst("_BaseMap", x);
                b.scaleOffset_Detail = BatchData.GetScaleOffst("_DetailAlbedoMap", x);
                m_Batch_Data[m_id_] = b;
                m_id_++;
            }
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
            AssetDatabase.Refresh();
        }

        private static void AddToTexHas(Texture2D tex_)
        {
            if (tex_ != null)
            {
                if (texHas.Contains(tex_))
                {
                    Debug.Log($" already added tex {tex_.name}");
                }
                else
                {
                    texHas.Add(tex_);
                }
            }
            
        }
        private static void GetScaleOffst(string Name_,Material mat_)
        {
            var scl = mat_.GetTextureScale(Name_);
            var offset=mat_.GetTextureOffset(Name_);
            if (scales.Contains(scl))
            {
                Debug.Log($"{Name_}  {scl}");
            }
            else
            {
                scales.Add(scl);
            }

            if (offsets.Contains(offset))
            {
                Debug.Log($"{Name_}  {offset}");
            }
            else
            {
                offsets.Add(offset);
            }
        }
        private static void SetTextureArray(Texture2D[]texures_,out Texture2DArray output_)
        {
            int depth=texures_.Length;
            Texture2D inTex = texures_[0];
            int width = inTex.width;
            int height = inTex.width;
            output_ = new Texture2DArray(width, height, depth, inTex.format, true);
            for (int i = 0; i < depth; i++)
            {
                inTex = texures_[i];
                for (int mip = 0; mip < inTex.mipmapCount; ++mip)
                {
                    int copyWidth = width >> mip;
                    int copyHeight = height >> mip;
                    Graphics.CopyTexture(inTex, 0, mip, 0, 0, copyWidth, copyHeight, output_, i, mip, 0, 0);
                }
            }
        }
        
        public static void SetNewBatchData(Material material_, ref int mainTex_ID_, ref int normalMap_ID_,
            ref int detailTex_ID_, ref int detailMap_ID_,ref BatchData batchData_)
        {
            t0=null;
            t1=null;
            t2=null;
            t3=null;
            
            batchData_.MainTex_ID = -1;
            batchData_.NormalMap_ID = -1;
            batchData_.DetailTex_ID = -1;
            batchData_.DetailMap_ID = -1;
            
            t0=material_.GetTexture("_BaseMap") as Texture2D;
            t1=material_.GetTexture("_BumpMap") as Texture2D;
            t2=material_.GetTexture("_DetailAlbedoMap") as Texture2D;
            t3=material_.GetTexture("_DetailNormalMap") as Texture2D;
            
            if (t0 != null)
                batchData_.MainTex_ID = ++mainTex_ID_;
            if (t1!=null)
                batchData_.NormalMap_ID=++normalMap_ID_;
            if (t2!=null)
                batchData_.DetailTex_ID=++detailTex_ID_;
            if (t3 != null)
                batchData_.DetailMap_ID = ++detailMap_ID_;
        }
        
        private void CreateSubmesh()
        {
            
        }
    }
    
  
#endif
    [CreateAssetMenu]
    public partial class RenderBatchData_Config : ScriptableObject
    {
        [SerializeField]
        private Material[] m_Materials;
        public Texture2D[] MainTex_Array;
        //   [HideInInspector]
        public Texture2D[] NormalMap_Array;
        //  [HideInInspector]
        public Texture2D[] DetailTex_Array;
        //    [HideInInspector]
        public Texture2D[] DetailMap_Array;
        
        [SerializeField] 
        private List<InstanceData> instanceDataList;
        [SerializeField]
        private BatchData[] m_Batch_Data;
        [SerializeField]
        private List<Mesh> mesh_Data;
    }

    [System.Serializable]
    public class TexArray
    {
        public string m_Name;
        public Texture2DArray m_TexArray;

        public TexArray(string name_, Texture2DArray texArray_)
        {
            m_Name = name_;
            m_TexArray = texArray_;
        }
    }
    
    public struct TransformData
    {
        public Matrix4x4 objToWorld;
        public Matrix4x4 worldToObj;
        public TransformData(Transform trs_)
        {
            objToWorld=Matrix4x4.TRS(trs_.position,trs_.rotation,trs_.lossyScale);
            worldToObj = objToWorld.inverse;
        }
    }
    
    [System.Serializable]
    public struct BatchData
    {
        // [HideInInspector]
        public int MainTex_ID;
            
        // [HideInInspector]
        public int NormalMap_ID;
            
        // [HideInInspector]
        public int DetailTex_ID;
            
        // [HideInInspector]
        public int DetailMap_ID;
            
        // [HideInInspector]
        public Vector4 scaleOffset_Main;
            
        // [HideInInspector]
        public Vector4 scaleOffset_Detail;

        public BatchData Init()
        {
            MainTex_ID = -1;
            NormalMap_ID = -1;
            DetailTex_ID = -1;
            DetailMap_ID = -1;
            return this;
        }
            
        public static Vector4 GetScaleOffst(string Name_,Material mat_)
        {
            var scl = mat_.GetTextureScale(Name_);
            var offset=mat_.GetTextureOffset(Name_);
            
            return new Vector4(scl.x,scl.y,offset.x,offset.y);
        }
    }
    
    [System.Serializable]
    public struct InstanceData
    {
        //   [HideInInspector]
        public int Batch_ID;
        //  [HideInInspector]
        public int Transform_ID;
        // [HideInInspector]
        public int Lightmap_ID;
        public InstanceData Init()
        {
            Batch_ID = -1;
            Transform_ID = -1;
            Lightmap_ID = -1;
            return this;
        }
    }
}
