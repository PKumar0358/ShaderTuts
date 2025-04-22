using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PRK.Procedural
{
#if UNITY_EDITOR
    public partial class CombinedMeshBatch_DataConfigBase
    {
        [SerializeField]
        protected List<Mesh> raw_mesh_list;
        [SerializeField]
        protected List<Material> material_list;
        
    }
#endif
    public partial class CombinedMeshBatch_DataConfigBase : ScriptableObject
    {
        [HideInInspector]
        [SerializeField] protected List<Vector3> positionList;
        [HideInInspector]
        [SerializeField] protected List<Vector3> rotationList;
        [HideInInspector]
        [SerializeField] protected List<Vector3> scaleList;

        [SerializeField] protected List<Batch_Tex_IDs> batch_Tex_IDs;
        [SerializeField] protected List<Batch_IDs0> batch_IDs0;

        [SerializeField]
        protected List<Texture2D> m_BaseMaps;
        [SerializeField]
        protected List<Texture2D> m_BumpMaps;
        [SerializeField]
        protected List<Texture2D> m_MetallicGloassMaps;
        [SerializeField]
        protected List<Texture2D> m_SpecGloassMaps;
        [SerializeField]
        protected List<Texture2D> m_DetailAlbedoMaps;
        [SerializeField]
        protected List<Texture2D> m_DetailMasks;
        [SerializeField]
        protected List<Texture2D> m_DetailNormalMaps;
        [SerializeField]
        protected List<Texture2D> m_OcclusionMaps;
        [SerializeField]
        protected List<Texture2D> m_EmissionMaps;
    }

    [System.Serializable]
    public partial class Batch_IDs0
    {
        public int scale_offset_main=-1;
        public int scale_offset_detail=-1;
        public int color_main = -1;
        public int color_emission = -1;
        public int smoothness = -1;
        public int alfaClip = -1;
    }
    [System.Serializable]
    public partial class  Batch_Tex_IDs
    {
        public int tex_main=-1;
        public int tex_normal_map=-1;
        public int tex_metalic_map=-1;
        public int tex_specular_map=-1;
        public int tex_detail_map=-1;
        public int tex_detail_normal_map=-1;
        public int tex_occulussion_map=-1;
        public int tex_detail_mask_map=-1;
        public int tex_emission_map=-1;
    }

    public enum TexNames
    {
        _BaseMap,
        _BumpMap,
        _MetallicGlossMap,
        _SpecGlossMap,
        _DetailAlbedoMap,
        _DetailMask,
        _DetailNormalMap,
        _OcclusionMap,
        _EmissionMap,
    }
}
