using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace PRK.Procedural
{
    #if UNITY_EDITOR
    using UnityEditor;

    public partial class BatchData_Config_Functions
    {
        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
            AssetDatabase.Refresh();
        }
        public void AddData(Mesh[] meshes_,bool save=false)
        {
            m_Meshes = new List<Mesh>(meshes_);
            if (save)
                Save();
        }
        public void AddData(MeshRenderer[] renderers_,bool save=false)
        {
            int c = renderers_.Length;
            m_Positions = new List<Vector3>(c);
            m_Angles= new List<Vector3>(c);
            m_Scales= new List<Vector3>(c);
            for (int i = 0; i < c; i++)
            {
                m_Positions.Add(renderers_[i].transform.position);
                m_Angles.Add(renderers_[i].transform.eulerAngles);
                m_Scales.Add(renderers_[i].transform.lossyScale);
            }
            if (save)
                Save();
        }
        public void AddData(Dictionary<int, List<Instance_Info>>batch_instance_dict,bool save=false)
        {
            m_Instance_Info_Array = new List<Instance_Info>();
            foreach (var x in batch_instance_dict)
                m_Instance_Info_Array.AddRange(x.Value);
            if (save)
                Save();
            
        }
    }
    #endif
    [CreateAssetMenu]
    public partial class BatchData_Config_Functions : BatchData_Config
    {
        private void Initialize_BatchDataBuffers()
        {
            int count = m_Instance_Info_Array.Count;
            int4[] _InstanceInfo_IDs=new int4[count];
            Matrix4x4[] o2w = new Matrix4x4[m_Positions.Count];
            Matrix4x4[] w2o = new Matrix4x4[m_Positions.Count];
            for (int i = 0; i < count; i++)
            {
                Instance_Info ifo = m_Instance_Info_Array[i];
                int trs_id = m_Instance_Info_Array[i].transform_id;
                o2w[trs_id] = Matrix4x4.TRS(m_Positions[trs_id],Quaternion.Euler(m_Angles[trs_id]),m_Scales[trs_id]);
                w2o[trs_id]=o2w[trs_id].inverse;
                _InstanceInfo_IDs[i]= new int4(trs_id,ifo.batch_id,ifo.lightmap_id,m_Batch_Info_Array[ifo.batch_id].scale_offset_main_id);
            }
            

            int count2 = m_Batch_Info_Array.Length;
            int4[]_Batch_Info_IDs0=new int4[count2];
            int4[]_BatchInfo_IDs1=new int4[count2];
        
            for (int i = 0; i < count2; i++)
            {
                Batch_Info b_info = m_Batch_Info_Array[i];
                _Batch_Info_IDs0[i] = new int4(b_info.texture_main_id,b_info.color_main_id,b_info.texture_normalmap_id,0);
                _BatchInfo_IDs1[i] = new int4(b_info.texture_detail_id,b_info.texture_detail_normalmap_id,0,b_info.scale_offset_detail_id);
            }

            instance_info_IDs_Buffer=NewGBuffer(_InstanceInfo_IDs);
            _Obj_To_World_Buffer=NewGBuffer(o2w);
            _World_To_Obj_Buffer=NewGBuffer(w2o);
            batch_info_IDs_Buffer_0=NewGBuffer(_Batch_Info_IDs0);
            batch_info_IDs_Buffer_1=NewGBuffer(_BatchInfo_IDs1);
            _Colors_Buffer=NewGBuffer(m_Colors_Array);
            _ScaleOffsets_Buffer=NewGBuffer(m_Scale_Offset_Array);
        }

        public void UploadMaterialData_Buffers(Material material_)
        {
            material_.SetBuffer("_InstanceInfo_IDs",instance_info_IDs_Buffer);
            material_.SetBuffer("_ObjTo_World_Buffer",_Obj_To_World_Buffer);
            material_.SetBuffer("_WorldTo_Obj_Buffer",_World_To_Obj_Buffer);
            material_.SetBuffer("_Batch_Info_IDs0",batch_info_IDs_Buffer_0);
            material_.SetBuffer("_BatchInfo_IDs1",batch_info_IDs_Buffer_1);
            material_.SetBuffer("_Colors_Buffer",_Colors_Buffer);
            material_.SetBuffer("_ScaleOffsets_Buffer",_ScaleOffsets_Buffer);
        }
        
        public void Dispose()
        {
            DisposeBuffer(instance_info_IDs_Buffer);
            DisposeBuffer(_Obj_To_World_Buffer);
            DisposeBuffer(_World_To_Obj_Buffer);
            DisposeBuffer(batch_info_IDs_Buffer_0);
            DisposeBuffer(batch_info_IDs_Buffer_1);
            DisposeBuffer(_Colors_Buffer);
            DisposeBuffer(_ScaleOffsets_Buffer);
        }
        
       
        private GraphicsBuffer NewGBuffer(Vector4[]array_)
        {
            GraphicsBuffer buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured,array_.Length,float4_size);
            buffer.SetData(array_);
            return buffer;
        }
        private GraphicsBuffer NewGBuffer(Color[]array_)
        {
            GraphicsBuffer buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured,array_.Length,float4_size);
            buffer.SetData(array_);
            return buffer;
        }
        private GraphicsBuffer NewGBuffer(int4[]array_)
        {
            GraphicsBuffer buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured,array_.Length,int4_size);
            buffer.SetData(array_);
            return buffer;
        }
        private GraphicsBuffer NewGBuffer(Matrix4x4[]array_)
        {
            GraphicsBuffer buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured,array_.Length,matrix_4x4_size);
            buffer.SetData(array_);
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
    
    public partial class Instance_Info
    {
        public Instance_Info(ref int instance_counter_,int mesh_id_,int batch_id_,int transform_id_)
        {
            mesh_id = mesh_id_;
            batch_id = batch_id_;
            transform_id = transform_id_;
            id=++instance_counter_;
        }
    }
    public partial class Batch_Info
    {
        public Batch_Info(int batch_counter_)
        {
            batch_id=batch_counter_;
        }

        
        public void AddInfo(Material mat_,out Color mainColor_,out Color emissionColor_)
        {
            m_Material = mat_;
            
            mainColor_ = mat_.GetColor("_BaseColor");
            emissionColor_ = mat_.GetColor("_EmissionColor");
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
