using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PRK.Procedural
{
    public class MeshDataCollector : MonoBehaviour
    {
        [SerializeField]private List<Texture2D>main_textures;
        [SerializeField]private List<Texture2D>normal_textures;
        [SerializeField]private List<Texture2D>detail_textures;
        [SerializeField]private List<Texture2D>detailMap_textures;
        [SerializeField] private Material[] m_Materials;
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
            List<ProcedrualRenderBatch>tempBatches=new List<ProcedrualRenderBatch>();
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

            foreach (var m in matss)
            {
                var x = m.Key;
                if (x.GetTexture("_BaseMap")!=null)
                    main_textures.Add((Texture2D)x.GetTexture("_BaseMap"));
                if (x.GetTexture("_BumpMap")!=null)
                    normal_textures.Add((Texture2D)x.GetTexture("_BumpMap"));
                if (x.GetTexture("_DetailAlbedoMap")!=null)
                    detail_textures.Add((Texture2D)x.GetTexture("_DetailAlbedoMap"));
                if (x.GetTexture("_DetailNormalMap")!=null)
                    detailMap_textures.Add((Texture2D)x.GetTexture("_DetailNormalMap"));
            }
        }
        
        [System.Serializable]
        public class ProcedrualRenderBatch
        {
            public Vector4 scaleOffset_Main;
            public Vector4 scaleOffset_Detail;
        }
        
        public struct InstanceData
        {
            public int Batch_ID;
            public int Transform_ID;
            public int MainTex_ID;
            public int NormalMap_ID;
            public int DetailTex_ID;
            public int DetailMap_ID;
            public int Lightmap_ID;

            public InstanceData Init()
            {
                Batch_ID = -1;
                Transform_ID = -1;
                MainTex_ID = -1;
                NormalMap_ID = -1;
                DetailTex_ID = -1;
                DetailMap_ID = -1;
                Lightmap_ID = -1;
                return this;
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
