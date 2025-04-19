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
        private void Start()
        {
            Batch_Info batchInfo = new Batch_Info(-1);
            AddScaleOffsetToDictionary(batchInfo);
            Debug.Log(batchInfo.batch_id);
        }

        private void Prepare_BatchData(Transform parent_)
        {
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
                       if (!mesh_submesh_ids.ContainsKey(mesh_id))
                           mesh_submesh_ids.Add(mesh_id,NewIds_List(c2));
                       for (int j = 0; j < c2; j++)
                           instance_info_list.Add(new Instance_Info(ref instance_counter,mesh_id,j,i));
                   }
               }
           }
           
           Dictionary<Vector4, int> scale_offsets=new Dictionary<Vector4, int>();
           Dictionary<Vector4,int>colors=new Dictionary<Vector4,int>();
           int batch_counter = 0;
           foreach (var x in mesh_submesh_ids)
           {
               Batch_Info b_info = new Batch_Info(batch_counter);
               batch_counter++;
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

        private void AddScaleOffsetToDictionary(Batch_Info batchInfo_)//Material mat_,Dictionary<Vector4,int>dict_)
        {
            batchInfo_.batch_id = 0;
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
    public class Batch_Info
    {
        public int batch_id=0;
        public int scale_offset_main_id=0;
        public int scale_offset_detail_id=0;
        public int color_main_id=0;
        public int texture_main_id=-1;
        public int texture_normalmap_id=-1;
        public int texture_detail_id=-1;
        public int texture_detail_normalmap_id=-1;
        public Batch_Info(int batch_counter_)
        {
            batch_id=batch_counter_;
        }
    }
}
