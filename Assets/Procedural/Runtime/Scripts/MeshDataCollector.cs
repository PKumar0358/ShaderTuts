using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PRK.Procedural
{
    public class MeshDataCollector : MonoBehaviour
    {
        [SerializeField]private BatchData[] m_Batch_Data;
        [SerializeField]private List<Texture2D>main_textures;
        [SerializeField]private List<Texture2D>normal_textures;
        [SerializeField]private List<Texture2D>detail_textures;
        [SerializeField]private List<Texture2D>detailMap_textures;
        [SerializeField]private Material[] m_Materials;
        [ContextMenu("Collect")]
        void Collect()
        {
            main_textures=new List<Texture2D>();
            normal_textures=new List<Texture2D>();
            detail_textures = new List<Texture2D>();
            detailMap_textures = new List<Texture2D>();
            Dictionary<int,int>batchID_matID=new Dictionary<int,int>();
            Dictionary<Mesh,int>mesh_batch_ = new Dictionary<Mesh, int>();
            List<InstanceData>instanceDataList=new List<InstanceData>();
            Dictionary<Material, int> matss = new Dictionary<Material, int>();
            var objs=transform.GetComponentsInChildren<MeshRenderer>();
            TransformData[]trs=new TransformData[objs.Length];
            int submeshCount=0;
            int mesh_ID = -1;
            int material_ID = -1;
            int batchCounter = -1;
            
            List<int[]>Batch_IDs_=new List<int[]>();
            for (int i = 0; i <objs.Length; i++)
            {
                if (objs[i].TryGetComponent(out MeshFilter ms))
                {
                    if (!mesh_batch_.ContainsKey(ms.sharedMesh))
                    {
                        submeshCount=ms.sharedMesh.subMeshCount;
                        int[] batch_IDs_=new int[submeshCount];
                        for (int idx = 0; idx < submeshCount; idx++)
                        {
                            batch_IDs_[idx]=++batchCounter;
                            if (!matss.ContainsKey(objs[i].sharedMaterials[idx]))
                                matss.Add(objs[i].sharedMaterials[idx],++material_ID);
                            batchID_matID.Add(batchCounter,material_ID);
                        }
                        Batch_IDs_.Add(batch_IDs_);
                        mesh_batch_.Add(ms.sharedMesh, ++mesh_ID);
                    }

                    trs[i] = new TransformData(objs[i].transform);
                    
                    for (int idx = 0; idx < submeshCount; idx++)
                    {
                        InstanceData i_data=new InstanceData().Init();
                        i_data.Transform_ID = i;
                        i_data.Lightmap_ID=objs[i].lightmapIndex;
                        i_data.Batch_ID=Batch_IDs_[mesh_ID][idx];
                        instanceDataList.Add(i_data);
                    }
                }
            }

          
            m_Materials = matss.Keys.ToArray();
            int m_id_=0, mainTex_ID_=-1, normalMap_ID_=-1,detailTex_ID_=-1,detailMap_ID_=-1;
            m_Batch_Data=new BatchData[matss.Count];
            foreach (var m in matss)
            {
                var x = m.Key;
                BatchData b=new BatchData().Init();
                MaterialData.SetNewBatchData(x,ref mainTex_ID_,ref normalMap_ID_,ref detailTex_ID_,ref detailMap_ID_,m_id_,ref b);
                b.scaleOffset_Main = BatchData.GetScaleOffst("_BaseMap", x);
                b.scaleOffset_Detail = BatchData.GetScaleOffst("_DetailAlbedoMap", x);
                m_Batch_Data[m_id_] = b;
               // materialsData[m_id_]=new MaterialData(x,ref mainTex_ID_,ref normalMap_ID_,ref detailTex_ID_,ref detailMap_ID_,m_id_);
                m_id_++;
                /*if (x.GetTexture("_BaseMap")!=null)
                    main_textures.Add((Texture2D)x.GetTexture("_BaseMap"));
                if (x.GetTexture("_BumpMap")!=null)
                    normal_textures.Add((Texture2D)x.GetTexture("_BumpMap"));
                if (x.GetTexture("_DetailAlbedoMap")!=null)
                    detail_textures.Add((Texture2D)x.GetTexture("_DetailAlbedoMap"));
                if (x.GetTexture("_DetailNormalMap")!=null)
                    detailMap_textures.Add((Texture2D)x.GetTexture("_DetailNormalMap"));*/
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
        
           
        [System.Serializable]
        public class MaterialData
        {
        //    [HideInInspector]
            public int MainTex_ID;
         //   [HideInInspector]
            public int NormalMap_ID;
          //  [HideInInspector]
            public int DetailTex_ID;
        //    [HideInInspector]
            public int DetailMap_ID;
            public MaterialData(Material material_,ref int mainTex_ID_,ref int normalMap_ID_,ref int detailTex_ID_,ref int detailMap_ID_,int materialID_)
            {
                MainTex_ID = -1;
                NormalMap_ID = -1;
                DetailTex_ID = -1;
                DetailMap_ID = -1;
                if (material_.GetTexture("_BaseMap") != null)
                    MainTex_ID = ++mainTex_ID_;
                if (material_.GetTexture("_BumpMap")!=null)
                    NormalMap_ID=++normalMap_ID_;
                if (material_.GetTexture("_DetailAlbedoMap")!=null)
                    DetailTex_ID=++detailTex_ID_;
                if (material_.GetTexture("_DetailNormalMap") != null)
                    DetailMap_ID = ++detailMap_ID_;
            }

            /*public void Set_InstanceData(ref InstanceData instanceData_)
            {
                instanceData_.MainTex_ID = MainTex_ID;
                instanceData_.NormalMap_ID = NormalMap_ID;
                instanceData_.DetailTex_ID = DetailTex_ID;
                instanceData_.DetailMap_ID = DetailMap_ID;
            }*/
            public void Set_InstanceData(ref BatchData batchData_)
            {
                batchData_.MainTex_ID = MainTex_ID;
                batchData_.NormalMap_ID = NormalMap_ID;
                batchData_.DetailTex_ID = DetailTex_ID;
                batchData_.DetailMap_ID = DetailMap_ID;
            }

            public static void SetNewBatchData(Material material_, ref int mainTex_ID_, ref int normalMap_ID_,
                ref int detailTex_ID_, ref int detailMap_ID_, int materialID_,ref BatchData batchData_)
            {
                MaterialData mt= new MaterialData(material_,ref mainTex_ID_,ref normalMap_ID_,ref detailTex_ID_,ref detailMap_ID_,materialID_);
                mt.Set_InstanceData(ref batchData_);
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
       
    }
}
