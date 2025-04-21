using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PRK.Procedural
{
    using Unity.Mathematics;


    public partial class BatchCreator : MonoBehaviour
    {
        public BatchData_Config_Functions batchData;
        public static Dictionary<Texture2D,int> tex_main;
        public static  Dictionary<Texture2D,int> tex_normal_map;
        public static  Dictionary<Texture2D, int> tex_detail;
        public static  Dictionary<Texture2D,int> tex_specular_map;
        public static  Dictionary<Texture2D,int> tex_metalic_map;
        public static  Dictionary<Texture2D, int> tex_detail_normal_map;
        
        public List<Batch_Info> m_Batch_Info;
        public List<Material> materials;
        public List<Color> colors_list;
      

        [ContextMenu("Prepare_BatchData")]
        private void Prepare_BatchData()
        {
            Prepare_BatchData(transform);
        }
        private void Prepare_BatchData(Transform parent_)
        {
            tex_main = new Dictionary<Texture2D, int>();
            tex_detail = new Dictionary<Texture2D, int>();
            tex_normal_map = new Dictionary<Texture2D, int>();
            tex_specular_map = new Dictionary<Texture2D, int>();
            tex_metalic_map = new Dictionary<Texture2D, int>();
            tex_detail_normal_map = new Dictionary<Texture2D, int>();
          
            m_Batch_Info=new List<Batch_Info>();
            colors_list = new List<Color>();
            materials=new List<Material>();
            
            Dictionary<int, List<Instance_Info>> batch_Instance_Dict = new Dictionary<int, List<Instance_Info>>();
            Dictionary<Mesh,int>mesh_ids = new Dictionary<Mesh,int>();
            List<Mesh>mesh_list = new List<Mesh>();
            Dictionary<int,int[]>mesh_submesh_ids = new Dictionary<int,int[]>();
            Dictionary<int,int[]>mesh_batch_ids = new Dictionary<int,int[]>();
            Dictionary<int,Material[]>meshids_and_materials = new Dictionary<int,Material[]>();
            
            MeshRenderer[]renderers = parent_.GetComponentsInChildren<MeshRenderer>();
           
           int count = renderers.Length;
           int instance_counter = -1;
           int batch_counter = 0;
           for (int i = 0; i < count; i++)
           {
               Transform t=renderers[i].transform;
               if (t.TryGetComponent(out MeshFilter filter_))
               {
                   if (filter_.sharedMesh != null)
                   {
                       int sub_meshcount_ = filter_.sharedMesh.subMeshCount;
                       int mesh_id = AddToDictionary(mesh_ids,filter_.sharedMesh);
                       if (!mesh_batch_ids.ContainsKey(mesh_id))
                       {
                           mesh_list.Add(filter_.sharedMesh);
                           List<int>tmplist = new List<int>();
                           for (int k = 0; k < sub_meshcount_; k++)
                           {
                               tmplist.Add(batch_counter);
                               batch_counter++;
                           }
                           mesh_batch_ids.Add(mesh_id,tmplist.ToArray()); 
                       }
                       if (!meshids_and_materials.ContainsKey(mesh_id))
                           meshids_and_materials.Add(mesh_id,renderers[i].sharedMaterials);
                      
                       if (!mesh_submesh_ids.ContainsKey(mesh_id))
                           mesh_submesh_ids.Add(mesh_id,NewIds_List(sub_meshcount_));
                       for (int j = 0; j < sub_meshcount_; j++)
                       {
                           Instance_Info ifo=new Instance_Info(ref instance_counter, mesh_id, mesh_batch_ids[mesh_id][j], i);
                           if (batch_Instance_Dict.ContainsKey(ifo.batch_id))
                               batch_Instance_Dict[ifo.batch_id].Add(ifo);
                           else
                           {
                               List<Instance_Info>ifolist = new List<Instance_Info>();
                               ifolist.Add(ifo);
                               batch_Instance_Dict.Add(ifo.batch_id,ifolist);
                           }
                       }
                   }
               }
           }
           
           Dictionary<Vector4, int> scale_offsets=new Dictionary<Vector4, int>();
           Dictionary<Vector4,int>colors_dict=new Dictionary<Vector4,int>();
         
           foreach (var x in mesh_batch_ids)
           {
               Mesh ms = mesh_list[x.Key];
               int[] submesh_ids = x.Value;
               for (int i = 0; i < submesh_ids.Length; i++)
               {
                   Material mat_ = meshids_and_materials[x.Key][i];
                   materials.Add(mat_);
                   Batch_Info b_info = new Batch_Info(x.Value[i]);
                   Add_MaterialData_ToDictionaries(b_info,mat_,scale_offsets);
                   b_info.AddInfo(mat_,out Color mainclr_,out Color emissionclr_);
                   b_info.color_main_id=AddToDictionary(colors_dict,mainclr_);
                   m_Batch_Info.Add(b_info);
               }
           }
          
        }

        private int[] NewIds_List(int count_)
        {
            int[] ids = new int[count_];
            for(int i=0; i<count_; i++)
                ids[i] = i;
            return ids;
        }
        
        private void Add_MaterialData_ToDictionaries(Batch_Info batchInfo_,Material mat_,Dictionary<Vector4,int>dict_scaleOffset_)
        {
            Vector4 v1 = GetScaleOffset("_BaseMap", mat_);
            Vector4 v2 = GetScaleOffset("_DetailAlbedoMap", mat_);
            batchInfo_.scale_offset_main_id = AddToDictionary(dict_scaleOffset_,v1);
            batchInfo_.scale_offset_detail_id = AddToDictionary(dict_scaleOffset_,v2);
        }
        
        private Vector4 GetScaleOffset(string Name_,Material mat_)
        {
            Vector4 scaleoffset=new Vector4(1,1,0,0);
            Vector2 v= mat_.GetTextureScale(Name_);
            scaleoffset.x=v.x;
            scaleoffset.y=v.y;
            v= mat_.GetTextureOffset(Name_);
            scaleoffset.z=v.x;
            scaleoffset.w=v.y;
            return scaleoffset;
        }
        
        private int AddToDictionary<T>(Dictionary<T,int> dict, T ke)
        {
            int id = -1;
            if (dict.ContainsKey(ke))
                id = dict[ke];
            else
            {
                id = dict.Count;
                dict.Add(ke,id);
            }

            return id;
        }

  
    }

    
}
