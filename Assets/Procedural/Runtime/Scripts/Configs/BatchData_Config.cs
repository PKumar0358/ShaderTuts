using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace PRK.Procedural
{
    using Unity.Mathematics;
    public class BatchData_Config : ScriptableObject
    {
        [SerializeField]
        protected Material m_Material;
        [SerializeField]
        protected List<Mesh> m_Meshes;
        protected const int int4_size = sizeof(int) * 4;
        protected const int float4_size = sizeof(float) * 4;
        protected const int matrix_4x4_size = sizeof(float) * 16;
        
        protected GraphicsBuffer instance_info_IDs_Buffer;
        protected GraphicsBuffer _Obj_To_World_Buffer;
        protected GraphicsBuffer _World_To_Obj_Buffer;
        protected GraphicsBuffer _Colors_Buffer;
        protected GraphicsBuffer batch_info_IDs_Buffer_0;
        protected GraphicsBuffer batch_info_IDs_Buffer_1;
        protected GraphicsBuffer _ScaleOffsets_Buffer;
        /// <summary>
        /// //x=Transform id, y= batch id, z=light map id.,w=.scale offset main.
        /// </summary>
       [System.NonSerialized] protected int4[] instance_info_IDs;
       [System.NonSerialized] protected int4[] batch_info_IDs0;
       [System.NonSerialized] protected int4[] batch_info_IDs1;

       [SerializeField,HideInInspector]
       protected List<Vector3> m_Positions;
       [SerializeField,HideInInspector]
       protected List<Vector3> m_Angles;
       [SerializeField,HideInInspector]
       protected List<Vector3> m_Scales;
       [SerializeField,HideInInspector]
       protected List<Instance_Info> m_Instance_Info_Array;
       [SerializeField,HideInInspector]
       protected Batch_Info[] m_Batch_Info_Array;
       [SerializeField,HideInInspector]
       protected Color[]m_Colors_Array;
       [SerializeField,HideInInspector]
       protected Vector4[] m_Scale_Offset_Array;
    }

    [System.Serializable]
    public partial class Batch_Info
    {
        public int batch_id = -1;
        public int scale_offset_main_id = -1;
        public int scale_offset_detail_id = -1;
        public int color_main_id = -1;
        public int texture_main_id = -1;
        public int texture_normalmap_id = -1;
        public int texture_detail_id = -1;
        public int texture_detail_normalmap_id = -1;

        public Material m_Material;
        public Texture2D tex_main;
        public Texture2D tex_normalmap;
        public Texture2D tex_detail;
        public Texture2D tex_detail_normalmap;
    }

    [System.Serializable]
    public partial class Instance_Info
    {
        public int id;
        public int mesh_id;
        public int batch_id;
        public int transform_id;
        public int lightmap_id;
    }
}
