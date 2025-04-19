using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PRK.Procedural
{
    using Unity.Mathematics;

    public partial class BatchCreator
    {
        private const int int4_size = sizeof(int) * 4;
        private const int float4_size = sizeof(float) * 4;
        private const int matrix_4x4_size = sizeof(float) * 16;
        /// <summary>
        /// int4 array =x=transform id, y= batch id, z= -1,w=-1
        /// </summary>
        private GraphicsBuffer _Instance_IDs_Buffer;
        /// <summary>
        /// int4 array x=
        /// </summary>
        private GraphicsBuffer _Batch_IDs_Buffer_0;
        private GraphicsBuffer _Obj_To_World_Buffer;
        private GraphicsBuffer _World_To_Obj_Buffer;
        
        private GraphicsBuffer NewStruct_GBuffer(int count_,int struct_size)
        {
            GraphicsBuffer buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured,count_,struct_size);
            return buffer;
        }

        private void DisposeBuffer(GraphicsBuffer buffer)
        {
            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }
        }
    }
    public partial class BatchCreator : MonoBehaviour
    {
        public static Dictionary<Texture2D,int> tex_main;
        public static  Dictionary<Texture2D,int> tex_normal_map;
        public static  Dictionary<Texture2D, int> tex_detail;
        public static  Dictionary<Texture2D,int> tex_specular_map;
        public static  Dictionary<Texture2D,int> tex_metalic_map;
        public static  Dictionary<Texture2D, int> tex_detail_normal_map;
        
        public List<Batch_Info> m_Batch_Info;
        public List<Material> materials;
        public List<Color> colors_list;
        private void Start()
        {
           // Prepare_BatchData(transform);
        }

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
            Dictionary<string,Dictionary<Texture2D, int>>ddt = new Dictionary<string,Dictionary<Texture2D, int>>();
            ddt.Add("main",tex_main);
            ddt.Add("bump",tex_normal_map);
            ddt.Add("detail",tex_detail);
            ddt.Add("detailbump",tex_detail_normal_map);
            m_Batch_Info=new List<Batch_Info>();
            colors_list = new List<Color>();
            materials=new List<Material>();
            List<Instance_Info>instance_info_list = new List<Instance_Info>();
            Dictionary<Mesh,int>mesh_ids = new Dictionary<Mesh,int>();
            Dictionary<int,int[]>mesh_submesh_ids = new Dictionary<int,int[]>();
            Dictionary<int,Material[]>meshids_and_materials = new Dictionary<int,Material[]>();
            
            MeshRenderer[]renderers = parent_.GetComponentsInChildren<MeshRenderer>();
           
           int count = renderers.Length;
           int instance_counter = -1;
           for (int i = 0; i < count; i++)
           {
               Transform t=renderers[i].transform;
               if (t.TryGetComponent(out MeshFilter filter_))
               {
                   if (filter_.sharedMesh != null)
                   {
                       int mesh_id = AddToDictionary(mesh_ids,filter_.sharedMesh);
                       if (!meshids_and_materials.ContainsKey(mesh_id))
                           meshids_and_materials.Add(mesh_id,renderers[i].sharedMaterials);
                       int c2 = filter_.sharedMesh.subMeshCount;
                      // Debug.Log($"{c2}  {renderers[i].sharedMaterials.Length}",t);
                       if (!mesh_submesh_ids.ContainsKey(mesh_id))
                           mesh_submesh_ids.Add(mesh_id,NewIds_List(c2));
                       for (int j = 0; j < c2; j++)
                           instance_info_list.Add(new Instance_Info(ref instance_counter,mesh_id,j,i));
                   }
               }
           }
           
           Dictionary<Vector4, int> scale_offsets=new Dictionary<Vector4, int>();
           Dictionary<Vector4,int>colors_dict=new Dictionary<Vector4,int>();
           int batch_counter = 0;
           foreach (var x in mesh_submesh_ids)
           {
               int[] submesh_ids = x.Value;
               for (int i = 0; i < submesh_ids.Length; i++)
               {
                   Material mat_ = meshids_and_materials[x.Key][i];
                   materials.Add(mat_);
                   Batch_Info b_info = new Batch_Info(batch_counter);
                   Add_MaterialData_ToDictionaries(b_info,mat_,scale_offsets);
                   b_info.AddInfo(mat_,out Color mainclr_,out Color emissionclr_);
                   b_info.color_main_id=AddToDictionary(colors_dict,mainclr_);
                   m_Batch_Info.Add(b_info);
                   batch_counter++;
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

        private int AddScaleOffsetToDictionary(Dictionary<Vector4,int>scale_offsets_,Vector4 scale_offset_)
        {
            int id=-1;
            if (scale_offsets_.ContainsKey(scale_offset_))
                id=scale_offsets_[scale_offset_];
            else
            {
                id= scale_offsets_.Count;
                scale_offsets_.Add(scale_offset_,id);
            }
            return id;
        }
        private void Collect_MaterialData(Material mat_,Dictionary<Vector4,int>scale_offsets_)
        {
           
            
        }

        private void Add_MaterialData_ToDictionaries(Batch_Info batchInfo_,Material mat_,Dictionary<Vector4,int>dict_scaleOffset_)
        {
            Vector4 v1 = GetScaleOffset("_BaseMap", mat_);
            Vector4 v2 = GetScaleOffset("_DetailAlbedoMap", mat_);
            batchInfo_.scale_offset_main_id = AddToDictionary(dict_scaleOffset_,v1);
            batchInfo_.scale_offset_detail_id = AddToDictionary(dict_scaleOffset_,v2);
        }

        public void AddTexDataToDictionaries(Material mat_,Batch_Info batchInfo_)
        {
            
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
        private int GetMeshID(MeshFilter filter,int transform_Index,Dictionary<Mesh,int>mesh_ids_)
        {
            int mesh_id = -1;
            if (mesh_ids_.ContainsKey(filter.sharedMesh))
                mesh_id = mesh_ids_[filter.sharedMesh];
            else
            {
                mesh_id = mesh_ids_.Count;
                mesh_ids_.Add(filter.sharedMesh, mesh_id);
            }
            return mesh_id;
        }
        
  
    }

    [System.Serializable]
    public class Instance_Info
    {
        public int id;
        public int mesh_id;
        public int submesh_id;
        public int transform_id;
        public Instance_Info(ref int instance_counter_,int mesh_id_,int submesh_id_,int transform_id_)
        {
            mesh_id = mesh_id_;
            submesh_id = submesh_id_;
            transform_id = transform_id_;
            id=++instance_counter_;
        }
    }
    [System.Serializable]
    public class Batch_Info
    {
        public int batch_id=-1;
        public int scale_offset_main_id=-1;
        public int scale_offset_detail_id=-1;
        public int color_main_id=-1;
        public int texture_main_id=-1;
        public int texture_normalmap_id=-1;
        public int texture_detail_id=-1;
        public int texture_detail_normalmap_id=-1;
        public Material m_Material;
        public Texture2D tex_main;
        public Texture2D tex_normalmap;
        public Texture2D tex_detail;
        public Texture2D tex_detail_normalmap;
        public Batch_Info(int batch_counter_)
        {
            batch_id=batch_counter_;
        }

        public void AddInfo(Material mat_,out Color mainColor_,out Color emissionColor_)
        {
            mainColor_ = mat_.GetColor("_BaseColor");
            emissionColor_ = mat_.GetColor("_EmissionColor");
            m_Material = mat_;
            Texture2D tex_0 =mat_.GetTexture("_BaseMap") as Texture2D;
            tex_main = tex_0;
            Texture2D tex_1 =mat_.GetTexture("_BumpMap") as Texture2D;  
            tex_normalmap= tex_1;
            Texture2D tex_2 =mat_.GetTexture("_MetallicGlossMap") as Texture2D;  
            Texture2D tex_3 =mat_.GetTexture("_SpecGlossMap") as Texture2D;  
            Texture2D tex_4 =mat_.GetTexture("_DetailAlbedoMap") as Texture2D;  
            tex_detail= tex_4;
            Texture2D tex_5 =mat_.GetTexture("_DetailNormalMap") as Texture2D;
            tex_detail_normalmap = tex_5;
           texture_main_id=AddToDictionary(BatchCreator.tex_main,tex_0);
           texture_normalmap_id=AddToDictionary(BatchCreator.tex_normal_map,tex_1);
            AddToDictionary(BatchCreator.tex_metalic_map,tex_2);
            AddToDictionary(BatchCreator.tex_specular_map,tex_3);
            AddToDictionary(BatchCreator.tex_detail,tex_4);
            texture_detail_normalmap_id=  AddToDictionary(BatchCreator.tex_detail_normal_map,tex_5);
        }

        private int AddToDictionary(Dictionary<Texture2D, int> dict,Texture2D tex_)
        {
            int i = -1;
            if (tex_ != null)
            {
                if(dict.ContainsKey(tex_))
                    i=dict[tex_];
                else
                {
                    i=dict.Count;
                    dict.Add(tex_, i);
                }
            }

            return i;
        }
    }
}
